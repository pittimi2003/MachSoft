using System.Threading.Tasks;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.Gantt;
using Mss.WorkForce.Code.Web.Components.Shared;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Components.Pages.LaborManagement
{
    public partial class LaborManagement : GanttOperations
    {
        #region Fields
        private DotNetObjectReference<LaborManagement> _dotNetHelper;
        private List<SelectDateFieldItem> EnumDateFields = new();
        private TopBar? TopBarRef;
        private DotNetObjectReference<TopBar>? _dotNetHelperTopBar;

        public GanttDataConvertDto<LaborTaskGantt> DataSimulation = new();
        public List<LaborTaskGantt> GanttReadyData = new();
        public GanttComponent<LaborTaskGantt> GanttReference;
        public GetAttributesDto<LaborTaskGantt>? GridParameters { get; set; }
        #endregion

        #region Methods
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadGanttServiceAsync();
                GanttSetup.ChooserColumnDefault = true;
            }

            await RepaintGanttAsync(true, ShowInfoDrawer);
        }

        protected override void OnInitialized()
        {
            GetUserDataAsync();
            GridParameters = new(l);
            GanttSetup.typeGantt = GanttView.LaborManagement;
            LoadSites();
            ShowHidePanel(true, true);
            GanttProperties = GridParameters.GetTranslateCaptions();
            _EventServices.Subscribe(ConstantComponents.GanttSimulation, ConstantComponents.GanttSimulation, SettingsActions);
        }

        private void LoadSites()
        {
            EnumDateFields = Enum.GetValues(typeof(EnumDateField))
            .Cast<EnumDateField>()
            .Select(e => new SelectDateFieldItem
            {
                Name = EnumHelper.GetItemDescription(e),
                Value = e
            })
            .ToList();
        }

        public override async Task LoadDataSimulation()
        {
            IsDisabledTopBarButtons = true;
            var planningId = await _SimulateService.ExecutePlanningSimulation(GanttSetup.WarehouseId, SimulationCase.Labor);
            planningId = IsPlnanningInWarehouse(planningId, GanttSetup.WarehouseId);
            var data = _SimulateService.GetLaborTasksByPlanningId(planningId, userFormat);
            GanttReadyData = CloneGanttList(data.TaskGantt);
            DataSimulation = data;
            DataSimulation.PlanningId = planningId;
            await base.LoadDataSimulation(planningId);
        }

        public override void LoadGanttSetupData(EnumViewPlanning newView)
        {
            GanttSetup.WarehouseId = CurrentSite.Id;
            GanttSetup.typeGantt = GanttView.LaborManagement;
            GanttSetup.Offset = CurrentSite.Offset;
            GanttSetup.SelectedView = newView;
            userFormat.TimeZoneOffSet = GanttSetup.Offset;
        }

        public override async Task LoadGanttServiceAsync()
        {
            await GanttModulo();

            _dotNetHelper = DotNetObjectReference.Create(this);
            await _ganttModule.InvokeVoidAsync("getDoNetHelper", _dotNetHelper);

            if (TopBarRef is not null)
            {
                _dotNetHelperTopBar = DotNetObjectReference.Create(TopBarRef);
                await _ganttModule.InvokeVoidAsync("registerAlertInterop", _dotNetHelperTopBar);
            }
        }
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
