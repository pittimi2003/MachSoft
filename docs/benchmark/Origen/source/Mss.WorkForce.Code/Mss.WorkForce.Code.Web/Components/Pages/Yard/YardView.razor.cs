using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.Gantt;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Components.Pages.Yard
{
    public partial class YardView : GanttOperations
    {

        #region Fields
        private YardResourceType selectedResourceGroup = YardResourceType.Docks;
        private int _hoursToShow;
        private bool LoadPanelVisibleForScheduler = false;

        GanttDataConvertDto<YardTaskGantt> DataSimulation = new();
        List<YardTaskGantt> GanttReadyData = new();
        public GanttComponent<YardTaskGantt> GanttReference;
        public GetAttributesDto<YardTaskGantt>? GridParameters { get; set; }
        public Guid CurrentPlanning { get; set; } = Guid.Empty;
        private DataTotalStages metrics { get; set; }

        #endregion

        #region Properties
        [Inject] private IInitialDataService _InitialDataService { get; set; }
        [Inject] private IMlxDialogService DialogService { get; set; }
        private DateTimeOffset LastUpdateScheduler { get; set; }
        public List<YardResourceBase> ResourcesList { get; set; } = new();

        [Inject]
        public IScheduleDataService ScheduleDataService { get; set; }

        #endregion

        #region Methods
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                GanttSetup.ChooserColumnDefault = true;
                await LoadGanttServiceAsync();
                await LoadStripLine();
                await SavedFilter(GanttView.YardView);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await GetUserDataAsync();
            GridParameters = new(l);
            GanttSetup.typeGantt = GanttView.YardView;
            ShowHidePanel(true, true);
            GanttProperties = GridParameters.GetTranslateCaptions();
            _EventServices.Subscribe(ConstantComponents.GanttSimulation, ConstantComponents.GanttSimulation, SettingsActions);
            _EventServices.Subscribe(ConstantComponents.GanttSimulation, EventActions.ApplyFilterTimeInterval.ToString(), SettingsActions);
        }

		public override async Task LoadDataSimulation()
		{
            IsDisabledTopBarButtons = true;
            var planningId = await _SimulateService.ExecutePlanningSimulation(GanttSetup.WarehouseId, SimulationCase.Yard);
			planningId = IsPlnanningInWarehouse(planningId, GanttSetup.WarehouseId);
			var data = _SimulateService.GetYardsByPlanningId(planningId, userFormat);
			GanttReadyData = CloneGanttList(data.TaskGantt);
			var dataFilters = await applyFilter(data);
			DataSimulation = dataFilters;
			DataSimulation.PlanningId = planningId;
			TraslateProperties(DataSimulation.TaskGantt);
			await base.LoadDataSimulation(planningId);
		}

        public override void LoadGanttSetupData(EnumViewPlanning newView)
        {
            GanttSetup.WarehouseId = CurrentSite.Id;
            GanttSetup.typeGantt = GanttView.YardView;
            GanttSetup.Offset = CurrentSite.Offset;
            userFormat.TimeZoneOffSet = GanttSetup.Offset;
        }

        public override async Task SuscribeEvents() => await ReloadScheduler();

        private void OnHoursChanged(int h) => _hoursToShow = h;

        private Int16 zoomLevel = 60;

        private Task OnZoomChanged(Int16 newZoom)
        {
            zoomLevel = newZoom;
            return Task.CompletedTask;
        }

        private void HandleResourceGroupSelection(YardResourceType resourceGroupType)
        {
            selectedResourceGroup = resourceGroupType;
            LoadSchedulerResourcesWithOutSimulate();
        }

        private async Task ReloadScheduler()
        {
            LoadPanelVisibleForScheduler = true;
            await SimulateLoading();
            LoadSchedulerResources();
            LoadPanelVisibleForScheduler = false;
        }

        private void LoadSchedulerResources()
        {
            ResourcesList = ScheduleDataService.GetSaturations(CurrentPlanning, selectedResourceGroup, CurrentSite.Offset).ToList();
            LastUpdateScheduler = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(CurrentSite.Offset));
            CalculateMetricsFromResources();
        }

        private void LoadSchedulerResourcesWithOutSimulate()
        {
            ResourcesList = ScheduleDataService.GetSaturations(CurrentPlanning, selectedResourceGroup, CurrentSite.Offset).ToList();
            CalculateMetricsFromResources();
        }

        private void CalculateMetricsFromResources()
        {
            var allAppointments = ResourcesList
                .Where(r => r.Appointments != null)
                .SelectMany(r => r.Appointments)
                .ToList();

            metrics = new DataTotalStages(userFormat)
            {
                TS = allAppointments.Any() ? allAppointments.Average(a => a.TotalSaturation) : 0,
                AU = allAppointments.Any() ? allAppointments.Average(a => a.ActualUtilization) : 0,
                PU = allAppointments.Any() ? allAppointments.Average(a => a.PlannedUtilization) : 0,
                TC = allAppointments.Any() ? allAppointments.Average(a => a.TotalCapacity) : 0
            };
        }

        private async Task SimulateLoading()
        {
            CurrentPlanning = await _SimulateService.ExecutePlanningSimulation(CurrentSite.Id, SimulationCase.Yard);
        }

        public virtual async Task ClearFilter(eFilterType filterType)
        {
            bool resp = await DialogService.ShowDialogAsync("NOTIFICATION", $"Are you sure you want to delete the filter?");
            if (!resp) return;

            switch (filterType)
            {
                case eFilterType.YardResource:
                    await _InitialDataService.ClearGanttFilterAsync(GanttView.YardView);
                    FilterSummaryTextYard = string.Empty;
                    break;
            }
            GanttSetup.ChooserColumnDefault = false;
            GanttSetup.ReloadData = true;
            DataSimulation.TaskGantt = GanttReadyData;
            StateHasChanged();
        }

        public override async void ApplyFilterTimeInterval()
        {
            var data = await applyFilter(DataSimulation);
            DataSimulation.TaskGantt = data.TaskGantt;
            GanttSetup.ReloadData = true;
            StateHasChanged();
        }

        private async Task LoadStripLine() => await _ganttModule.InvokeVoidAsync("showToltip", true);
        private async Task GetStripLine() => await _ganttModule.InvokeVoidAsync("getStripLine", GanttSetup.Offset);

        public override void Dispose()
        {
            _EventServices.Unsubscribe(ConstantComponents.GanttSimulation, EventActions.ApplyFilterTimeInterval.ToString(), SettingsActions);
            _EventServices.Unsubscribe(ConstantComponents.GanttSimulation, ConstantComponents.GanttSimulation, SettingsActions);

            base.Dispose();
        }

        #endregion
    }
}
