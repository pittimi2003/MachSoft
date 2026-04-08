using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Web.Model;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Web.Helper;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Models.Common;
using System.Text.Json;

namespace Mss.WorkForce.Code.Web.Components.Shared
{
    public partial class TopBar
    {
        #region Fields
        public List<ColumnChoose> ColumnChooseProperties { get; set; } = new();
        public string viewSelector = "Order";
        private IJSObjectReference? _ganttFileJS;
        private bool ShowModalSettings { get; set; } = false;
        private bool showColumnChoose = false;
        private List<ColumnChoose>? _originalColumnChooseProperties;
        private bool _initialOrderApplied = false;
        private List<ColumnChoose> updateColumnChoose { get; set; }
        private List<ColumnChoose> _backupColumnChooseBeforeEdit = new();
        private bool chartVisible = true;

        // For filter time interval
        private bool isTimeIntervalModalVisible = false;
        private GanttFilterSettingsDto TimeIntervalModel { get; set; } = new();
        private GanttFilterSettingsDto TimeIntervalLocalStorage { get; set; }
        private List<SelectItemEnum> dateFieldFilters;

        #endregion

        #region Fields txt

        private string CaptionSelector = "View selector";

        private string CaptionReload = "Reload";

        private string CaptionRevert = "Revert";

        private string CaptionColumnselector = "Column chooser";

        private string CaptionRevertColumns = "Revert columns";

        private string CaptionCollapsed = "Collapsed";

        private string CaptionExpanded = "Expanded";

        private string CaptionZoomOut = "Zoom out";

        private string CaptionZoomIn = "Zoom in";

        private string CaptionGotoNow = "Go to now";

        private string CaptionTimeInterval = "Time interval";

        private string CaptionExport = "Export";

        private string CaptionHideChart = "Hide chart";

        private string CaptionShowChart = "Show chart";

        private string CaptionOrder = EnumHelper.GetItemDescription(EnumViewPlanning.Order);

        private string CaptionPriority = EnumHelper.GetItemDescription(EnumViewPlanning.Priority);

        private string CaptionTrailer = EnumHelper.GetItemDescription(EnumViewPlanning.Trailer);

        #endregion

        #region Parameters

        [Parameter]
        public bool IsSwitchOn { get; set; } = false;

        [Parameter]
        public bool AddIntervalTime { get; set; } = false;

        [Parameter]
        public GanttSetup gantt { get; set; } = new();

        [Parameter]
        public EventCallback<bool> OnToggleChart { get; set; }

        [Parameter]
        public bool ShowPrioritySecction { get; set; }

        [Parameter]
        public DateTimeOffset LastUpdateGantt { get; set; }

        [Parameter]
        public DateTimeOffset LastUpdateGrid { get; set; }

        [Parameter]
        public bool ActiveButtos { get; set; } = false;

        [Parameter]
        public UserFormatOptions UserFormat { get; set; } = new();

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            var allFieldFilterItems = EnumHelper.GetSelectItems<EnumDateField>().Select(x => new SelectItemEnum { Value = x.Value, Text = l.Loc(x.Text)}).ToList();

            if (gantt.typeGantt == GanttView.Planning)
                dateFieldFilters = allFieldFilterItems
                    .Where(x => (EnumDateField)x.Value != EnumDateField.ArrivalDate)
                    .ToList();
            else if (gantt.typeGantt == GanttView.YardView)
                dateFieldFilters = allFieldFilterItems
                    .Where(x => (EnumDateField)x.Value != EnumDateField.CommittedDate)
                    .ToList();
            else
                dateFieldFilters = allFieldFilterItems;

            ColumnChooseProperties = gantt.ColumnChooseProperties;
            eventServices.Subscribe(ConstantComponents.GanttComponent, ConstantComponents.GanttComponent, EventsTopBar);
            Translate();
        }

        private void Translate()
        {
            viewSelector = l.Loc(viewSelector);
            CaptionSelector = l.Loc("View selector");
            CaptionReload = l.Loc("Reload");
            CaptionRevert = l.Loc("Revert");
            CaptionColumnselector = l.Loc("Column chooser");
            CaptionRevertColumns = l.Loc("Revert columns");
            CaptionCollapsed = l.Loc("Collapsed");
            CaptionExpanded = l.Loc("Expanded");
            CaptionZoomOut = l.Loc("Zoom out");
            CaptionZoomIn = l.Loc("Zoom in");
            CaptionGotoNow = l.Loc("Go to now");
            CaptionTimeInterval = l.Loc("Time interval");
            CaptionExport = l.Loc("Export");
            CaptionHideChart = l.Loc("Hide chart");
            CaptionShowChart = l.Loc("Show chart");
            CaptionOrder = l.Loc(EnumHelper.GetItemDescription(EnumViewPlanning.Order));
            CaptionPriority = l.Loc(EnumHelper.GetItemDescription(EnumViewPlanning.Priority));
            CaptionTrailer = l.Loc(EnumHelper.GetItemDescription(EnumViewPlanning.Trailer));
        }

        private List<ColumnChoose> CloneColumnList(List<ColumnChoose> source)
        {
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<List<ColumnChoose>>(json);
        }

