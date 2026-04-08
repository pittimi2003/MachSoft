using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.SchedulerComponent;
using Mss.WorkForce.Code.Web.Helper;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;
using Mss.WorkForce.Code.WMSSimulator.Helper;

namespace Mss.WorkForce.Code.Web.Components.Gantt
{
    public partial class GanttComponent<TModel> : ComponentBase, IDisposable where TModel : GanttTaskBase
    {
        #region Fields

        /// <summary>
        /// Muestra el load mientras el Gantt termina de cargar
        /// </summary>
        [Parameter]
        public bool LoadPanelVisible { get; set; }

        [Parameter]
        public bool BackgroundShading { get; set; }

        [Parameter]
        public TModel Model { get; set; }

        [Parameter]
        public GanttSetup GanttSetup { get; set; } = new();

        [Parameter]
        public GanttDataConvertDto<TModel> GanttTasksData { get; set; }

        [Parameter]
        public UserFormatOptions UserFormat { get; set; } = new();

        [Parameter]
        public Dictionary<string, DisplayAttributes> Properties { get; set; }

        [Inject] private ISignalRClientService SignalRClientService { get; set; }

        [Inject] private ILogger<GanttComponent<TModel>> _logger { get; set; }
        private Dictionary<string, DisplayAttributes>? FillProperties = new Dictionary<string, DisplayAttributes>();
        public IJSObjectReference? _ganttFileJS;

        [Inject] private ProtectedLocalStorage LocalStorage { get; set; }

        private YardResourceFilter? YardFilter { get; set; }

        private Dictionary<string, string> formats = new Dictionary<string, string>();

        #endregion

        #region Init
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                if (firstRender)
                {
                    // Obtenemos el criterio de filtro para filtrar por dock y stage las ordenes
                    var resultYardResourceFilter = await LocalStorage.GetAsync<YardResourceFilter?>(StorageKeysConstants.YardResourceFilter);

                    if (resultYardResourceFilter.Success)
                        YardFilter = resultYardResourceFilter.Value;
                    else
                        YardFilter = null;
                }

                if (GanttSetup.ReloadSimulation)
                    await ReloadGantt(GanttSetup);

                if (GanttSetup.ReloadColumnsView)
                    await UpdateColumnChoose(GanttSetup.ColumnChooseProperties);

