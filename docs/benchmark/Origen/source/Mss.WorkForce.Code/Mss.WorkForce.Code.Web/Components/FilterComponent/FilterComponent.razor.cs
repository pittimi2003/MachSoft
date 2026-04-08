using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using Mss.WorkForce.Code.Web.Services.Interfaces;
using System.Reflection;
using System.Text.Json;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Helper;
using Mss.WorkForce.Code.Web.Services;
using Mss.WorkForce.Code.Models.Common;

namespace Mss.WorkForce.Code.Web.Components.FilterComponent
{
    public partial class FilterComponent : IDisposable
    {
        #region FieldsPivotData

        [Parameter]
        public SiteModel Site { get; set; }

        [Parameter]
        public Guid CurrentPlanning { get; set; }

        [Parameter]
        public bool ShowChart { get; set; }

        private Dictionary<string, string> fieldChooserTexts = new Dictionary<string, string>();

        private IJSObjectReference? _ganttModulePivot;
        private IJSObjectReference? _ganttModuleGantt;

        public bool LoadPanelVisible { get; set; } = false;
        #endregion

        [Inject] private IPivotGridService _pivotGridService { get; set; }

        [Parameter]
        public EventCallback<Guid> LastUpdateGantt { get; set; }

        [Parameter]
        public UserFormatOptions UserFormat { get; set; } = new();

        public const string SUMTIMEORDERDELAY = "Total time order delay";
        public const string SUMACTUALWORKTIME = "Total actual work time";

        #region Methods

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadServiceJS();
                await loadFilterPivot();
            }

        }

        protected override void OnInitialized()
        {
            DisabledEnableLoadPanel(true);

            eventServices.Subscribe(ConstantComponents.PivotData, ConstantComponents.PivotData, EventsToPivorGrid);
            eventServices.Subscribe(ConstantComponents.TopBarComponent, ConstantComponents.TopBarComponent, EventsToPivorGrid);
        }

        private async void EventsToPivorGrid(EventArguments eventData)
        {
            switch (eventData.EventActions)
            {
                case EventActions.PivotDataFilter:
                    await loadColumnsPivot();
                    await LoadWarehouseName();
                    await loadDataPivotGrid();
                    break;

                case EventActions.Update:
                    Site = !string.IsNullOrEmpty(eventData.EventData.ToString()) ? eventData.EventData as SiteModel : Site;
                    await LoadWarehouseName();
                    await reloadDataPivot(Site.Id);
                    await OnReloadPivotGrid();
                    break;
            }

        }

        private async Task LoadServiceJS()
        {
            _ganttModulePivot = await GanttService.GetModulePivotAsync(JS);
            _ganttModuleGantt = await GanttService.GetModuleAsync(JS);
        }

        private async Task loadFilterPivot()
        {
            TranslateFields();
            await _ganttModuleGantt.InvokeVoidAsync("isReadyGantt", false);
            await _ganttModulePivot.InvokeVoidAsync("drawFilterPivot", fieldChooserTexts);
            await LoadWarehouseName();
            await loadColumnsPivot();
            await loadDataPivotGrid();
        }

        private void TranslateFields()
        {
            fieldChooserTexts = new Dictionary<string, string>
            {
                { "allFields", l.Loc("All Fields") },
                { "columnFields", l.Loc("Column Fields") },
                { "dataFields", l.Loc("Data Fields") },
                { "rowFields", l.Loc("Row Fields") },
                { "filterFields", l.Loc("Filter Fields") },
                { "dropFilterFieldsHere", l.Loc("Drop Filter Fields Here") },
                { "showFieldChooser", l.Loc("Show Field Chooser") },
                { "fieldChooserTitle", l.Loc("Field Chooser") },
                { "grandTotal", l.Loc("Grand Total") }
            };
        }

        private async Task loadColumnsPivot()
        {
            var columns = GetGanttColumns<PivotTaskData>(l);
            string data = JsonSerializer.Serialize(columns);
            await JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.loadColumnsDefault.ToString(), data);
        }

        private async Task LoadDataPivot(string data) => await JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.loadDataForPivot.ToString(), data);

        private async Task OnReloadPivotGrid() => await JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.reloadPivotGrid.ToString());



        public static List<ColumnsPivot> GetGanttColumns<T>(ILocalizationService l)
        {
            try
            {
                var columns = new List<ColumnsPivot>();
                var props = typeof(T).GetProperties();
                foreach (var prop in props)
                {
                    var attr = prop.GetCustomAttribute<DisplayAttributes>();
                    if (attr != null && !string.IsNullOrEmpty(attr.Caption))
                    {
                        string captionText = prop.Name switch
                        {
                            nameof(PivotTaskData.SumTimeOrderDelay) => l.Loc(SUMTIMEORDERDELAY),
                            nameof(PivotTaskData.SumActualWorkTime) => l.Loc(SUMACTUALWORKTIME),
                            _ => l.Loc(attr.Caption)
                        };

                        columns.Add(new ColumnsPivot
                        {
                            dataField = prop.Name,
                            caption = captionText,
                            index = attr.Index,
                        });
                    }
                }

                return columns.OrderBy(c => c.index).ToList();
            }
            catch (Exception)
            {
                return new List<ColumnsPivot> { };
            }

        }

        private async Task reloadDataPivot(Guid warehouseId)
        {
            DisabledEnableLoadPanel(true);
            var dataPlanning = await _pivotGridService.GetPlanningForWarehouse(warehouseId);
            var data = PivotConverter.ConverterDataPlanning(dataPlanning);
            await LoadDataJsonPivot(data);
            await LastUpdateGantt.InvokeAsync(CurrentPlanning);
            DisabledEnableLoadPanel(false);
        }

        private async Task loadDataPivotGrid()
        {
            DisabledEnableLoadPanel(true);
            var data = new List<PivotTaskData>();
            if (CurrentPlanning != Guid.Empty)
            {
                var dataPlanning = _pivotGridService.GetPlanningData(CurrentPlanning);
                data = PivotConverter.ConverterDataPlanning(dataPlanning);
                foreach (var d in data)
                {
                    d.ActivityTitle = l.Loc(d.ActivityTitle);
                    d.DelayedOrder = l.Loc(d.DelayedOrder);
                    d.Flow = l.Loc(d.Flow);
                    d.OrderStatus = l.Loc(d.OrderStatus);
                    d.Priority = l.Loc(d.Priority);
                }

            }
            await LoadDataJsonPivot(data);
            DisabledEnableLoadPanel(false);
        }

        private async Task LoadDataJsonPivot(List<PivotTaskData> data)
        {
            string jsonData = JsonSerializer.Serialize(data);
            await LoadDataPivot(jsonData);
            await DrawChartAndPivotAsync(eChartType.bar);
        }

        private async Task LoadWarehouseName() => await JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.loadWarehouseName.ToString(), Site.Name);

        private void DisabledEnableLoadPanel(bool isActive)
        {
            LoadPanelVisible = isActive;
            StateHasChanged();
        }

        private async Task ClosePopupColumnChooser() => await JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.ClosePopupColumnChooser.ToString());

        public async Task DrawChartAndPivotAsync(eChartType chartType) => await JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.drawChartAndPivotTogether.ToString(), chartType.ToString());

        public void Dispose()
        {
            ClosePopupColumnChooser();
            eventServices.Unsubscribe(ConstantComponents.PivotData, ConstantComponents.PivotData, EventsToPivorGrid);
            eventServices.Unsubscribe(ConstantComponents.TopBarComponent, ConstantComponents.TopBarComponent, EventsToPivorGrid);
        }

        #endregion
    }
}
