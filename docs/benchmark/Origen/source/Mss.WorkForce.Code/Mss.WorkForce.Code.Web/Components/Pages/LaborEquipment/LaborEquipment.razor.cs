using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.Gantt;

namespace Mss.WorkForce.Code.Web.Components.Pages.LaborEquipment
{
    public partial class LaborEquipment : GanttOperations
    {
        #region Fields

        GanttDataConvertDto<LaborEquipmentGantt> DataSimulation = new();

        List<LaborEquipmentGantt> GanttReadyData = new();
        public GetAttributesDto<LaborEquipmentGantt>? GridParameters { get; set; }

        public GanttComponent<LaborEquipmentGantt> GanttReference;

        #endregion

        #region Methods

        protected override void OnInitialized()
        {
            GridParameters = new(l);
            GanttSetup.typeGantt = GanttView.LaborEquipment;
            ShowHidePanel(true, true);
            GanttProperties = GridParameters.GetTranslateCaptions();
            _EventServices.Subscribe(ConstantComponents.GanttSimulation, ConstantComponents.GanttSimulation, SettingsActions);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadGanttServiceAsync();
                GanttSetup.ChooserColumnDefault = true;
                await GetUserDataAsync();
            }
        }

        public override async Task LoadDataSimulation()
        {
            IsDisabledTopBarButtons = true;
            var planningId = await _SimulateService.ExecutePlanningSimulation(GanttSetup.WarehouseId, SimulationCase.Labor);
            planningId = IsPlnanningInWarehouse(planningId, GanttSetup.WarehouseId);
            var data = _SimulateService.GetLaborEquipmentsByPlanningId(planningId, userFormat);
            GanttReadyData = CloneGanttList(data.TaskGantt);
            DataSimulation = data;
            DataSimulation.PlanningId = planningId;
            await base.LoadDataSimulation(planningId);
        }

        public override void LoadGanttSetupData(EnumViewPlanning newView)
        {
            GanttSetup.WarehouseId = CurrentSite.Id;
            GanttSetup.Offset = CurrentSite.Offset;
            GanttSetup.typeGantt = GanttView.LaborEquipment;
            userFormat.TimeZoneOffSet = GanttSetup.Offset;
        }

        public override async Task LoadGanttServiceAsync() => await base.LoadGanttServiceAsync();

        private async Task LoadStripLine() => await _ganttModule.InvokeVoidAsync("showToltip", true);
        private async Task GetStripLine() => await _ganttModule.InvokeVoidAsync("getStripLine", GanttSetup.Offset);

        public override void Dispose()
        {
            _EventServices.Unsubscribe(ConstantComponents.GanttSimulation, ConstantComponents.GanttSimulation, SettingsActions);
            base.Dispose();
        }

        #endregion
    }
}
