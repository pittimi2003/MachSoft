using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Services;
using System.Collections.ObjectModel;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Components.BaseComponent;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.Users
{
    public partial class Users : PageOperations, IDisposable
    {
        #region Fields

        private EventActions SelectMode = EventActions.UnSelected;

        #endregion

        #region Properties

        [Inject]
        private IContextConfig _contextConfig { get; set; }
        [Inject]
        private IInitialDataService _initialDataService { get; set; }

        [Inject]
        private LocalStorageService _storageService { get; set; }
        private List<EventActions> Actions { get; set; }

        private string EventListener { get; set; } = string.Empty;

        [Inject]
        private IEventServices eventServices { get; set; }

        [Inject]
        private IInitialDataService _InitialDataService { get; set; }
        [Inject]
        private SendParamService _SendParamService { get; set; }

        [Inject] ILocalizationService _localizationService { get; set; }

        private GetAttributesDto<List<UserDto>> GetParametersDto { get; set; } 

        private bool IsEditMode { get; set; } = false;
        private bool IsNewMode { get; set; } = false;
        private bool showChangePassword { get; set; } = false;
        private string UserPassword { get; set; } = string.Empty;
        private string PasswordErrorMessage { get; set; } = string.Empty;
        private UserDto? SelectedUser { get; set; } = null;
        private List<UserDto?> lstSelectedUsers { get; set; } = new();
        private List<Guid> SelectedUserIds = new();
        private ReadOnlyCollection<TimeZoneInfo> TimeZones { get; set; }
        private List<WarehouseDto> warehouses { get; set; } = new();
        [Inject]
        private IUserService UserService { get; set; }

        private BaseComponente<UserDto>? baseComponentRef;
        private bool savebtn { get; set; } = false;
        private string EmiterContainer { get; set; }
        [Inject] private IMlxDialogService DialogService { get; set; }

        #endregion

        #region Methods
        public override bool ToolBarActionsNew(EventArguments eventArguments)
        {
            lstSelectedUsers.Clear();
            SelectedUserIds.Clear();
            IsNewMode = true;
            IsDetailsVisible = false;
            AddNewItem();
            lstSelectedUsers.Add(SelectedUser);
            return true;
        }

        public override async Task<bool> ToolBarActionsChangePassword(EventArguments eventArguments)
        {
            if (lstSelectedUsers != null)
            {
                showChangePassword = true;
                UserPassword = lstSelectedUsers[0].Password;
                StateHasChanged();
            }

            return true;
        }

        private void HandleValidationError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                PasswordErrorMessage = error;
            }
            else
            {
                PasswordErrorMessage = string.Empty;
            }

            StateHasChanged();
        }

        private void UpdatePassword(string newValue)
        {
            UserPassword = newValue;
        }

        private void CancelChangePassword() => showChangePassword = false;

        private async void ChangePassword()
        {
            showChangePassword = false;
            lstSelectedUsers[0].Password = UserPassword;

            await UserService.UpdateUser(lstSelectedUsers);
            SelectedUser = null;
            GetParametersDto.Model = GetUsers();
            IsDetailsVisible = false;
            IsEditMode = false;
            SelectMode = EventActions.UnSelected;
            eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
            StateHasChanged();

        }

        public override async Task<bool> ToolBarActionsEnabled(EventArguments eventArguments)
        {
            bool resp = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Are you sure you want to enable user?"));

            if (resp && lstSelectedUsers != null)
            {
                foreach (UserDto user in lstSelectedUsers)
                {
                    user.IsEnabled = true;
                }

                await UserService.UpdateUser(lstSelectedUsers);
                SelectedUser = null;
                GetParametersDto.Model = GetUsers();
                IsDetailsVisible = false;
                IsEditMode = false;
                SelectMode = EventActions.UnSelected;
                eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                _SendParamService.ReloadInitData = true;
                StateHasChanged();
            }

            return true;
        }

        public override async Task<bool> ToolBarActionsDisabled(EventArguments eventArguments)
        {
            var code = _InitialDataService.GetDatauser().Code;

            if (!lstSelectedUsers.Any(x => x.Code == code))
            {
                bool resp = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Are you sure you want to disable user?"));

                if (resp && lstSelectedUsers != null)
                {
                    foreach (UserDto user in lstSelectedUsers)
                    {
                        user.IsEnabled = false;
                    }

                    await UserService.UpdateUser(lstSelectedUsers);
                    SelectedUser = null;
                    GetParametersDto.Model = GetUsers();
                    IsDetailsVisible = false;
                    IsEditMode = false;
                    SelectMode = EventActions.UnSelected;
                    eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                    StateHasChanged();
                }
            }
            else
            {
                bool resp = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Unable to disable current user!"), false, "", true, Loc("Close"));
            }

            return true;
        }

        public override bool ToolBarActionsUpdate(EventArguments eventArguments)
        {
            if (SelectedUser != null)
            {
                IsEditMode = true;
                isExpandGrid = false;
                IsDetailsVisible = true;
            }
            return true;
        }

        public override async Task<bool> ToolBarActionsDelete(EventArguments eventArguments)
        {
            var code = _InitialDataService.GetDatauser().Code;
            if (!lstSelectedUsers.Any(x => x.Code == code))
            {
                bool resp = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Are you sure you want to delete {0}?", string.Join(", ", lstSelectedUsers.Select(u => u.Code))));

                if (resp && lstSelectedUsers != null)
                {
                    await UserService.DeleteUser(lstSelectedUsers);
                    SelectedUser = null;
                    GetParametersDto.Model = GetUsers();
                    IsDetailsVisible = false;
                    IsEditMode = false;
                    SelectMode = EventActions.UnSelected;
                    eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                    _SendParamService.ReloadInitData = true;
                    StateHasChanged();
                }
            }
            else
            {
                bool resp = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Unable to delete current user!"), false, "", true, Loc("Close"));
            }
            return true;
        }

        public override async Task SaveEvent()
        {
            try
            {
                var isValid = baseComponentRef != null ? baseComponentRef.Validate() : false;

                if (IsNewMode && SelectedUser != null && isValid)
                {
                    await UserService.AddUser(SelectedUser);
                    IsNewMode = false;
                    SelectedUser = null;

                    GetParametersDto.Model = GetUsers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el registro: {ex.Message}");
            }

            StateHasChanged();
        }

        public override void CancelEvent()
        {
            IsNewMode = false;
            SelectMode = EventActions.UnSelected;
        }

        public override bool GridActionsSelected(EventArguments eventArguments)
        {
            isExpandGrid = true;
            SelectedUser = null;
            lstSelectedUsers.Clear();
            SelectedUserIds.Clear();
            IsDetailsVisible = false;
            SelectMode = EventActions.UnSelected;

            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count == 1)
            {
                lstSelectedUsers.Add(selectedItems.FirstOrDefault() as UserDto);
                SelectedUser = selectedItems.FirstOrDefault() as UserDto;
                SelectedUserIds = lstSelectedUsers.Select(w => w.Id).ToList();
                IsDetailsVisible = true;
                SelectMode = SelectedUser.IsEnabled ? EventActions.Disabled : EventActions.Enabled;
                isExpandGrid = false;
            }

            return true;
        }

        public override bool GridActionsMultiSelected(EventArguments eventArguments)
        {

            if ((eventArguments.EventData as IEnumerable<object>)?.Cast<UserDto>().ToList() is List<UserDto> selectedItems && selectedItems.Count >= 1)
            {
                lstSelectedUsers.Clear();
                lstSelectedUsers = selectedItems;
                SelectedUser = selectedItems.FirstOrDefault() as UserDto;
                SelectedUserIds = lstSelectedUsers.Select(w => w.Id).ToList();
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
                if (eventArguments.EventData is List<UserDto> usersToUpdate)
                {
                    var currentUserBeforeUpdate = _InitialDataService.GetDatauser();
                    bool isCurrentUserBeingUpdated = usersToUpdate.Any(u => u.Id == currentUserBeforeUpdate?.Id);

                    await UserService.UpdateUser(usersToUpdate);

                    GetParametersDto.Model = GetUsers();
                    IsEditMode = false;

                    if (isCurrentUserBeingUpdated)
                    {
                        var currentUserAfterUpdate = UserService.GetUserByCode(currentUserBeforeUpdate.Code);
                        bool cultureChanged = currentUserBeforeUpdate.CultureCode !=
                                              currentUserAfterUpdate.RegionalSettings.Language.InternationalCode;

                        if (cultureChanged)
                        {
                            await _localizationService.ChangeCultureAsync(currentUserAfterUpdate.RegionalSettings.Language?.InternationalCode);
                            return true;
                        }
                    }

                    _SendParamService.ReloadInitData = true;
                    eventServices.Publish(
                        GetParametersDto.NameContainer,
                        new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));

                    StateHasChanged();
                    return true;
                }
                else

                    throw new InvalidOperationException("Invalid data type for update action.");

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override bool GridActionsRefresh(EventArguments eventArguments)
        {
            GetParametersDto.Model = GetUsers();
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
            GetParametersDto.Model = GetUsers();

            GetParametersDto.NameContainer = nameof(Users);
            GetParametersDto.GridPublish = $"{nameof(GridActions)}{nameof(Users)}";
            GetParametersDto.ToolBarPublish = $"{nameof(ToolBarActions)}{nameof(Users)}";
            GetParametersDto.DetailPublish = $"{nameof(DetailsAction)}{nameof(Users)}";

            EventListener = $"{nameof(ToolBarActions)}{nameof(Users)}";
            EmiterContainer = $"{nameof(Users)}{nameof(BaseComponent)}";
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Subscribe(nameof(Users), EmiterContainer, BaseComponentActions);

            Actions = new List<EventActions>()
            {
                {EventActions.New},
                {EventActions.Edit},
                {EventActions.Delete},
                {EventActions.Enabled},
                {EventActions.Disabled},
                {EventActions.ChangePassword}
            };
        }

        public void BaseComponentActions(EventArguments eventArguments)
        {
            savebtn = (eventArguments.EventActions == EventActions.SaveValid && eventArguments.EventData is bool flag && flag);
            StateHasChanged();
        }

        private void AddNewItem()
        {
            SelectedUser = UserDto.NewDto();

            if (_initialDataService.Organization is OrganizationDto organization)
            {
                SelectedUser.RegionalSettings.DecimalSeparator = organization.RegionalSettings.DecimalSeparator;
                SelectedUser.RegionalSettings.ThousandsSeparator = organization.RegionalSettings.ThousandsSeparator;
                SelectedUser.RegionalSettings.DateFormat = organization.RegionalSettings.DateFormat;
                SelectedUser.RegionalSettings.HourFormat = organization.RegionalSettings.HourFormat;
                SelectedUser.RegionalSettings.Language = organization.RegionalSettings.Language;
            }
        }

        private List<UserDto> GetUsers()
        {
            var organizationBD = _contextConfig.GetOnlyOrganization();
            return UserService.GetUsers(organizationBD.Id).OrderBy(x => x.Code).ToList();
        }

        private void OnRightSideVisibilityChanged(bool isVisible)
        {
            Console.WriteLine($"Right sidebar visibility changed: {isVisible}");
        }

        private void SaveDetails(WarehouseDto updatedWarehouse)
        {
            // Actualizar el registro en el modelo
            var index = warehouses.FindIndex(w => w.Id == updatedWarehouse.Id);
            if (index >= 0)
            {
                warehouses[index] = updatedWarehouse;
            }

            // Ocultar el DetailsComponent después de guardar
            SelectedUser = null;
            IsDetailsVisible = false;
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
            eventServices.Unsubscribe(nameof(Users), EmiterContainer, BaseComponentActions);
        }

        #endregion

    }
}