        private async Task OnChangeChartType(eChartType chartType) => await JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.drawChartAndPivotTogether.ToString(), chartType.ToString());

        private void EventsTopBar(EventArguments eventData)
        {
            switch (eventData.EventActions)
            {
                case EventActions.ChooseColumns:
                    if (updateColumnChoose == null)
                        updateColumnChoose = CloneColumnList(((List<ColumnChoose>)((dynamic)eventData.EventData).ColumnChooseProperties));
                    ColumnChooseProperties = ((dynamic)eventData.EventData).columnChooses;
                    break;
                case EventActions.UpdateColumns:
                    ColumnChooseProperties = (List<ColumnChoose>)eventData.EventData;
                    break;
            }
        }

        private void CloseModal(bool closed) => ShowModalSettings = !closed;

        public async Task UpdateColumnChoose()
        {
            try
            {
                var newOrder = await JS.InvokeAsync<List<string>>(eSiteMethods.getReorderedFieldNames.ToString());
                if (newOrder != null && newOrder.Any())
                {
                    int index = 3;
                    foreach (var fieldName in newOrder)
                    {
                        var column = ColumnChooseProperties?.FirstOrDefault(c => c.FieldName == fieldName);
                        if (column != null && column.IsVisible)
                        {
                            column.Index = index;
                            index++;
                        }
                    }
                }
                var missingColumns = updateColumnChoose
                .Where(c => !ColumnChooseProperties.Any(x => x.FieldName == c.FieldName))
                .ToList();
                ColumnChooseProperties.AddRange(missingColumns);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            showColumnChoose = false;
            eventServices.Publish(ConstantComponents.TopBarComponent, new EventArguments(ConstantComponents.TopBarComponent, EventActions.UpdateColumns, ColumnChooseProperties));
        }

        private void CancelColumnChoose()
        {
            ColumnChooseProperties = CloneColumnList(_backupColumnChooseBeforeEdit);
            gantt.ColumnChooseProperties = CloneColumnList(_backupColumnChooseBeforeEdit);
            showColumnChoose = false;
            StateHasChanged();
        }

        private async Task ShowTimeIntervalModal()
        {
            TimeIntervalLocalStorage = await initialDataService.GetGanttFilterForViewAsync(gantt.typeGantt);

            if (TimeIntervalLocalStorage != null)
            {
                TimeIntervalModel.Reference = TimeIntervalLocalStorage.Reference;
                TimeIntervalModel.TimeUntil = TimeIntervalLocalStorage.TimeUntil;
            }
            else
                TimeIntervalModel = new();

            isTimeIntervalModalVisible = true;
        }

        private async Task ToggleChart()
        {
            chartVisible = !chartVisible;
            await OnToggleChart.InvokeAsync(chartVisible);
        }

        private async Task TimeIntervalApplyFilter()
        {
            await initialDataService.SaveGanttFilterForViewAsync(TimeIntervalModel, gantt.typeGantt);
            isTimeIntervalModalVisible = false;
            StateHasChanged();
            eventServices.Publish(ConstantComponents.GanttSimulation, new EventArguments(ConstantComponents.GanttSimulation, EventActions.ApplyFilterTimeInterval, TimeIntervalModel));
        }

        private void OnCloseTimeIntervalModal()
        {
            TimeIntervalModel = new();
            isTimeIntervalModalVisible = false;
        }

        private async Task OnGanttEventClick(EventActions action, object value)
        {
            switch (action)
            {
                case EventActions.Update:
                    eventServices.Publish(ConstantComponents.GanttSimulation, new EventArguments(ConstantComponents.GanttSimulation, action, value));
                    return;

                case EventActions.ChooseColumns:
                    _backupColumnChooseBeforeEdit = CloneColumnList(gantt.ColumnChooseProperties);
                    showColumnChoose = true;
                    await Task.Delay(100);
                    break;

                case EventActions.ApplyFilterTimeInterval:
                    await initialDataService.SaveGanttFilterForViewAsync(TimeIntervalModel, gantt.typeGantt);
                    isTimeIntervalModalVisible = false;
                    StateHasChanged();
                    break;

                case EventActions.ChangeView:
                    viewSelector = l.Loc(value.ToString());
                    eventServices.Publish(ConstantComponents.GanttSimulation, new EventArguments(ConstantComponents.GanttSimulation, action, value));
                    
                    return;

                case EventActions.ZoomOutZoomIn:
                    await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.zoom.ToString(), value);
                    return;

                case EventActions.ExportPDF:
                    await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.exportGanttToPdf.ToString(), ConstantComponents.GanttComponent);
                    return;

				case EventActions.ExportCSV:
					await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.exportGanttToCsv.ToString(), UserFormat.CultureCode);
					return;

                case EventActions.Print:
                    await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.printGantt.ToString());
                    return;

                case EventActions.ToNow:
                    await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.updateToDay.ToString());
                    return;
                case EventActions.CollapsedExpanded:
                case EventActions.RevertColumns:
                    break;

            }
            eventServices.Publish(ConstantComponents.TopBarComponent, new EventArguments(ConstantComponents.TopBarComponent, action, value));
        }

        private void OnPivotEventClick(EventActions action, object value)
        {
            switch (action)
            {
                case EventActions.UpdateScheduleGrid:
                    eventServices.Publish(ConstantComponents.TopBarComponent, new EventArguments(ConstantComponents.TopBarComponent, action, value));
                    break;
                case EventActions.PivotColumns:
                    JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.resetPivot.ToString(), value);
                    break;
                case EventActions.CollapsedExpanded:
                    JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.collapsedExpanded.ToString(), value);
                    break;
                case EventActions.ExportCSV:
                    JS.CallJs(ConstantComponents.PivotComponent, eGanttMethods.exportPivotToCsv.ToString(), value);
                    break;
            }
        }

        public void Dispose()
        {
            eventServices.Unsubscribe(ConstantComponents.GanttComponent, ConstantComponents.GanttComponent, EventsTopBar);
        }

        public bool IsButtonActive() => true;
        #endregion
    }
}
