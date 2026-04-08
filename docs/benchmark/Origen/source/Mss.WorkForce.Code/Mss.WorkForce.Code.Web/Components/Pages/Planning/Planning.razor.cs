using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.Shared;
using Mss.WorkForce.Code.Web.Helper;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Components.Pages.Planning
{
    public partial class Planning : GanttOperations, IDisposable
    {
        #region Fields  
        private bool showPivotChart = true;
        /* Variables para Menu contextual de tareas*/
        public bool IsLockTaskVisible { get; set; } = false;
        public bool IsChangePriorityVisible { get; set; } = false;
        public bool IsCancelTaskVisible { get; set; } = false;
        private bool isLockButtonDisabled = false;
        private bool isChangepriorityBtnDisabled = true;
        private DateTime dateTimeBlock;
        private string newPriority = string.Empty;
        private DotNetObjectReference<Planning> _dotNetHelper;

        public GetAttributesDto<TaskData>? GridParameters { get; set; }
        private TaskData selectTask { get; set; }
        private List<TaskData> ListTasksSelectedForAction { get; set; } = new();
        public bool MakeChangeToListTask { get; set; } = false;
        public eToolbarActions actionToTask { get; set; }

        public string MesssageMultiSelect = string.Empty;

        /* Variables para alertas*/
        private IEnumerable<AlertMessageDto> Alerts = new List<AlertMessageDto>();
        private MlxAlertBar? MlxAlerBarRef;
        private DotNetObjectReference<MlxAlertBar>? _dotNetHelperTopBar;

        public GanttDataConvertDto<TaskData> DataSimulation = new();
        public List<TaskData> GanttReadyData = new();

        public bool OnModeView { get; set; }

        [Inject] private IWarehouseService WarehouseService { get; set; }

        /* Variables de titulos*/
        public string txtLockOrder = "Lock order";
        public string txtCancelOrder = "Cancel order";
        public string txtChangePriorityOrder = "Change priority";
        public string txtMultiSelect = string.Empty;
        public DateTimeOffset LastUpdateGrid { get; set; }

        #endregion

        #region Methods      

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadGanttServiceAsync();
                GanttSetup.ChooserColumnDefault = true;
                GanttSetup.ApplyFilterStatus = true;
                var resultYardResourceFilter = await _LocalStorage.GetAsync<YardResourceFilter?>(StorageKeysConstants.YardResourceFilter);

                if (resultYardResourceFilter.Success)
                    YardResourceFilter = resultYardResourceFilter.Value;
                else
                    YardResourceFilter = null;

                await SavedFilter(GanttView.Planning);
            }
            await RepaintGanttAsync(true, ShowInfoDrawer);
        }

        protected override async Task OnInitializedAsync()
        {
            await GetUserDataAsync();
            GridParameters = new(l);
            GanttSetup.typeGantt = GanttView.Planning;
            TypeView = EnumViewPlanning.Order;
            ShowHidePanel(true, true);
            GanttProperties = GridParameters.GetTranslateCaptions();
            _EventServices.Subscribe(ConstantComponents.GanttSimulation, ConstantComponents.GanttSimulation, SettingsActions);
            _EventServices.Subscribe(ConstantComponents.GanttSimulation, EventActions.ApplyFilterTimeInterval.ToString(), SettingsActions);
        }

        private void HandleLastUpdateGrid(DateTimeOffset value)
        {
            LastUpdateGrid = value;
        }

        public override async Task LoadDataSimulation()
        {
            IsDisabledTopBarButtons = true;
            var data = await GetBaseData();
            data = await applyFilter(data);

            await GetMessageAlerts(data.PlanningId);
            PrepareDataTasks(data);
            await base.LoadDataSimulation(data.PlanningId);
            OnModeView = WarehouseService.GetWarehousesById(CurrentSite.Id)?.Mode == "Manual";
        }

        public override void LoadGanttSetupData(EnumViewPlanning newView)
        {
            GanttSetup.WarehouseId = CurrentSite.Id;
            GanttSetup.typeGantt = GanttView.Planning;
            GanttSetup.EditGantt = true;
            GanttSetup.ShowToltip = true;
            GanttSetup.Offset = CurrentSite.Offset;
            GanttSetup.SelectedView = newView;
            GanttSetup.value = new SimulationParametrizedDto()
            {
                dataOperator = new(),
                inputList = new(),
                outputList = new(),
                shiftDtos = new(),
            };

            userFormat.TimeZoneOffSet = GanttSetup.Offset;
        }

        public override async Task LoadGanttServiceAsync()
        {
            _ganttModule = await _GanttService.GetModuleAsync(JS);
            _dotNetHelper = DotNetObjectReference.Create(this);
            await _ganttModule.InvokeVoidAsync("getDoNetHelper", _dotNetHelper);

            if (MlxAlerBarRef is not null)
            {
                _dotNetHelperTopBar = DotNetObjectReference.Create(MlxAlerBarRef);
                await _ganttModule.InvokeVoidAsync("registerAlertInterop", _dotNetHelperTopBar);
            }
        }

        private void LoadColumns(EnumViewPlanning viewName)
        {
            void SetIndex(string fieldName, int index, LevelFilterType levelFilterType)
            {
                var col = GanttSetup.ColumnChooseProperties.FirstOrDefault(c => c.FieldName == fieldName);
                col.Index = index;
                col.LevelFilterType = levelFilterType;
            }

            void SetVisibility(string fieldName, bool isVisible)
            {
                var col = GanttSetup.ColumnChooseProperties.FirstOrDefault(c => c.FieldName == fieldName);
                col.IsVisible = isVisible;
            }

            switch (viewName)
            {
                case EnumViewPlanning.Order:
                    SetIndex(ConstantsTask.Flow, 3, LevelFilterType.SecondLevel);
                    SetIndex(ConstantsTask.Title, 4, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.StartTask, 5, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.Priority, 7, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.Status, 8, LevelFilterType.ThirdLevel);
                    SetVisibility(ConstantsTask.Status, true);
                    SetVisibility(ConstantsTask.Trailer, false);
                    break;

                case EnumViewPlanning.Priority:
                    SetIndex(ConstantsTask.Priority, 3, LevelFilterType.SecondLevel);
                    SetIndex(ConstantsTask.Flow, 4, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.StartTask, 5, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.Trailer, 7, LevelFilterType.ThirdLevel);
                    SetVisibility(ConstantsTask.Status, false);
                    SetVisibility(ConstantsTask.Trailer, true);
                    break;

                case EnumViewPlanning.Trailer:
                    SetIndex(ConstantsTask.Trailer, 3, LevelFilterType.SecondLevel);
                    SetIndex(ConstantsTask.Flow, 4, LevelFilterType.SecondLevel);
                    SetIndex(ConstantsTask.Title, 5, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.StartTask, 6, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.Priority, 7, LevelFilterType.ThirdLevel);
                    SetIndex(ConstantsTask.Status, 8, LevelFilterType.ThirdLevel);
                    SetVisibility(ConstantsTask.Status, true);
                    SetVisibility(ConstantsTask.Trailer, true);
                    break;
            }

            GanttSetup.ReloadColumnsView = true;
        }

        private string BuildResourceFilterLabel(YardResourceFilter filter)
        {
            string resourceLabel = filter.ResourceType switch
            {
                YardResourceType.Docks => "Dock",
                YardResourceType.Stages => "Stage",
                _ => "Resource"
            };

            string startTime = $"{filter.StartDate.ToUserTime(userFormat.HourFormat)}";
            string endTime = $"{filter.EndDate.ToUserTime(userFormat.HourFormat)}";

            return $"{resourceLabel} {filter.Name} {l.Loc("from")} {startTime} {l.Loc("to")} {endTime}";
        }

        public override async void SettingsActions(EventArguments args)
        {
            switch (args.EventActions)
            {
                case EventActions.PivotDataFilter:
                    OnSwitchView = (bool)args.EventData;
                    RepaintDataGantt = !OnSwitchView;
                    if (RepaintDataGantt)
                    {
                        GanttSetup.ChooserColumnDefault = false;
                        await ClosePoup();
                    }
                    break;
                case EventActions.ChangeView:
                    await ChangeViewPriority(args);
                    break;
                default:
                    base.SettingsActions(args);
                    break;
            }
        }

        public override async Task SuscribeEvents() => LoadDataPivotGrid(true);

        private async Task ClosePoup() => await JS.CallJs(ConstantComponents.GanttComponent, eGanttMethods.closePopup.ToString());

        private void LoadDataPivotGrid(bool updatePivot = false)
        {
            EventActions action = updatePivot ? EventActions.Update : EventActions.PivotDataFilter;
            object value = updatePivot ? CurrentSite : "";

            _EventServices.Publish(ConstantComponents.PivotData, new EventArguments(nameof(Planning), action, value));
        }

        public override List<T> ApplyActionMenuContextual<T>(List<T> values)
        {
            foreach (var obj in values)
            {
                if (obj is TaskData item)
                {
                    bool isActiveStatus = item.Status != null && item.Status.ToLower() == InputOrderStatus.Waiting.ToLower();
                    if (isActiveStatus)
                    {
                        item.ActionBlock = true;
                        item.ActionCancel = true;
                    }

                    if (!item.isBlock && isActiveStatus)
                        item.ActionBlock = true;

                    if (item.isBlock)
                    {
                        item.ActionBlock = false;
                        item.ActionUnlock = true;
                    }

                    if (item.StartDate > DateTime.UtcNow.AddHours(GanttSetup.Offset))
                        item.ActionPriority = true;
                }
            }

            return values;
        }

        public override void Dispose()
        {
            _deletedTaskIds.Clear();
            _EventServices.Unsubscribe(ConstantComponents.GanttSimulation, EventActions.ApplyFilterTimeInterval.ToString(), SettingsActions);
            _EventServices.Unsubscribe(ConstantComponents.GanttSimulation, ConstantComponents.GanttSimulation, SettingsActions);
        }

        #endregion

        #region Handles

        private async Task<GanttDataConvertDto<TaskData>> GetBaseData()
        {
            var data = await _SimulateService.GetSimulateData(GanttSetup.WarehouseId, userFormat, GanttSetup.SelectedView);
            data.TaskGantt = ChangeMultiselect(data.TaskGantt);
            GanttReadyData = CloneGanttList(data.TaskGantt);
            return data;
        }

        private async void PrepareDataTasks(GanttDataConvertDto<TaskData> data)
        {
            DataSimulation = await getAlerst(data);
            DataSimulation.TaskGantt = ApplyActionMenuContextual(DataSimulation.TaskGantt);
            TraslateProperties(DataSimulation.TaskGantt);
        }



        #endregion

        #region Events

        private void CancelLockTask() => IsLockTaskVisible = false;

        private void CancelButtonTask() => IsCancelTaskVisible = false;

        private void CancelChanges() => MakeChangeToListTask = false;

        private void CancelPriorityTask()
        {
            IsChangePriorityVisible = false;
            isChangepriorityBtnDisabled = true;
        }

        private void HandleLockButtonStateChanged(bool newState) => isLockButtonDisabled = newState;

        private void HandleChangePriorityButtonState(bool newState) => isChangepriorityBtnDisabled = newState;

        private void HandleDateTimeChanged(DateTime newDateTime) => dateTimeBlock = newDateTime;

        private void HandleChangePriorityTask(string newDateTime) => newPriority = newDateTime;

        private async void SaveLockTask()
        {
            try
            {
                IsLockTaskVisible = false;
                StateHasChanged();
                bool isTaskStillPrepared = ListTasksSelectedForAction.Any(x => x.id == selectTask.id) ? ListTasksSelectedForAction.Any(t => _DataAccess.IsTaskStillPrepared(t.id)) : _DataAccess.IsTaskStillPrepared(selectTask.id);
                if (isTaskStillPrepared)
                {
                    ShowHidePanel(true, true);
                    await lockUnlockTask(true);
                    await ReloadGanttSetupData();
                }
                else
                {
                    GanttSetup.ReloadColumnsView = true;
                    GanttSetup.ReloadSimulation = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async void CancelTask()
        {
            try
            {
                IsCancelTaskVisible = false;
                bool isTaskStillPrepared = ListTasksSelectedForAction.Any(x => x.id == selectTask.id) ? ListTasksSelectedForAction.Any(t => _DataAccess.IsTaskStillPrepared(t.id)) : _DataAccess.IsTaskStillPrepared(selectTask.id);
                if (isTaskStillPrepared)
                {
                    ShowHidePanel(true, true);
                    await cancelTask();
                    await ReloadGanttSetupData();
                }
                else
                {
                    GanttSetup.ReloadColumnsView = true;
                    GanttSetup.ReloadSimulation = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void HandleAlertDataReceived(IEnumerable<AlertMessageDto> alerts) => Alerts = alerts;

        private async void SaveChangePriorityTask()
        {
            IsChangePriorityVisible = false;
            ShowHidePanel(true, true);
            await ChangePriorityTask();
            await ReloadGanttSetupData();
        }

        private void UpdateSelectedTask(List<TaskData> selectedListTasks, TaskData selectedTask)
        {
            selectTask = selectedTask;
            ListTasksSelectedForAction.Clear();
            foreach (TaskData taskData in selectedListTasks)
                ListTasksSelectedForAction.Add(taskData);
        }

        private async Task lockUnlockTask(bool isLock)
        {
            ShowHidePanel(true, false);

            dateTimeBlock = new DateTime(
                dateTimeBlock.Year,
                dateTimeBlock.Month,
                dateTimeBlock.Day,
                dateTimeBlock.Hour,
                dateTimeBlock.Minute,
                0
            );

            string blockDateString = dateTimeBlock.ToUserTime(userFormat.HourFormat);

            var tasks = ListTasksSelectedForAction.Any(x => x.id == selectTask.id)
                ? ListTasksSelectedForAction
                : new List<TaskData> { selectTask };

            foreach (var t in tasks)
            {
                t.isBlock = isLock;

                if (isLock)
                    t.blockDate = new DateTimeOffset(dateTimeBlock);

                UpdateLocalLockState(t, isLock, dateTimeBlock);
            }

            await LockOrderBase(tasks, blockDateString, isLock);

            if (tasks == ListTasksSelectedForAction)
                ListTasksSelectedForAction.Clear();

            CleanList(ListTasksSelectedForAction);
            selectTask = null;
            GanttSetup.ReloadColumnsView = true;
            await Task.Delay(500);
            ShowHidePanel(false, false);
        }

        private async Task LockOrderBase(List<TaskData> lockTask, string blockDateTime, bool isLock)
        {
            ShowHidePanel(true, true);
            await _SimulateService.SaveDataLockTaskPlanning(lockTask, CurrentSite.Id, GanttSetup.Offset);

            foreach (TaskData task in lockTask)
                await _ganttModule.InvokeVoidAsync("LockUnlockTask", task, isLock, blockDateTime);
            await ReloadGanttSetupData();
        }

        private List<TaskData> AddWorkFlow(List<TaskData> items)
        {
            foreach (var task in items)
            {
                task.Multiselect = true;

                if (string.IsNullOrEmpty(task.WorkFlow))
                    task.WorkFlow = DataSimulation.TaskGantt.FirstOrDefault(t => t.id == task.parentId).WorkFlow;

            }
            return items;
        }

        private async void CleanList(List<TaskData> taskDatas) => await _ganttModule.InvokeVoidAsync("clearListSelectedTask", !taskDatas.Any());

        private async Task cancelTask()
        {
            var tasksToCancel = (ListTasksSelectedForAction.Any() && ListTasksSelectedForAction.Any(x => x.id == selectTask.id))
                ? new List<TaskData>(ListTasksSelectedForAction) : new List<TaskData> { selectTask };

            try
            {
                await _SimulateService.CancelTaskPlanning(tasksToCancel, CurrentSite.Id);

                await RemoveTasksFromGanttDom(tasksToCancel);

                ListTasksSelectedForAction.Clear();
                selectTask = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CancelTask: {ex.ToString()}");
            }
        }

        private void ToggleChartVisibility() => showPivotChart = !showPivotChart;

        private async Task ChangePriorityTask()
        {
            if (string.IsNullOrEmpty(newPriority))
                return;

            var tasksToUpdate = ListTasksSelectedForAction.Any(x => x.id == selectTask.id)
                ? ListTasksSelectedForAction
                : new List<TaskData> { selectTask };

            await _SimulateService.SaveDataChangePriorityPlanning(
                newPriority,
                tasksToUpdate,
                CurrentSite.Id
            );

            foreach (var task in tasksToUpdate)
            {
                var ganttTask = DataSimulation.TaskGantt
                    .FirstOrDefault(t => t.id == task.id);

                if (ganttTask != null)
                    ganttTask.Priority = newPriority;

                var planningItem = DataSimulation.itemsPlanning
                    .FirstOrDefault(x => x.WorkOrderPlanning.Id == task.id);

                if (planningItem?.WorkOrderPlanning != null)
                {
                    planningItem.WorkOrderPlanning.Priority = newPriority;

                    if (planningItem.WorkOrderPlanning.InputOrder != null)
                        planningItem.WorkOrderPlanning.InputOrder.Priority = newPriority;
                }
                var inputOrder = DataSimulation.itemsPlanning
                    .Where(x => x.WorkOrderPlanning?.InputOrder?.Id == task.InputOrderId)
                    .Select(x => x.WorkOrderPlanning.InputOrder)
                    .FirstOrDefault();

                if (inputOrder != null)
                    inputOrder.Priority = newPriority;
            }

            await UpdatePriorityInGantt(tasksToUpdate, newPriority);
        }

        private void MakeToChanges(eToolbarActions actions)
        {
            MakeChangeToListTask = false;

            switch (actions)
            {
                case eToolbarActions.ChangePriority:
                    IsChangePriorityVisible = true;
                    isChangepriorityBtnDisabled = true;
                    break;
                case eToolbarActions.CancelTask:
                    IsCancelTaskVisible = true;
                    break;

                case eToolbarActions.LockTask:
                    IsLockTaskVisible = true;
                    break;
            }
        }

        private async Task ChangeViewPriority(EventArguments eventData)
        {
            if (eventData.EventData is EnumViewPlanning View && View != TypeView)
            {
                PrepareViewChange(View);

                if (!OnSwitchView)
                    LoadGanttViewAsync(View);
                else
                    LoadDataPivotGrid();
            }
        }

        private void PrepareViewChange(EnumViewPlanning view)
        {
            ShowHidePanel(true, false);
            GanttSetup.ChooserColumnDefault = false;
            TypeView = view;
        }

        private void LoadGanttViewAsync(EnumViewPlanning view)
        {
            LoadGanttSetupData(view);
            LoadColumns(view);

            var data = GanttConverter.ConvertToGanttData(
                DataSimulation.itemsPlanning,
                DataSimulation.warehouseProcessPlanning,
                userFormat,
                view
            );

            var filteredTasks = data.TaskGantt
                .Where(task => !_deletedTaskIds.Contains(task.id))
                .ToList();

            filteredTasks = RemoveEmptyParentNodes(filteredTasks);

            DataSimulation.TaskGantt = filteredTasks;

            PrepareDataTasks(DataSimulation);
            GanttSetup.ReloadData = true;
            GanttSetup.ReloadColumnsView = true;
            GanttSetup.ApplyFilterStatus = GanttSetup.ApplyOrderByStart = TypeView != EnumViewPlanning.Priority;

            StateHasChanged();
        }

        [JSInvokable]
        public async void ToolBarActionsPopup(List<Guid> guidSelected, TaskData selectedTask, string action)
        {
            ShowHidePanel(false, false);
            if (guidSelected != null)
            {
                var SelectionListTask = AddWorkFlow(DataSimulation.TaskGantt.Where(x => guidSelected.Contains(x.id)).ToList());
                bool isSelected = guidSelected.Contains(selectedTask.id);
                actionToTask = (eToolbarActions)Enum.Parse(typeof(eToolbarActions), action);
                switch (actionToTask)
                {
                    case eToolbarActions.LockTask:
                        if (!isSelected)
                            IsLockTaskVisible = true;
                        else
                            SelectionListTask = MakesChangesLockTask(SelectionListTask);
                        UpdateSelectedTask(SelectionListTask, selectedTask);
                        break;

                    case eToolbarActions.CancelTask:
                        if (!isSelected)
                            IsCancelTaskVisible = true;
                        else
                            SelectionListTask = MakeChangesCancelTask(SelectionListTask);

                        UpdateSelectedTask(SelectionListTask, selectedTask);
                        break;

                    case eToolbarActions.UnlockTask:
                        UpdateSelectedTask(SelectionListTask, selectedTask);
                        await MakesChangesUnlockTask();
                        GanttSetup.ReloadColumnsView = true;
                        break;

                    case eToolbarActions.ChangePriority:
                        if (!isSelected)
                        {
                            IsChangePriorityVisible = true;
                            isChangepriorityBtnDisabled = true;
                        }
                        else
                            SelectionListTask = MakeChangesPriorityToTask(SelectionListTask);

                        UpdateSelectedTask(SelectionListTask, selectedTask);
                        break;
                }
                StateHasChanged();
            }
        }

        private List<TaskData> ChangeMultiselect(List<TaskData> taskGantt)
        {
            foreach (var t in ListTasksSelectedForAction)
            {
                var task = taskGantt.FirstOrDefault(x => x.InputOrderId == t.InputOrderId);
                if (task != null)
                    task.Multiselect = t.Multiselect;
            }
            return taskGantt;
        }

        private List<TaskData> MakeChangesPriorityToTask(List<TaskData> selectionLockTasks)
        {
            if (selectionLockTasks.Any(x => !x.ActionPriority))
            {
                var notSelectionLockTasks = selectionLockTasks.Where(x => !x.ActionPriority).ToList();
                selectionLockTasks = selectionLockTasks.Where(x => x.ActionPriority).ToList();
                txtMultiSelect = txtChangePriorityOrder;
                MesssageMultiSelect = BuildPriorityMessage(notSelectionLockTasks) + " " + BuildPriorityQuestion(selectionLockTasks);
                MakeChangeToListTask = true;
            }
            else
            {
                IsChangePriorityVisible = true;
                isChangepriorityBtnDisabled = true;
            }

            return selectionLockTasks;
        }

        private List<TaskData> MakesChangesLockTask(List<TaskData> selectionLockTasks)
        {
            if (selectionLockTasks.Any(x => !x.ActionBlock))
            {
                txtMultiSelect = txtLockOrder;
                MakeChangeToListTask = true;
                var notSelectionLockTasks = selectionLockTasks.Where(x => !x.ActionBlock).ToList();
                selectionLockTasks = selectionLockTasks.Where(x => x.ActionBlock).ToList();

                MesssageMultiSelect = BuildLockMessage(notSelectionLockTasks) + " " + BuildBlockQuestion(selectionLockTasks);
            }
            else
                IsLockTaskVisible = true;

            return selectionLockTasks;
        }

        private async Task MakesChangesUnlockTask()
        {
            ShowHidePanel(false, false);
            var taskGantt = ListTasksSelectedForAction.Any(x => x.id == selectTask.id) ? ListTasksSelectedForAction : new List<TaskData> { selectTask };
            bool resp = await _DialogService.ShowDialogAsync(l.Loc("Unlock order"), BuildUnlockMessage(taskGantt.Where(x => x.isBlock).ToList()));
            if (!resp) return;

            await lockUnlockTask(false);
        }

        private List<TaskData> MakeChangesCancelTask(List<TaskData> selectionLockTasks)
        {
            if (selectionLockTasks.Any(x => !x.ActionCancel))
            {
                var notSelectionCancelTask = selectionLockTasks.Where(x => !x.ActionCancel).ToList();
                selectionLockTasks = selectionLockTasks.Where(x => x.ActionCancel).ToList();
                txtMultiSelect = txtCancelOrder;
                MesssageMultiSelect = BuildCancelMessage(notSelectionCancelTask) + " " + BuildCancelQuestion(selectionLockTasks);
                MakeChangeToListTask = true;
            }
            else
                IsCancelTaskVisible = true;

            return selectionLockTasks;
        }

        public virtual async Task OnAutomaticManualChanged(bool changeValue)
        {
            OnModeView = changeValue;

            var warehouse = WarehouseService.GetWarehousesById(CurrentSite.Id);
            if (warehouse == null) return;

            warehouse.Mode = changeValue ? "Manual" : "Automatic";
            await WarehouseService.UpdateWarehouse(new List<WarehouseDto> { warehouse });

            ShowHidePanel(false, false);
        }

        #endregion

        #region Build Messages

        private string BuildLockMessage(List<TaskData> taskGantt) => BuildMessage(taskGantt, "Order {0} cannot be blocked because it has already been started or completed.", "Orders {0} cannot be blocked because they have already been started or completed.");

        private string BuildBlockQuestion(List<TaskData> taskGantt) => BuildMessage(taskGantt, "Do you want to block the other selected order {0} ?", "Do you want to block the other selected orders {0} ?");

        private string BuildCancelMessage(List<TaskData> taskGantt) => BuildMessage(taskGantt, "Order {0} cannot be canceled because it has already been started, completed or blocked.", "Orders {0} cannot be canceled because they have already been started, completed or blocked.");

        private string BuildCancelQuestion(List<TaskData> taskGantt) => BuildMessage(taskGantt, "Do you want to cancel the other selected order {0} ?", "Do you want to cancel the other selected orders {0} ?");

        private string BuildPriorityMessage(List<TaskData> taskGantt) => BuildMessage(taskGantt, "Order {0} cannot be reprioritized because it has already been started, completed or blocked.", "Orders {0} cannot be reprioritized because they have already been started, completed or blocked.");

        private string BuildPriorityQuestion(List<TaskData> taskGantt) => BuildMessage(taskGantt, "Do you want to reprioritize the other selected order {0} ?", "Do you want to reprioritize the other selected orders {0} ?");

        private string BuildUnlockMessage(List<TaskData> taskGantt) => BuildMessage(taskGantt, "Are you sure you want to unlock the order {0} ?", "Are you sure you want to unlock the orders {0} ?");

        private string BuildMessage(List<TaskData> taskGantt, string singular, string plural)
        {
            string messageTemplate = taskGantt.Count > 1 ? plural : singular;
            return l.Loc(messageTemplate, AddMesaggeForTasks(taskGantt));
        }

        private string AddMesaggeForTasks(List<TaskData> taskGantt) => " [" + string.Join("], [", taskGantt.Select(t => t.title)) + "] ";
        #endregion

        #region Alerts

        private async Task<GanttDataConvertDto<TaskData>> getAlerst(GanttDataConvertDto<TaskData> ganttData)
        {
            IEnumerable<AlertMessageDto> alerts = await _AlertService.GetAlertNotificationsByPlanningAsync(ganttData.PlanningId);

            foreach (TaskData task in ganttData.TaskGantt)
            {
                if (task.levelTask == 3 && alerts.Where(x => x.AlertType == AlertType.SpecificIndicator).ToList() is List<AlertMessageDto> alertPlanning && alertPlanning.Any())
                {
                    if (alertPlanning.Where(t => t.EntityId == task.id).ToList() is List<AlertMessageDto> alertsbyTask && alertsbyTask.Any())
                    {
                        task.Alerts = alertsbyTask;
                        task.HasAlerts = true;
                    }
                }
            }

            return ganttData;
        }

        private async Task GetMessageAlerts(Guid IdPlanning)
        {
            IEnumerable<AlertMessageDto> alerts = await _AlertService.GetAlertNotificationsByPlanningAsync(IdPlanning);
            HandleAlertDataReceived(alerts);
        }

        public override async Task ClearFilter(eFilterType filterType)
        {
            ShowHidePanel(false, false);
            bool resp = await _DialogService.ShowDialogAsync(l.Loc("NOTIFICATION"), l.Loc($"Are you sure you want to delete the filter?"));
            if (!resp) return;
            ShowHidePanel(true, false);
            switch (filterType)
            {
                case eFilterType.TimeIntervale:
                    await _InitialDataService.ClearGanttFilterAsync(GanttView.Planning);
                    FilterSummaryTextPlanning = string.Empty;
                    break;

                case eFilterType.YardResource:
                    YardResourceFilter = null;
                    await _LocalStorage.DeleteAsync(StorageKeysConstants.YardResourceFilter);
                    break;
            }
            GanttSetup.ChooserColumnDefault = false;
            GanttSetup.ReloadData = true;
            DataSimulation.TaskGantt = GanttReadyData;
            ShowHidePanel(true, false);
        }

        public override async void ApplyFilterTimeInterval()
        {
            ShowHidePanel(true, false);
            var data = await applyFilter(DataSimulation);
            DataSimulation.TaskGantt = data.TaskGantt;
            base.ApplyFilterTimeInterval();
        }

        #endregion

        #region RefreshLocalData
        private List<TaskData> RemoveEmptyParentNodes(List<TaskData> tasks)
        {
            var result = new List<TaskData>(tasks);
            var groupingTasks = tasks.Where(t => t.levelTask == 1 || t.levelTask == 2).ToList();

            foreach (var group in groupingTasks)
            {
                var children = tasks.Where(t => t.parentId == group.id).ToList();
                var visibleChildren = children.Where(c => !_deletedTaskIds.Contains(c.id)).ToList();
                if (!visibleChildren.Any())
                    result.Remove(group);
            }
            return result;
        }

        private void UpdateLocalLockState(TaskData task, bool isLock, DateTime blockDate)
        {
            var ganttItem = DataSimulation.TaskGantt.FirstOrDefault(x => x.id == task.id);
            if (ganttItem != null)
            {
                ganttItem.isBlock = isLock;

                if (isLock)
                    ganttItem.blockDate = new DateTimeOffset(blockDate).AddHours(-GanttSetup.Offset);
                else
                    ganttItem.blockDate = DateTimeOffset.MinValue;
            }

            var planningItem = DataSimulation.itemsPlanning
                .FirstOrDefault(x => x.WorkOrderPlanning?.InputOrderId == task.InputOrderId);

            if (planningItem?.WorkOrderPlanning?.InputOrder != null)
            {
                var input = planningItem.WorkOrderPlanning.InputOrder;
                input.IsBlocked = isLock;

                if (isLock)
                {
                    input.BlockDate = blockDate.AddHours(-GanttSetup.Offset);
                    input.EndBlockDate = blockDate.AddHours(-GanttSetup.Offset);
                }
                else
                {
                    input.BlockDate = null;
                    input.EndBlockDate = null;
                }
            }
        }

        private async Task UpdatePriorityInGantt(List<TaskData> tasks, string newPriority)
        {
            if (_ganttModule != null && tasks.Any())
            {
                var taskIds = tasks.Select(t => t.id).ToList();
                await _ganttModule.InvokeVoidAsync("updateTaskPriority", taskIds, l.Loc(newPriority));
            }
        }
        #endregion
    }
}
