using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using Mss.WorkForce.Code.Web.Helper;
using Mss.WorkForce.Code.Web.Interfaces;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;
using Mss.WorkForce.Code.Web.Services.Interfaces;

namespace Mss.WorkForce.Code.Web.Common
{
    public abstract class GanttOperations : ComponentBase, IGanttOperations
    {
        #region Fields
        public IJSObjectReference? _ganttModule;
        public Dictionary<string, DisplayAttributes> GanttProperties;
        #endregion

        #region Inject
        [Inject] public IMlxDialogService _DialogService { get; set; }
        [Inject] public IInitialDataService _InitialDataService { get; set; }
        [Inject] public ProtectedLocalStorage _LocalStorage { get; set; }
        [Inject] public IJSRuntime JS { get; set; }
        [Inject] public DataAccess _DataAccess { get; set; }
        [Inject] public ILocalizationService l { get; set; }
        [Inject] public GanttJsService _GanttService { get; set; }
        [Inject] public IEventServices _EventServices { get; set; }
        [Inject] public ISettingService _SettingService { get; set; }
        [Inject] public NavigationManager _Navigation { get; set; }
        [Inject] public ISimulateService _SimulateService { get; set; }
        [Inject] public IAlertService _AlertService { get; set; }
        [Inject] public IContextConfig _ContextConfig { get; set; }
        [Inject] public ILogger<GanttOperations> _logger { get; set; }

        #endregion

        #region Properties
        public bool BackGroudShading { get; set; }
        public Guid CurrentPlanning { get; set; }
        public SiteModel CurrentSite { get; set; } = new SiteModel();
        public string FilterSummaryTextPlanning { get; set; } = string.Empty;
        public string FilterSummaryTextYard { get; set; } = string.Empty;
        public GanttSetup GanttSetup { get; set; } = new();
        public bool LoadPanelVisible { get; set; }
        public bool OnSwitchView { get; set; }
        public bool RepaintDataGantt { get; set; }
        public bool ShowInfoDrawer { get; set; }
        public List<SiteModel> Sites { get; set; } = new List<SiteModel>();
        public EnumViewPlanning TypeView { get; set; }
        public DateTimeOffset LastUpdateGantt { get; set; }
        public bool ReloadDashboard { get; set; }
        public YardResourceFilter? YardResourceFilter { get; set; }
        public UserFormatOptions userFormat { get; set; } = new();
        public bool IsDisabledTopBarButtons { get; set; } = false;

        protected HashSet<Guid> _deletedTaskIds = new();
        #endregion

        #region Methods
        public virtual async Task LoadDataSimulation() => GanttSetup.ReloadSimulation = true;

        public virtual async Task LoadDataSimulation(Guid planningId)
        {
            CurrentPlanning = planningId;
            LastDateUpdateGantt(planningId);
            GanttSetup.ReloadSimulation = true;
            IsDisabledTopBarButtons = false;
        }

        public virtual void LoadGanttSetupData(EnumViewPlanning view) => TypeView = view;

        public virtual Task LoadGanttServiceAsync() => GanttModulo();

        public virtual async Task ReloadGanttSetupData()
        {
            ShowHidePanel(true, true);
            GanttSetup.ChooserColumnDefault = false;
            await LoadDataSimulation();
            StateHasChanged();
        }

        public virtual async Task RepaintGanttAsync(bool isPlanning, bool showDashboard)
        {
            if (RepaintDataGantt)
            {
                await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.repaintGantt.ToString(), showDashboard, isPlanning);
                //TODO posible recarga de dashboard en esta parte
                //RepaintDataGantt = reloadDashboard = false;
                RepaintDataGantt = false;
                if (!OnSwitchView)
                    LoadGanttSetupData(TypeView);
            }
            await Task.CompletedTask;
        }

        public async Task GetUserDataAsync()
        {
            await _InitialDataService.GetDataUserLocal();
            userFormat = _InitialDataService.GetUserFormat();
        }

        public virtual void ChangeGanttRepaint(bool repaintData)
        {
            ShowHidePanel(false, false);
            RepaintDataGantt = true;
            ShowInfoDrawer = repaintData;
        }

