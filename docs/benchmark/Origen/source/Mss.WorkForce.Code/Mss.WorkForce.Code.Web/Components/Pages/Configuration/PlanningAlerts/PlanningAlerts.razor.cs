using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.BaseComponent;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.PlanningAlerts
{
    public partial class PlanningAlerts : PageOperations, IDisposable
    {
        #region Fields

        private EventActions SelectMode = EventActions.UnSelected;

        #endregion

        #region Properties

        [Inject]
        private IContextConfig _contextConfig { get; set; }
        private List<EventActions> Actions { get; set; }

        private string EventListener { get; set; } = string.Empty;

        [Inject]
        private IEventServices eventServices { get; set; }

        private GetAttributesDto<List<PlanningAlertsDto>> GetParametersDto { get; set; }
        private bool IsDetailsVisible { get; set; } = false;
        private bool IsEditMode { get; set; } = false;
        private bool IsNewMode { get; set; } = false;
        private PlanningAlertsDto? SelectedProcess { get; set; } = null;
        private List<PlanningAlertsDto?> lstSelectedProcess { get; set; } = new();
        private List<Guid> SelectedProcessIds = new();
        private List<PlanningAlertsDto> processes { get; set; } = new();
        [Inject]
        private ICatalogueService<PlanningAlertsDto>? ProcessService { get; set; }

        private BaseComponente<PlanningAlertsDto>? baseComponentRef;
        private bool savebtn { get; set; } = false;
        private bool isExpandGrid { get; set; } = true;
        private string EmiterContainer { get; set; }
        [Inject] private IMlxDialogService DialogService { get; set; }

        #endregion

        #region Methods

        public override bool ToolBarActionsNew(EventArguments eventArguments)
        {
            lstSelectedProcess.Clear();
            SelectedProcessIds.Clear();
            IsNewMode = true;
            IsDetailsVisible = false;
            AddNewItem();
            lstSelectedProcess.Add(SelectedProcess);
            return true;
        }

        public override bool ToolBarActionsUpdate(EventArguments eventArguments)
        {
            if (SelectedProcess != null)
            {
                IsEditMode = true;
            }
            return true;
        }

        public override async Task<bool> ToolBarActionsDelete(EventArguments eventArguments)
        {
            bool resp = await DialogService.ShowDialogAsync("NOTIFICATION", $"Are you sure you want to delete {lstSelectedProcess?.Count} items?");

            if (resp && lstSelectedProcess != null)
            {
                await ProcessService.DeleteItems(lstSelectedProcess);
                SelectedProcess = null;
                GetParametersDto.Model = GetItems();
                IsDetailsVisible = false;
                SelectMode = EventActions.UnSelected;
                eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                StateHasChanged();
            }

            return true;
        }

        public override void CancelEvent()
        {
            IsNewMode = false;
            SelectMode = EventActions.UnSelected;
        }

        public override async Task SaveEvent()
        {
            try
            {
                var isValid = baseComponentRef != null ? baseComponentRef.Validate() : false;

                if (IsNewMode && SelectedProcess != null && isValid)
                {
                    await ProcessService.AddItem(SelectedProcess);
                    IsNewMode = false;
                    SelectedProcess = null;

                    GetParametersDto.Model = GetItems();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el registro: {ex.Message}");
            }

            StateHasChanged();
        }

        public override bool GridActionsSelected(EventArguments eventArguments)
        {
            isExpandGrid = true;
            SelectedProcess = null;
            lstSelectedProcess.Clear();
            SelectedProcessIds.Clear();
            IsDetailsVisible = false;
            SelectMode = EventActions.UnSelected;

            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count == 1)
            {
                lstSelectedProcess.Add(selectedItems.FirstOrDefault() as PlanningAlertsDto);
                SelectedProcess = selectedItems.FirstOrDefault() as PlanningAlertsDto;
                SelectedProcessIds = lstSelectedProcess.Select(w => w.Id).ToList();
                IsDetailsVisible = true;
                SelectMode = EventActions.Selected;
                isExpandGrid = false;
            }

            return true;
        }

        public override bool GridActionsMultiSelected(EventArguments eventArguments)
        {
            lstSelectedProcess.Clear();
            SelectedProcessIds.Clear();
            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count >= 1)
            {
                foreach (object selectedItem in selectedItems)
                {
                    if (!lstSelectedProcess.Contains(selectedItem))
                        lstSelectedProcess.Add(selectedItem as PlanningAlertsDto);
                }
                SelectedProcess = selectedItems.FirstOrDefault() as PlanningAlertsDto;
                SelectedProcessIds = lstSelectedProcess.Select(w => w.Id).ToList();
                SelectMode = EventActions.MultiSelected;
                IsDetailsVisible = true;
                isExpandGrid = false;
            }

            return true;
        }

        public override bool DetailsActionsCancel(EventArguments eventArguments)
        {
            IsEditMode = false;
            StateHasChanged();
            return true;
        }

        public override async Task<bool> DetailsActionsUpdate(EventArguments eventArguments)
        {
            try
            {
                if (eventArguments.EventData is List<PlanningAlertsDto> item)
                {
                    await ProcessService.UpdateItems(item);
                    GetParametersDto.Model = GetItems();
                    IsEditMode = false;
                    eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                    StateHasChanged();
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("Invalid data type for update action.");
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool GridActionsRefresh(EventArguments eventArguments)
        {
            GetParametersDto.Model = GetItems();
            eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
            return true;
        }
        public override bool GridActionsCollapseGrid(EventArguments eventArguments)
        {
            IsDetailsVisible = true;
            isExpandGrid = false;
            StateHasChanged();
            return true;
        }

        public override bool GridActionsExpandGrid(EventArguments eventArguments)
        {
            IsDetailsVisible = false;
            isExpandGrid = true;
            StateHasChanged();
            return true;
        }

        protected override void OnInitialized()
        {
            GetParametersDto = new(l);
            GetParametersDto.Model = GetItems();
            GetParametersDto.NameContainer = nameof(PlanningAlerts);
            GetParametersDto.GridPublish = $"{nameof(GridActions)}{nameof(PlanningAlerts)}";
            GetParametersDto.ToolBarPublish = $"{nameof(ToolBarActions)}{nameof(PlanningAlerts)}";
            GetParametersDto.DetailPublish = $"{nameof(DetailsAction)}{nameof(PlanningAlerts)}";

            EventListener = $"{nameof(ToolBarActions)}{nameof(PlanningAlerts)}";
            EmiterContainer = $"{nameof(PlanningAlerts)}{nameof(BaseComponent)}";
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Subscribe(nameof(PlanningAlerts), EmiterContainer, BaseComponentActions);

            Actions = new List<EventActions>();
        }

        public void BaseComponentActions(EventArguments eventArguments)
        {
            savebtn = (eventArguments.EventActions == EventActions.SaveValid && eventArguments.EventData is bool flag && flag);
            StateHasChanged();
        }

        private void AddNewItem()
        {
            SelectedProcess = PlanningAlertsDto.NewDto();
        }

        private List<PlanningAlertsDto> GetItems()
        {
            return ProcessService.GetItems().Cast<PlanningAlertsDto>().ToList();
        }


        private void OnRightSideVisibilityChanged(bool isVisible)
        {
            Console.WriteLine($"Right sidebar visibility changed: {isVisible}");
        }

        private void ToggleDetailsSidebar()
        {
            IsDetailsVisible = !IsDetailsVisible;
        }

        protected override void LoadData()
        {
            // Lógica específica para este componente

        }

        public void Dispose()
        {
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Unsubscribe(nameof(PlanningAlerts), EmiterContainer, BaseComponentActions);
        }

        #endregion
    }
}
