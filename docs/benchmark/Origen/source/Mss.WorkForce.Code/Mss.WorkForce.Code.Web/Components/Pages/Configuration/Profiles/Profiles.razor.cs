using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.Profiles
{
    public partial class Profiles : IDisposable
    {
        #region Injections

        [Inject]
        private IContextConfig _contextConfig { get; set; }
        [Inject]
        private IProfileService _profileService { get; set; }
        [Inject]
        private ILocalizationService Loc { get; set; }
        #endregion

        #region Properties

        public SiteModel SelectedSite = new SiteModel();
        private Guid WarehouseId { get; set; }
        public List<IBaseModel> DataLoadProfile { get; set; } = new List<IBaseModel>();
        public List<IBaseModel> DataVehicleProfile { get; set; } = new List<IBaseModel>();
        public List<IBaseModel> DataPutawayProfile { get; set; } = new List<IBaseModel>();
        public List<IBaseModel> DataPostprocessProfile { get; set; } = new List<IBaseModel>();
        public List<IBaseModel> DataPreprocessProfile { get; set; } = new List<IBaseModel>();
        public List<IBaseModel> DataOrderLoadProperties { get; set; } = new List<IBaseModel>();
        public UserFormatOptions userFormat { get; set; } = new();

        private int activeTabIndex;
        private bool _urlCleaned = false;
        private bool HasDataBaseProfile { get; set; }

        private string MESSAGEDEPENDENCIES = "* To continue, you must complete the information in the Base profiles section.";
        #endregion

        #region Methods

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_urlCleaned)
                CleanNamePage();
        }
        public async Task GetUserDataAsync()
        {
            await _InitialDataService.GetDataUserLocal();
            userFormat = _InitialDataService.GetUserFormat();
        }

        protected override void OnInitialized()
        {
            Navigation.LocationChanged += OnLocationChanged;
            NavigationNewPage();
            SetActiveTab();
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            CleanNamePage();
            SetActiveTab();
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            TranslateResoruce();
            await GetUserDataAsync();
            await base.OnInitializedAsync();
        }

        protected void TranslateResoruce()
        {
              MESSAGEDEPENDENCIES = Loc.Loc("* To continue, you must complete the information in the Base profiles section.");
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

        private void NavigationNewPage()
        {
            var uri = new Uri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var tabFromUrl = query.Get("tab");
            if (!string.IsNullOrEmpty(tabFromUrl))
                _SendParamService.ActiveTab = tabFromUrl;
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

        private async Task OnSiteSeleccionadaChanged((SiteModel newSite, bool changeColumns) args)
        {
            SelectedSite = args.newSite;
            WarehouseId = SelectedSite.Id;
            LoadDataProfiles();
            StateHasChanged();
        }

        private void LoadDataProfiles()
        {
            GetDataLoadProfile();
            GetDataVehicleProfile();
            GetDataPutawayProfile();
            GetDataPostprocessProfile();
            GetDataPreprocessProfile();
            GetOrderLoadPropertiesProfile();
            SetDependecies();
            HasDataBaseProfile = (DataLoadProfile?.Any() == true) && (DataVehicleProfile?.Any() == true) ? false : true;
        }

        private void SetDependecies()
        {

            foreach (LoadProfileDto i in DataLoadProfile.OfType<LoadProfileDto>())
                i.SetDependencies(DataPreprocessProfile.OfType<OrderProfilesDto>().Any(x => x.Load.Id == i.Id) || DataOrderLoadProperties.OfType<OrderLoadPropertiesDto>().Any(x => x.Load.Id == i.Id));

            foreach (VehicleProfileDto i in DataVehicleProfile.OfType<VehicleProfileDto>())
                i.SetDependencies(DataPreprocessProfile.OfType<OrderProfilesDto>().Any(x => x.Vehicle.Id == i.Id) || DataOrderLoadProperties.OfType<OrderLoadPropertiesDto>().Any(x => x.Vehicle.Id == i.Id));
        }

        private async Task SaveChangesInDataBase(IEnumerable<IBaseModel> data)
        {
            switch (data.FirstOrDefault().GetType().Name)
            {
                case nameof(LoadProfileDto):
                    await _profileService.LoadProfileSaveChanges(data.OfType<LoadProfileDto>(), WarehouseId);
                    break;

                case nameof(VehicleProfileDto):
                    await _profileService.VehicleProfileSaveChanges(data.OfType<VehicleProfileDto>(), WarehouseId);
                    break;

                case nameof(PutawayProfileDto):
                    await _profileService.PutawayProfileSaveChanges(data.OfType<PutawayProfileDto>(), WarehouseId);
                    break;

                case nameof(PostprocessProfileDto):
                    await _profileService.PostprocessProfileSaveChanges(data.OfType<PostprocessProfileDto>(), WarehouseId);
                    break;

                case nameof(OrderProfilesDto):
                    await _profileService.PreprocessProfileSaveChanges(data.OfType<OrderProfilesDto>(), WarehouseId);
                    break;

                case nameof(OrderLoadPropertiesDto):
                    await _profileService.ProprocessProfileSaveChanges(data.OfType<OrderLoadPropertiesDto>(), WarehouseId);
                    break;
            }
            LoadDataProfiles();
            StateHasChanged();
        }

        #region DataProfile
        private void GetDataLoadProfile() => DataLoadProfile = _profileService.LoadProfiles(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetDataVehicleProfile() => DataVehicleProfile = _profileService.VehicleProfiles(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetDataPutawayProfile() => DataPutawayProfile = _profileService.PutawayProfiles(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetDataPostprocessProfile() => DataPostprocessProfile = _profileService.PostprocessProfiles(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetDataPreprocessProfile() => DataPreprocessProfile = _profileService.OrderSchedules(WarehouseId).Cast<IBaseModel>().ToList();

        private void GetOrderLoadPropertiesProfile() => DataOrderLoadProperties = _profileService.OrderLoadPropertiesProfile(WarehouseId).Cast<IBaseModel>().ToList();

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }

        #endregion

        #endregion
    }
}