        public virtual void ShowHidePanel(bool IsPanelVisible, bool IsBackGroundVisible)
        {
            LoadPanelVisible = IsPanelVisible;
            BackGroudShading = IsBackGroundVisible;
            StateHasChanged();
        }

        public virtual async Task OnSiteSeleccionadaChanged((SiteModel newSite, bool changeColumns) args)
        {
            ShowHidePanel(true, true);
            GanttSetup.ApplyFilterStatus = GanttSetup.ApplyOrderByStart = GanttSetup.typeGantt == GanttView.Planning && TypeView != EnumViewPlanning.Priority;
            CurrentSite = args.newSite;
            _deletedTaskIds.Clear();
            if (!OnSwitchView)
            {
                LoadGanttSetupData(TypeView);
                await LoadDataSimulation();
                ReloadDashboard = false;
                GanttSetup.ReloadSimulation = true;
                GanttSetup.ChooserColumnDefault = args.changeColumns;
            }
            else
            {
                GanttSetup = new();
                await SuscribeEvents();
            }
            await Task.CompletedTask;
        }

        public virtual async Task OnSwitchChanged(bool changeValue)
        {
            OnSwitchView = changeValue;

            if (!changeValue)
            {
                GanttSetup.ChooserColumnDefault = true;
                await OnSiteSeleccionadaChanged((CurrentSite, true));
            }
            else
                await SuscribeEvents();
        }

        public virtual async Task SuscribeEvents() => await Task.CompletedTask;

        public virtual async Task<bool> OnReadyGantt() => await _ganttModule.InvokeAsync<bool>("ReadyLoadGantt");

        public string FormatFilterSummary(GanttFilterSettingsDto filter)
        {
            var fieldDesc = EnumHelper.GetItemDescription(filter.Reference);
            return $"{l.Loc("From {0} to next {1} hours", l.Loc(fieldDesc), filter.TimeUntil.ToUserTime(userFormat.HourFormat))}";
        }

        public virtual async void SettingsActions(EventArguments args)
        {
            try
            {
                switch (args.EventActions)
                {
                    case EventActions.Update:
                        GanttSetup.ApplyFilterStatus = GanttSetup.ApplyOrderByStart = GanttSetup.typeGantt == GanttView.Planning;
                        await ReloadGanttSetupData();
                        break;
                    case EventActions.UpdateColumns:
                        GanttSetup.ColumnChooseProperties = (List<ColumnChoose>)args.EventData;
                        break;
                    case EventActions.ApplyFilterTimeInterval:
                        ApplyFilterTimeInterval();
                        break;
                }
            } catch(Exception ex){
                _logger.LogError(ex, "Error in SettingsActions. Error: {Message}", ex.Message);
            }
        }

        public List<T> IncludeParentTasks<T>(List<T> allTasks, List<T> viewChild) where T : GanttTaskBase
        {
            var result = new List<T>();

            // Usamos un HashSet para evitar duplicados
            var visited = new HashSet<Guid>();

            foreach (var child in viewChild)
                AddWithParentsRecursive(child, allTasks, result, visited);

            return result;
        }

        private void AddWithParentsRecursive<T>(T task, List<T> allTasks, List<T> result, HashSet<Guid> visited) where T : GanttTaskBase
        {
            if (visited.Contains(task.id))
                return;

            result.Add(task);
            visited.Add(task.id);

            if (task.parentId.HasValue)
            {
                var parent = allTasks.FirstOrDefault(t => t.id == task.parentId.Value);
                if (parent != null)
                    AddWithParentsRecursive(parent, allTasks, result, visited);
            }
        }

        public List<T> CloneGanttList<T>(List<T> source) => source.ToList();