                if (GanttSetup.ReloadData)
                    await ReloadDataSourceGantt();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error (OnAfterRenderAsync)", ex.Message);
            }
        }

        protected override void OnInitialized()
        {
            LoadGanttService();
            _eventServices.Subscribe(nameof(MlxSchedulerOptions), nameof(MlxSchedulerOptions), EventsTopBar);
            _eventServices.Subscribe(ConstantComponents.TopBarComponent, ConstantComponents.TopBarComponent, EventsTopBar);
        }

        protected override async Task OnInitializedAsync()
        {
            await SignalRClientService.SuscribeToReloadGantt(OnReloadGantt);
            await SignalRClientService.InitConnectionAsync();
        }

        private async Task OnReloadGantt(GanttView ganttView)
        {
            try
            {
                if (GanttSetup.typeGantt == ganttView)
                {
                    await InvokeAsync(async () =>
                    {
                        await ReloadGantt(GanttSetup);
                        await _ganttFileJS.InvokeVoidAsync("expandGanttToLevel");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OnReloadGantt] Error: {ex.Message}");
            }
        }

        private async Task LoadGanttService()
        {
            if (_ganttFileJS == null)
                _ganttFileJS = await GanttService.GetModuleAsync(JS);
        }

        #endregion

        #region Load Events

        private async void EventsTopBar(EventArguments eventData)
        {
            try
            {
                switch (eventData.EventActions)
                {
                    case EventActions.RevertColumns:
                        await RevertColumnsAndFilters(eventData.EventData as List<ColumnChoose>);
                        break;
                    case EventActions.UpdateColumns:
                        await UpdateColumnChoose(eventData.EventData as List<ColumnChoose>);
                        break;
                    case EventActions.ChooseColumns:
                        LoadColumnChoose(eventData.EventData as List<ColumnChoose>);
                        break;
                    case EventActions.Cancel:
                        if (eventData.EventData is List<ColumnChoose> columnsChoose)
                            CancelChooseColumns(columnsChoose);
                        break;

                    case EventActions.CollapsedExpanded:
                        await SetPanelAndBackground(true, false);
                        await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.collapsedExpandedClick.ToString(), (bool)eventData.EventData);
                        await SetPanelAndBackground(false, false);
                        break;
                }

            }
            catch (Exception e)
            {
                _logger.LogWarning("Error: ", e.Message);
            }
        }

        private async Task CollapsedGantt(bool updateColumns)
        {
            if (updateColumns)
                await _ganttFileJS.InvokeVoidAsync("expandGanttToLevel");
        }

        #endregion

        #region Load Data       

        private async Task ReloadDataSourceGantt()
        {
            await Task.Yield();
            try
            {
                GanttSetup.ReloadData = false;
                TranslateGanttData(GanttTasksData);
                await DeleteFilterColumns(ShouldApplyInitialFilters(), false);
                await LoadDataGanttJS(GanttTasksData);

                await ApplyInitialFiltersAsync();
            }
            finally
            {
                await SetPanelAndBackground(false, false);
            }
        }

        private async Task LoadDataGanttJS<TModel>(GanttDataConvertDto<TModel> data) where TModel : GanttTaskBase
        {
            GetFormats();
            await LoadStripLine();
            string jsonData = JsonSerializer.Serialize(data);

            if (data == null)
            {
                bool LoadChildTasks2 = GanttSetup.typeGantt != GanttView.Planning;
                await _ganttFileJS.InvokeVoidAsync("UpdateToDay");
                bool result2 = await _ganttFileJS.InvokeAsync<bool>("loadDataForGantt", jsonData, formats, LoadChildTasks2);
                return;
            }

            // Carga de hijos
            int taskCount = data.TaskGantt.Count;
            bool hasManyTasks = taskCount > 10; //En caso de ser necesario validar por tamaño se tomara esta bandera
            bool LoadChildTasks = GanttSetup.typeGantt != GanttView.Planning;
            await _ganttFileJS.InvokeVoidAsync("UpdateToDay");
            bool result = await _ganttFileJS.InvokeAsync<bool>("loadDataForGantt", jsonData, formats, LoadChildTasks);

            await Task.Delay(2000);
            if (result)
                await CollapsedGantt(true);
        }

        private void GetFormats()
        {
            formats = new Dictionary<string, string>
            {
                { "hourFormat", UserFormat.HourFormatWithoutSeconds },
                { "regionFormat", UserFormat.CultureCode },
                { "dateFormat", UserFormat.DateFormat },
                { "hourFormatComplete", UserFormat.HourFormat }
            };
        }

        private void TranslateGanttData<TModel>(GanttDataConvertDto<TModel> data) where TModel : GanttTaskBase
        {
            foreach (var task in data.TaskGantt)
            {
                if (task is TaskData taskData)
                {
                    if (!string.IsNullOrEmpty(taskData.IDCode))
                        taskData.IDCode = L.Loc(taskData.IDCode);

                    if (!string.IsNullOrEmpty(taskData.SLAMet))
                        taskData.SLAMet = L.Loc(taskData.SLAMet);

                    if (!string.IsNullOrEmpty(taskData.WorkFlow))
                        taskData.WorkFlow = L.Loc(taskData.WorkFlow);

                    if (!string.IsNullOrEmpty(taskData.Priority))
                        taskData.Priority = L.Loc(taskData.Priority);

                    if (!string.IsNullOrEmpty(taskData.Status))
                        taskData.Status = L.Loc(taskData.Status);
                }
            }
        }


        private async Task LoadStripLine()
        {
            await _ganttFileJS.InvokeVoidAsync("showToltip", GanttSetup.ShowToltip);
            await _ganttFileJS.InvokeVoidAsync("getStripLine", GanttSetup.Offset);
            await _ganttFileJS.InvokeVoidAsync("resetData");
        }

        private async Task LoadGantt<TModel>(GanttDataConvertDto<TModel> dataGantt) where TModel : GanttTaskBase
        {
            GanttSetup.ReloadSimulation = false;
            await LoadDataGanttJS(dataGantt);
        }

        #endregion

        #region Event Columns
        public void DefaultColumnChoose(List<ColumnChoose> columnChooses)
        {
            List<ColumnChoose> defaultColumns = new List<ColumnChoose>();
            foreach (var chooseColumn in columnChooses)
            {
                if (FillProperties.TryGetValue(chooseColumn.FieldName, out DisplayAttributes selectorColumns))
                {
                    if (GanttSetup.value is SimulationParametrizedDto GanttValue)
                        selectorColumns = UpdateColumnsForView(GanttSetup.SelectedView, selectorColumns);

                    if (selectorColumns.IsVisibleDefault)
                        defaultColumns.Add(chooseColumn);

                    chooseColumn.IsVisible = selectorColumns.IsVisibleDefault;
                }
            }

            GanttSetup.ColumnChooseProperties = SortColumnChoose(columnChooses);
            UpdateColumnsGantt(defaultColumns);
            _eventServices.Publish(ConstantComponents.GanttComponent, new EventArguments(ConstantComponents.GanttComponent, EventActions.UpdateColumns, GanttSetup.ColumnChooseProperties));
        }

        private DisplayAttributes UpdateColumnsForView(EnumViewPlanning viewName, DisplayAttributes selectorColumns)
        {
            if (viewName is not (EnumViewPlanning.Priority or EnumViewPlanning.Trailer))
                return selectorColumns;

            selectorColumns.IsVisibleDefault = selectorColumns.Caption switch
            {
                ConstantsTask.Trailer => true,
                ConstantsTask.Status => false,
                _ => selectorColumns.IsVisibleDefault
            };

            return selectorColumns;
        }

        private List<ColumnChoose> SortColumnChoose(List<ColumnChoose> columns)
        {
            return columns
                .OrderByDescending(c => c.IsVisible)
                .ThenBy(c => c.IsVisible ? c.Index : int.MaxValue)
                .ThenBy(c => c.IsVisible ? null : c.Caption)
                .ToList();
        }

        private async Task DeleteFilterColumns(bool viewFilter, bool updateGantt) => await _ganttFileJS.InvokeVoidAsync("deleteHeaderFilters", viewFilter, updateGantt);

        public async Task OnColumnChooser()
        {
            foreach (var kv in Properties)
            {
                string propName = kv.Key;
                var displayAttr = kv.Value;

                FillProperties.TryAdd(propName, displayAttr);

                if (!GanttSetup.ColumnChooseProperties.Any(x => x.FieldName == propName))
                {
                    GanttSetup.ColumnChooseProperties?.Add(
                        new ColumnChoose(
                            propName,
                            displayAttr.Caption,
                            displayAttr.IsVisible,
                            displayAttr.Index,
                            displayAttr.FieldType,
                            displayAttr.LevelFilterType
                        )
                    );
                }
            }
        }


        public void LoadColumnChoose(List<ColumnChoose> columnChooses)
        {
            bool isNew = columnChooses.Any();
            foreach (var chooseColumn in GanttSetup.ColumnChooseProperties)
            {
                if (isNew)
                {
                    foreach (var newColumn in columnChooses)
                    {
                        var existing = columnChooses.FirstOrDefault(c => c.FieldName == newColumn.FieldName);
                        if (existing != null)
                        {
                            existing.IsVisible = newColumn.IsVisible;
                        }
                    }
                    columnChooses = SortColumnChoose(columnChooses.Where(c => c.Caption != "").ToList());

                }
                else
                {

                    if (FillProperties.TryGetValue(chooseColumn.FieldName, out DisplayAttributes selectorColumns))
                        selectorColumns.IsVisible = chooseColumn.IsVisible;

                    if (!string.IsNullOrEmpty(chooseColumn.Caption))
                        columnChooses.Add(chooseColumn);

                    columnChooses = SortColumnChoose(columnChooses);
                }
            }
            _eventServices.Publish(ConstantComponents.GanttComponent, new EventArguments(ConstantComponents.GanttComponent, EventActions.ChooseColumns, new { columnChooses, GanttSetup.ColumnChooseProperties }));
        }

        public async Task UpdateColumnChoose(List<ColumnChoose> columnChooses)
        {
            await UpdateColumnsGantt(columnChooses.Where(x => x.IsVisible).ToList());
            if (ShouldApplyInitialFilters())
                await ApplyStartTaskFilterAsync();
        }

        private void CancelChooseColumns(List<ColumnChoose> columnChooses)
        {
            foreach (var chooseColumn in columnChooses)
            {
                if (FillProperties.TryGetValue(chooseColumn.FieldName, out DisplayAttributes selectorColumns))
                    chooseColumn.IsVisible = selectorColumns.IsVisible;
            }
            GanttSetup.ColumnChooseProperties = columnChooses;
        }

        private async Task UpdateColumnsGantt(List<ColumnChoose> columnChooses)
        {
            List<ColumnsGantt> columnsGantts = new List<ColumnsGantt>();
            foreach (var col in columnChooses)
            {
                if (col.FieldName == nameof(TaskData.Multiselect))
                {
                    if (GanttSetup.typeGantt == GanttView.Preview)
                        continue;

                    if (GanttSetup.typeGantt == GanttView.Planning)
                    {
                        col.FieldName = nameof(TaskData.Multiselect);
                        col.Caption = string.Empty;
                        col.ComponentType = ComponentType.CheckBox;
                    }
                }

                columnsGantts.Add(new ColumnsGantt
                {
                    dataField = col.FieldName,
                    caption = col.Caption,
                    index = col.Index,
                    dataType = col.ComponentType.ToString(),
                    levelFilterType = col.LevelFilterType,
                });
            }

            columnsGantts = columnsGantts.OrderBy(x => x.index).ToList();
            string jsonColumns = JsonSerializer.Serialize(columnsGantts);
            await _ganttFileJS.InvokeVoidAsync("UpdateColumns", jsonColumns);
            await _ganttFileJS.InvokeVoidAsync("ColumnHeaderClicks");
            GanttSetup.ReloadColumnsView = false;
        }

        private async Task RevertColumnsAndFilters(List<ColumnChoose> columnsChoose)
        {
            await SetPanelAndBackground(true, false);
            await DeleteFilterColumns(true, true);
            DefaultColumnChoose(columnsChoose);
            await Task.Delay(1500);
            await CollapsedGantt(true);
            await SetPanelAndBackground(false, false);
        }

        #endregion

        #region Methods

        public async Task ReloadGantt(GanttSetup setup)
        {
            await SetPanelAndBackground(true, true);
            await DeleteFilterColumns(ShouldApplyInitialFilters(), false);
            await LoadGantt(GanttTasksData);
            setup.ReloadSimulation = false;
            if (setup.ChooserColumnDefault)
            {
                await OnColumnChooser();
                DefaultColumnChoose(GanttSetup.ColumnChooseProperties);
            }
            await ApplyInitialFiltersAsync();
            await SetPanelAndBackground(false, false);
        }

        private async Task SetPanelAndBackground(bool panelVisible, bool backgroundShading)
        {
            BackgroundShading = backgroundShading;
            LoadPanelVisible = panelVisible;
            await InvokeAsync(StateHasChanged);
        }

        public async Task ApplyInitialFiltersAsync()
        {
         
            if (ShouldApplyInitialFilters())
            {
                GanttSetup.ApplyFilterStatus = false;
                await ApplyStatusFilterAsync();
            }

            if (!GanttSetup.ApplyOrderByStart)
                return;
            await CollapsedGantt(true);
            GanttSetup.ApplyOrderByStart = false;
            await ApplyStartTaskFilterAsync();
            await CollapsedGantt(true);
        }

        private async Task ApplyStatusFilterAsync()
        {
            var column = GetColumn(ConstantsTask.Status);

            if (column == null)
                return;

            var columnFilter = CreateColumnFilter(column);

            var filterArray = new[] { L.Loc(OrderStatus.Waiting), L.Loc(OrderStatus.Released), L.Loc(OrderStatus.Paused), };

            await _ganttFileJS.InvokeVoidAsync("aplicarFiltro", columnFilter, filterArray);
        }

        private async Task ApplyStartTaskFilterAsync()
        {
            var columnStart = GetColumn(ConstantsTask.StartTask);

            if (columnStart == null)
                return;

            await _ganttFileJS.InvokeVoidAsync("addOrderFilters", columnStart.FieldName);
            await _ganttFileJS.InvokeVoidAsync("updateIcons", columnStart.Index - 1);
        }

        private ColumnChoose GetColumn(string fieldName)
        {
            return GanttSetup.ColumnChooseProperties
                             .FirstOrDefault(x => x.FieldName == fieldName);
        }

        private ColumnsGantt CreateColumnFilter(ColumnChoose column)
        {
            return new ColumnsGantt
            {
                dataField = column.FieldName,
                caption = column.Caption,
                index = column.Index,
                dataType = column.ComponentType.ToString(),
                levelFilterType = column.LevelFilterType
            };
        }


        private bool ShouldApplyInitialFilters()
        {
            return GanttSetup.ApplyFilterStatus
                && GanttSetup.ColumnChooseProperties.Any();
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            GanttSetup = new();
            _ganttFileJS.InvokeVoidAsync("resetInstanceGantt");
            _eventServices.Unsubscribe(ConstantComponents.TopBarComponent, ConstantComponents.TopBarComponent, EventsTopBar);
            _eventServices.Unsubscribe(nameof(MlxSchedulerOptions), nameof(MlxSchedulerOptions), EventsTopBar);
            SignalRClientService.UnsubscribeFromReloadGantt(OnReloadGantt);
        }

        #endregion

    }
}
