using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using Mss.WorkForce.Code.Web.Services;
using Mss.WorkForce.Code.Web.Components.PanelEditor;
using Microsoft.AspNetCore.Components.Routing;
using Mss.WorkForce.Code.Models.Common;
using DevExpress.Blazor;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.Resources
{
    public partial class Resources : IDisposable
    {

        #region Fields

        private BreakProfilesDto _breakProfile = new();

        private bool _urlCleaned = false;

        private int activeTabIndex;

        private bool IsEdit = false;

        private string MESSAGEDEPENDENCIES = "* To continue, you must complete the information in the Base profiles section.";

        private string MESSAGEDEPENDENCIESNOTIFICATION = "It is not possible to delete this profile, it is linked to another registry.";

        private string NOTIFICATIONTITLE = "Notification";

        private string WorkRolesNoDataMessage = "*To continue, you must create processes in the designer for this warehouse.";
        private string AreaEquipmentNoDataMessage = "*To continue, you must create areas in the designer for this warehouse.";

        private PanelEditor<BreakDto>? PanelBreak;

        private PanelEditor<ResourceWorkerScheduleDto>? PanelWorkerSchedule;

        private string titlePopup = string.Empty;

        #endregion

        #region Properties

        public List<IBaseModel> BreakProfiles { get; set; }

        public List<IBaseModel> Breaks { get; set; }

        public List<IBaseModel> Equipments { get; set; }

        public List<IBaseModel> EquipmentsGroups { get; set; }

        public bool IsBreakPopupVisible { get; set; } = false;

        public List<IBaseModel> Roles { get; set; }

        public List<IBaseModel> Shifts { get; set; }

        public List<IBaseModel> Teams { get; set; }

        public List<IBaseModel> Workers { get; set; }

        [Inject] private IContextConfig _contextConfig { get; set; }

        [Inject] private IResourceService _resourceService { get; set; }

        [Inject] private IMlxDialogService DialogService { get; set; }

        private bool HasDataBaseProfile { get; set; }

        public UserFormatOptions userFormat { get; set; } = new();

        private bool IsBreakProfileNameDuplicated => BreakProfiles.Cast<BreakProfilesDto>().Any(x => x.Id != _breakProfile.Id && string.Equals(x.Name, _breakProfile.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        private bool IsBreakProfileValid => !string.IsNullOrWhiteSpace(_breakProfile.Name) && !string.IsNullOrWhiteSpace(_breakProfile.Type) && !IsBreakProfileNameDuplicated;

        [Inject]
        private ILocalizationService Loc { get; set; }
        private SiteModel SelectedSite { get; set; }
        private List<SiteModel> Sites { get; set; }
        private Guid WarehouseId { get; set; } = Guid.Empty;

        #endregion

        #region Methods

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_urlCleaned)
                CleanNamePage();
        }

        protected override void OnInitialized()
        {
            Navigation.LocationChanged += OnLocationChanged;
            NavigationNewPage();
            SetActiveTab();
        }

        protected override async Task OnInitializedAsync()
        {
            TranslateResoruce();
            await GetUserDataAsync();
            await base.OnInitializedAsync();
        }

        public async Task GetUserDataAsync()
        {
            await _InitialDataService.GetDataUserLocal();
            userFormat = _InitialDataService.GetUserFormat();
        }

        private void CleanNamePage()
        {
            var uri = new Uri(Navigation.Uri);
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var cleanUrl = uri.GetLeftPart(UriPartial.Path);
                Navigation.NavigateTo(cleanUrl, replace: true);
                _urlCleaned = true;
            }
        }

        private async Task DeleteBreakProfile(Guid id)
        {
            GetBreaks();
            var dto = BreakProfiles.OfType<BreakProfilesDto>().FirstOrDefault(x => x.Id == id);

            if (Breaks.OfType<BreakDto>().Any(b => b.BreakProfileId == id))
            {
                await DialogService.ShowDialogAsync(NOTIFICATIONTITLE, MESSAGEDEPENDENCIESNOTIFICATION, true, Loc.Loc("Cancel"), false);
                return;
            }

            if (!await DialogService.ShowDialogAsync(NOTIFICATIONTITLE, Loc.Loc("Are you sure you want to delete the break profile \"{0}\"?", dto.Name), true, Loc.Loc("Cancel"), true, Loc.Loc("Delete")))
                return;

            dto.DataOperationType = OperationType.Delete;
            if (await _resourceService.BreakProfileSaveChanges(new[] { dto }, WarehouseId))
            {
                UpdateDataModels();
                await DialogService.ShowDialogAsync(NOTIFICATIONTITLE, Loc.Loc("Break profile \"{0}\" deleted successfully.", dto.Name), true, Loc.Loc("Ok"), false);
            }
        }

        private void GetBreakProfiles() => BreakProfiles = _resourceService.GetBreakProfiles(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetBreaks() => Breaks = _resourceService.GetBreaks(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetEquipmentGroups() => EquipmentsGroups = _resourceService.GetEquipmentGroups(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetEquipments() => Equipments = _resourceService.GetTypeEquipments(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetRoles() => Roles = _resourceService.GetRolProcesses(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetShifts() => Shifts = _resourceService.GetShifts(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetTeams() => Teams = _resourceService.GetTeams(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetWorkers() => Workers = _resourceService.GetWorkers(WarehouseId).Cast<IBaseModel>().ToList();

        private void NavigationNewPage()
        {
            var uri = new Uri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var tabFromUrl = query.Get("tab");
            if (!string.IsNullOrEmpty(tabFromUrl))
                _SendParamService.ActiveTab = tabFromUrl;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            CleanNamePage();
            SetActiveTab();
            StateHasChanged();
        }

        private void OnSiteSeleccionadaChanged((SiteModel newSite, bool changeColumns) args)
        {
            SelectedSite = args.newSite;
            UpdateDataModels();
        }

        private async Task OpenEditBreakProfile(Guid id)
        {
            var dto = BreakProfiles.OfType<BreakProfilesDto>().FirstOrDefault(x => x.Id == id);
            if (dto is null)
                return;

            _breakProfile = new BreakProfilesDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Type = dto.Type,
                WarehouseId = dto.WarehouseId,
                DataOperationType = OperationType.Update
            };
            titlePopup = Loc.Loc("Edit break profile");
            IsEdit = true;
            IsBreakPopupVisible = true;
        }

        private void ResetBreakProfileForm()
        {
            UpdateDataModels();
            _breakProfile = new();
            IsEdit = false;
        }

        private async Task SaveBreakProfile()
        {
            if (_breakProfile.Id == Guid.Empty)
                _breakProfile.Id = Guid.NewGuid();

            _breakProfile.DataOperationType = IsEdit ? OperationType.Update : OperationType.Insert;
            _breakProfile.WarehouseId = WarehouseId;
            IsBreakPopupVisible = false;

            if (await _resourceService.BreakProfileSaveChanges(new[] { _breakProfile }, WarehouseId))
            {
                await DialogService.ShowDialogAsync(
                   Loc.Loc("NOTIFICATION"),
                    IsEdit
                        ? Loc.Loc("Break profile \"{0}\" updated successfully.", _breakProfile.Name)
                        : Loc.Loc("Break profile \"{0}\" created successfully.", _breakProfile.Name),
                    true, Loc.Loc("Close"), false
                );
            }
            ResetBreakProfileForm();
        }

        private async Task SaveChangesInDataBase(IEnumerable<IBaseModel> data)
        {
            switch (data.FirstOrDefault())
            {
                case TypeEquipmentDto:
                    await _resourceService.EquipmentTypeSaveChanges(data.OfType<TypeEquipmentDto>(), WarehouseId);
                    break;

                case EquipmentGroupsDto:
                    await _resourceService.EquipmentGroupTypeSaveChanges(data.OfType<EquipmentGroupsDto>(), WarehouseId);
                    break;

                case ResourceWorkerScheduleDto:
                    await _resourceService.WorkersSaveChanges(data.OfType<ResourceWorkerScheduleDto>(), WarehouseId);
                    break;

                case TeamsDto:
                    await _resourceService.TeamsSaveChanges(data.OfType<TeamsDto>(), WarehouseId);
                    PanelWorkerSchedule?.GetCatalogues();
                    break;

                case ShiftsDto:
                    await _resourceService.ShiftSaveChanges(data.OfType<ShiftsDto>(), WarehouseId);
                    PanelWorkerSchedule?.GetCatalogues();
                    break;

                case BreakProfilesDto:
                    await _resourceService.BreakProfileSaveChanges(data.OfType<BreakProfilesDto>(), WarehouseId);
                    PanelBreak?.GetCatalogues();
                    break;

                case BreakDto:
                    await _resourceService.BreaksSaveChanges(data.OfType<BreakDto>(), WarehouseId);
                    PanelWorkerSchedule?.GetCatalogues();
                    break;

                case ResourceRolProcessDto:
                    await _resourceService.RolesProcessSaveChanges(data.OfType<ResourceRolProcessDto>(), WarehouseId);
                    PanelWorkerSchedule?.GetCatalogues();
                    break;
            }

            UpdateDataModels();
        }

        private void SetActiveTab()
        {
            activeTabIndex = _SendParamService.ActiveTab switch
            {
                "tab1" => 0,
                "tab2" => 1,
                "tab3" => 2,
                _ => 0
            };
        }

        void OnTabClick(TabClickEventArgs args)
        {
            _SendParamService.ActiveTab = args.TabIndex switch
            {
                 0 => "tab1",
                 1 => "tab2",
                 2 => "tab3",
                _ => "tab1"
            };
            _SendParamService.ReloadInitData = true;
            activeTabIndex = args.TabIndex;
        }

        void ShowNewBreakPopup()
        {
            _breakProfile = new();
            IsEdit = false;
            titlePopup = Loc.Loc("New break profile");
            IsBreakPopupVisible = true;
            GetBreakProfiles();
        }

        private void TranslateResoruce()
        {
            MESSAGEDEPENDENCIES = Loc.Loc("* To continue, you must complete the information in the Base profiles section.");
            MESSAGEDEPENDENCIESNOTIFICATION = Loc.Loc("It is not possible to delete this profile, it is linked to another registry.");
            NOTIFICATIONTITLE = Loc.Loc("Notification");
            WorkRolesNoDataMessage = Loc.Loc(WorkRolesNoDataMessage);
            AreaEquipmentNoDataMessage = Loc.Loc(AreaEquipmentNoDataMessage);
        }
        private void UpdateDataModels()
        {
            WarehouseId = SelectedSite?.Id ?? Guid.Empty;
            GetWorkers();
            GetRoles();
            GetTeams();
            GetShifts();
            GetBreaks();
            GetBreakProfiles();
            GetEquipmentGroups();
            GetEquipments();
            HasDataBaseProfile = (Teams?.Any() == true) && (Shifts?.Any() == true) && (BreakProfiles?.Any() == true) && (Roles?.Any() == true) ? false : true;
        }

        private string GetErrorMessageWorkRoles() => Roles != null &&  Roles.Any() ?  string.Empty: WorkRolesNoDataMessage;

        private string GetErrorMessageGroupEquipment() => EquipmentsGroups != null && EquipmentsGroups.Any() ? string.Empty : AreaEquipmentNoDataMessage;

        
        #endregion
    }
}