        public void LastDateUpdateGantt(Guid planningId)
        {
            if (!_DataAccess.IsPlanningInWarehouse(planningId, GanttSetup.WarehouseId))
                LastUpdateGantt = new DateTimeOffset();
            else
                LastUpdateGantt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(GanttSetup.Offset));
        }

        public Guid IsPlnanningInWarehouse(Guid planningId, Guid warehouseId) => _DataAccess.IsPlanningInWarehouse(planningId, GanttSetup.WarehouseId) ? planningId : Guid.Empty;

        public virtual List<T> ApplyActionMenuContextual<T>(List<T> values) => values;

        public virtual void Dispose() { }

        public async Task GanttModulo()
        {
            if (_ganttModule == null)
                _ganttModule = await _GanttService.GetModuleAsync(JS);
        }


        #endregion

        #region Filters		

        public async Task<GanttDataConvertDto<T>> applyFilter<T>(GanttDataConvertDto<T> ganttData) where T : GanttTaskBase
        {
            GanttDataConvertDto<T> DataFilters = new();
            GanttFilterSettingsDto savedFilter = await getFilter(GanttSetup.typeGantt);

            if (YardResourceFilter != null)
                DataFilters = ApplyFilterDockAndStage(ganttData);
            else if (savedFilter != null)
                DataFilters = await ApplyFilterTimeInterval(savedFilter, ganttData);
            else
                return ganttData;

            return DataFilters;
        }

        private async Task<GanttFilterSettingsDto> getFilter(GanttView nameGantt) => await _InitialDataService.GetGanttFilterForViewAsync(nameGantt);

        public async Task<GanttDataConvertDto<T>> ApplyFilterTimeInterval<T>(GanttFilterSettingsDto GanttFilterSettingsDto, GanttDataConvertDto<T> ganttData) where T : GanttTaskBase
        {
            try
            {
                GanttDataConvertDto<T> data = new();
                GanttSetup.ChooserColumnDefault = true;
                if (GanttFilterSettingsDto != null)
                {
                    data.TaskGantt = ApplyFilterToTasks(ganttData.TaskGantt, GanttFilterSettingsDto);
                    if (!data.TaskGantt.Any())
                        GanttSetup.ReloadColumnsView = true;

                    AddFilterText(GanttSetup.typeGantt, GanttFilterSettingsDto);
                }

                return data;
            }
            catch (Exception e)
            {
                //_logger.LogWarning("Error: ", e.Message);
                return new GanttDataConvertDto<T> { };
            }
        }

        private void AddFilterText(GanttView ganttView, GanttFilterSettingsDto GanttFilterSettingsDto)
        {
            switch (ganttView)
            {
                case GanttView.YardView:
                    FilterSummaryTextYard = FormatFilterSummary(GanttFilterSettingsDto);
                    break;

                case GanttView.Planning:
                    FilterSummaryTextPlanning = FormatFilterSummary(GanttFilterSettingsDto);
                    break;
            }
        }

        private List<T> ApplyFilterToTasks<T>(List<T> tasks, GanttFilterSettingsDto filter) where T : GanttTaskBase
        {
            // APlicamos al hora con 
            var now = DateTimeOffset.UtcNow.AddHours(GanttSetup.Offset);
            var until = now.Add(filter.TimeUntil);
            List<T> childTasks;

            switch (filter.Reference)
            {
                case EnumDateField.ArrivalDate:
                case EnumDateField.CommittedDate:
                    childTasks = [.. tasks.Where(t => t.IsChildTask && t.CommintedDate >= now && t.CommintedDate <= until)]; break;
                case EnumDateField.StartDate:
                    childTasks = [.. tasks.Where(t => t.IsChildTask && t.StartDate >= now && t.StartDate <= until)]; break;
                case EnumDateField.EndDate:
                    childTasks = [.. tasks.Where(t => t.IsChildTask && t.EndDate >= now && t.EndDate <= until)]; break;
                default:
                    childTasks = [.. tasks.Where(t => t.IsChildTask && t.StartDate >= now && t.StartDate <= until)]; break;
            }

            var resp = IncludeParentTasks(tasks, childTasks);

            // Si las tareas padre tienen segmentos, quitar las que ya no tienen tareas hijas
            foreach (var parentTask in resp.Where(t => t.segments is not null))
            {
                var segmentsToRemove = parentTask.segments
                    .Where(segment => !resp.Exists(x => x.id == segment.id)).ToList();

                foreach (var segment in segmentsToRemove)
                {
                    parentTask.segments.Remove(segment);
                }
            }

            return resp;
        }

        private GanttDataConvertDto<T> ApplyFilterDockAndStage<T>(GanttDataConvertDto<T> ganttData) where T : GanttTaskBase
        {
            try
            {
                GanttDataConvertDto<T> data = new();
                data.TaskGantt = ApplyFilterYardToTasks(ganttData.TaskGantt.OfType<TaskData>().ToList()).OfType<T>().ToList();

                if (!data.TaskGantt.Any())
                    GanttSetup.ReloadColumnsView = true;
                return data;
            }
            catch (Exception e)
            {
                //_logger.LogWarning("Error: ", e.Message);
                return new GanttDataConvertDto<T> { };
            }
        }

        private List<TaskData> ApplyFilterYardToTasks(List<TaskData> tasks)
        {
            List<TaskData> childTasks;

            string dockOrStageName = YardResourceFilter?.Name ?? string.Empty;
            DateTimeOffset starDate = YardResourceFilter.StartDate;
            DateTimeOffset endDate = YardResourceFilter.EndDate;

            childTasks = [.. tasks.Where(t => t.IsChildTask && t.DockName == dockOrStageName && t.StartDate >= starDate && t.StartDate <= endDate)];

            var resp = IncludeParentTasks(tasks, childTasks);

            // Si las tareas padre tienen segmentos, quitar las que ya no tienen tareas hijas
            foreach (var parentTask in resp.Where(t => t.segments is not null))
            {
                var segmentsToRemove = parentTask.segments
                    .Where(segment => !resp.Exists(x => x.id == segment.id)).ToList();

                foreach (var segment in segmentsToRemove)
                {
                    parentTask.segments.Remove(segment);
                }
            }

            return resp;
        }

        public async Task SavedFilter(GanttView ganttView)
        {
            var savedFilter = await _InitialDataService.GetGanttFilterForViewAsync(ganttView);

            if (savedFilter != null)
                AddFilterText(ganttView, savedFilter);
        }

        public virtual Task ClearFilter(eFilterType filterType) => Task.CompletedTask;

        public virtual void ApplyFilterTimeInterval()
        {
            GanttSetup.ReloadData = true;
            ShowHidePanel(true, false);
        }

        #endregion

        public void TraslateProperties<T>(List<T> source)
        {
            foreach (var item in source)
            {
                if (item == null)
                    continue;

                foreach (var kvp in GanttProperties)
                {
                    var prop = typeof(T).GetProperty(kvp.Key);
                    var value = prop.GetValue(item);

                    if (value == null || !kvp.Value.TraslateCaption)
                        continue;

                    if (prop.PropertyType == typeof(string))
                    {
                        var translated = string.Empty;
                        if (kvp.Key == "Priority")
                        {
                            var priorityEnum = EnumHelper.GetEnumValueFromDescription<EnumOrderPriority>(value.ToString());

                            translated = priorityEnum switch
                            {
                                EnumOrderPriority.Urgent => EnumHelper.GetItemDescription(EnumOrderPriority.Urgent),
                                EnumOrderPriority.High => EnumHelper.GetItemDescription(EnumOrderPriority.High),
                                EnumOrderPriority.Normal => EnumHelper.GetItemDescription(EnumOrderPriority.Normal),
                                EnumOrderPriority.Low => EnumHelper.GetItemDescription(EnumOrderPriority.Low),
                                EnumOrderPriority.VeryLow => EnumHelper.GetItemDescription(EnumOrderPriority.VeryLow),
                                _ => value.ToString()
                            };
                        }
                        else
                            translated = value.ToString();


                        prop.SetValue(item, l.Loc(translated));
                    }
                }
            }
        }

        public async Task RemoveTasksFromGanttDom(List<TaskData> tasks)
        {
            if (_ganttModule == null)
                _ganttModule = await _GanttService.GetModuleAsync(JS);

            var ids = tasks.Select(x => x.id).ToList();

            MarkTasksAsDeleted(ids);

            await _ganttModule.InvokeVoidAsync("deleteTasksFromGanttDom", ids);
        }

        public void MarkTasksAsDeleted(List<Guid> taskIds)
        {
            foreach (var id in taskIds)
                _deletedTaskIds.Add(id);
        }

        [JSInvokable]
        public void showLoadingJs(bool show) => ShowHidePanel(show, false);        

    }
}