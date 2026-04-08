using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.BaseComponent;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using Mss.WorkForce.Code.Web.Services;
using Mss.WorkForce.Code.Web.Services.Interfaces;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.Alert
{
    public partial class Alerts : PageOperations, IDisposable
    {

        #region Fields

        private BaseComponente<AlertDto>? baseComponentRef;
        private List<Guid> SelectedAlertIds = new();
        private EventActions SelectMode = EventActions.UnSelected;
        private string Data = string.Empty;
        private string Cancel = string.Empty;
        private string Save = string.Empty;
        private string SelectWarehouse = string.Empty;
        private string Accept = string.Empty;
        private string Select = string.Empty;
        private string Warehouse = string.Empty;
        #endregion

        #region Properties

        [Inject] private IInitialDataService _initialDataService { get; set; }
        private List<EventActions> Actions { get; set; }
        private string EmiterContainer { get; set; }
        private string EventListener { get; set; } = string.Empty;
        [Inject] private IEventServices eventServices { get; set; }
        [Inject] private IAlertService _alertService { get; set; }
        private GetAttributesDto<List<AlertDto>> GetParametersDto { get; set; }
        private bool IsEditMode { get; set; } = false;
        private bool IsNewMode { get; set; } = false;
        private List<AlertDto?> lstSelectedAlerts { get; set; } = new();
        private bool savebtn { get; set; } = false;
        private AlertDto? SelectedAlert { get; set; } = null;
        [Inject] private IMlxDialogService DialogService { get; set; }
        private bool IsModalWarehouse { get; set; } = false;
        public List<SiteModel> Sites = new List<SiteModel>();
        public SiteModel SelectedSite = new SiteModel();
        #endregion

        #region Methods
        public void BaseComponentActions(EventArguments eventArguments)
        {
            savebtn = (eventArguments.EventActions == EventActions.SaveValid && eventArguments.EventData is bool flag && flag);
            StateHasChanged();

        }

        public override void CancelEvent()
        {
            IsNewMode = false;
            SelectMode = EventActions.UnSelected;
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
                if (eventArguments.EventData is List<AlertDto> alertDto)
                {
                    await _alertService.UpdateAlert(alertDto);
                    GetParametersDto.Model = _alertService.GetAlertManagement().ToList();
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

        public override bool GridActionsMultiSelected(EventArguments eventArguments)
        {
            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count >= 1)
            {
                lstSelectedAlerts.Clear();
                SelectedAlertIds.Clear();

                lstSelectedAlerts.AddRange(selectedItems.OfType<AlertDto>());
                SelectedAlertIds = lstSelectedAlerts.Select(w => w.Id).ToList();
                SelectedAlert = selectedItems.FirstOrDefault() as AlertDto;
                SelectMode = EventActions.MultiSelected;
                IsDetailsVisible = true;
                isExpandGrid = false;
            }

            return true;
        }

        public override bool GridActionsRefresh(EventArguments eventArguments)
        {
            GetParametersDto.Model = _alertService.GetAlertManagement().ToList();
            eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
            return true;
        }

        public override bool GridActionsSelected(EventArguments eventArguments)
        {
            isExpandGrid = true;
            SelectedAlert = null;
            lstSelectedAlerts.Clear();
            SelectedAlertIds.Clear();
            IsDetailsVisible = false;
            SelectMode = EventActions.UnSelected;

            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count == 1)
            {
                lstSelectedAlerts.Add(selectedItems.FirstOrDefault() as AlertDto);
                SelectedAlert = selectedItems.FirstOrDefault() as AlertDto;
                SelectedAlertIds = lstSelectedAlerts.Select(w => w.Id).ToList();
                IsDetailsVisible = true;
                SelectMode = EventActions.Selected;
                isExpandGrid = false;
            }

            return true;
        }

        public override async Task SaveEvent()
        {
            try
            {
                var isValid = baseComponentRef != null ? baseComponentRef.Validate() : false;

                if (IsNewMode && SelectedAlert != null && isValid)
                {
                    await _alertService.AddAlert(SelectedAlert);
                    IsNewMode = false;
                    SelectedAlert = null;

                    GetParametersDto.Model = _alertService.GetAlertManagement().ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el registro: {ex.Message}");
            }

            StateHasChanged();
        }

        public override async Task<bool> ToolBarActionsDelete(EventArguments eventArguments)
        {
            int selectedCount = lstSelectedAlerts?.Count ?? 0;

            bool resp = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Are you sure you want to delete {0} alert{1}?", selectedCount, selectedCount == 1 ? "" : "s"));

            if (resp && lstSelectedAlerts.Any())
            {
                await _alertService.DeleteAlert(lstSelectedAlerts);
                SelectedAlert = null;
                GetParametersDto.Model = _alertService.GetAlertManagement().ToList();
                IsDetailsVisible = false;
                IsEditMode = false;
                SelectMode = EventActions.UnSelected;
                eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                StateHasChanged();
            }

            return true;
        }

        public override bool ToolBarActionsNew(EventArguments eventArguments)
        {
            lstSelectedAlerts.Clear();
            SelectedAlertIds.Clear();
            IsNewMode = true;
            IsDetailsVisible = false;
            AddNewItem();

            SelectedAlert.Message = string.IsNullOrEmpty(SelectedAlert.Message) ? "" : Loc(SelectedAlert.Message);

            lstSelectedAlerts.Add(SelectedAlert);
            return true;
        }

        public override bool ToolBarActionsUpdate(EventArguments eventArguments)
        {
            if (SelectedAlert != null)
            {
                IsEditMode = true;
                isExpandGrid = false;
                IsDetailsVisible = true;
            }
            return true;
        }

        protected override void LoadData()
        {
            // Lógica específica para este componente
        }

        protected override void OnInitialized()
        {
            LoadSites();
            TranslateFields();
            GetParametersDto = new(l);
            GetParametersDto.Model = _alertService.GetAlertManagement().ToList();

            GetParametersDto.NameContainer = nameof(Alerts);
            GetParametersDto.GridPublish = $"{nameof(GridActions)}{nameof(Alerts)}";
            GetParametersDto.ToolBarPublish = $"{nameof(ToolBarActions)}{nameof(Alerts)}";
            GetParametersDto.DetailPublish = $"{nameof(DetailsAction)}{nameof(Alerts)}";

            EventListener = $"{nameof(ToolBarActions)}{nameof(Alerts)}";
            EmiterContainer = $"{nameof(Alerts)}{nameof(BaseComponent)}";
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Subscribe(nameof(Alerts), EmiterContainer, BaseComponentActions);

            Actions = new()
            {
                {EventActions.New},
                {EventActions.Edit},
                {EventActions.Delete},
                {EventActions.Clone}
            };
        }
        private void TranslateFields()
        {
            Data = Loc("DATA");
            Cancel = Loc("Cancel");
            Save = Loc("Save");
            SelectWarehouse = Loc("Select warehouse");
            Accept = Loc("Accept");
            Select = Loc("Select...");
            Warehouse = Loc("Warehouse");
        }

        private void AddNewItem() => SelectedAlert = AlertDto.NewDto();

        private void OnRightSideVisibilityChanged(bool isVisible)
        {
            Console.WriteLine($"Right sidebar visibility changed: {isVisible}");
        }

        private void ToggleDetailsSidebar()
        {
            IsDetailsVisible = !IsDetailsVisible;
        }

        public void Dispose()
        {
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Unsubscribe(nameof(Alerts), EmiterContainer, BaseComponentActions);
        }

        public async override Task<bool> ToolBarActionsClone(EventArguments eventArguments)
        {
            if (SelectedAlert != null)
            {
                IsModalWarehouse = true;
            }
            return true;
        }

        private void HiddeModalWarehouse()
        {
            SelectedSite = new SiteModel();
            IsModalWarehouse = false;
        }

        private bool Validate()
        {
            return SelectedSite.Id == Guid.Empty;
        }

        private async Task CloneAlertWithWarehouse()
        {
            await _alertService.CloneAlert(SelectedAlert, SelectedSite);

            SelectedAlert = null;
            GetParametersDto.Model = _alertService.GetAlertManagement().ToList();
            IsDetailsVisible = false;
            SelectMode = EventActions.UnSelected;
            IsModalWarehouse = false;
            eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));

            StateHasChanged();
        }

        private void LoadSites()
        {
            Sites = dataAccess.GetWarehouse(dataAccess.GetOrganization().FirstOrDefault().Id)
            .Select(x => new SiteModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        #endregion

    }
}
