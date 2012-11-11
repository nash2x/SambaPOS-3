﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Infrastructure.Data.Serializer;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule.Widgets.ResourceGrid
{
    public class ResourceGridWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        public ResourceGridWidgetViewModel(Widget model, IApplicationState applicationState,
            IResourceService resourceService, IUserService userService, ICacheService cacheService)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            ResourceSelectorViewModel = new ResourceSelectorViewModel(applicationState, resourceService, userService, cacheService);
        }

        readonly EntityOperationRequest<Resource> _request = new EntityOperationRequest<Resource>(null, EventTopicNames.ResourceSelected);


        [Browsable(false)]
        public ResourceGridWidgetSettings Settings { get { return SettingsObject as ResourceGridWidgetSettings; } }
        [Browsable(false)]
        public ResourceSelectorViewModel ResourceSelectorViewModel { get; private set; }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<ResourceGridWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            _resourceScreen = null;
            Settings.StateFilterNameValue.UpdateValues(_cacheService.GetResourceStates().Select(x => x.Name));
        }

        private ResourceScreen _resourceScreen;
        public ResourceScreen ResourceScreen
        {
            get { return _resourceScreen ?? (_resourceScreen = CloneResourceScreen()); }
        }

        private ResourceScreen CloneResourceScreen()
        {
            var result = ObjectCloner.EntityClone(_applicationState.SelectedResourceScreen);
            result.RowCount = Settings.Rows;
            result.ColumnCount = Settings.Columns;
            result.PageCount = Settings.PageCount > 0 ? Settings.PageCount : 1;
            result.FontSize = Settings.FontSize;
            return result;
        }

        public override void Refresh()
        {
            var stateFilter = _cacheService.GetResourceStateByName(Settings.StateFilterName);
            var stateFilterId = stateFilter != null ? stateFilter.Id : ResourceScreen.StateFilterId;
            ResourceSelectorViewModel.Refresh(ResourceScreen, stateFilterId, _request);
        }
    }
}