using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.SignalR;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Interfaces
{
    public interface IGanttOperations
    {
        #region Properties
        bool BackGroudShading { get; set; }
        Guid CurrentPlanning { get; set; }
        SiteModel CurrentSite { get; set; }
        string FilterSummaryTextPlanning { get; set; }
        string FilterSummaryTextYard { get; set; }
        GanttSetup GanttSetup { get; set; }
        bool OnSwitchView { get; set; }
        bool RepaintDataGantt { get; set; }
        bool ShowInfoDrawer { get; set; }
        List<SiteModel> Sites { get; set; }
        IMlxDialogService _DialogService { get; set; }
        EnumViewPlanning TypeView { get; set; }
        IInitialDataService _InitialDataService { get; set; }
        ProtectedLocalStorage _LocalStorage { get; set; }
        DateTimeOffset LastUpdateGantt { get; set; }
        bool ReloadDashboard { get; set; }
        YardResourceFilter? YardResourceFilter { get; set; }
        UserFormatOptions userFormat { get; set; }
        #endregion

        #region Methods
        Task LoadDataSimulation();

        Task LoadDataSimulation(Guid planningId);

        void LoadGanttSetupData(EnumViewPlanning view);

        Task LoadGanttServiceAsync();

        Task GetUserDataAsync();

        Task OnSiteSeleccionadaChanged((SiteModel newSite, bool changeColumns) args);

        Task OnSwitchChanged(bool changeValue);

        Task ReloadGanttSetupData();

        Task RepaintGanttAsync(bool isPlanning, bool showDashboard);

        void ChangeGanttRepaint(bool repaintData);

        void ShowHidePanel(bool IsPanelVisible, bool ISBackGroundVisible);

        Task SuscribeEvents();

        string FormatFilterSummary(GanttFilterSettingsDto filter);

        void SettingsActions(EventArguments args);

        Task<GanttDataConvertDto<T>> applyFilter<T>(GanttDataConvertDto<T> ganttData) where T : GanttTaskBase;

        List<T> IncludeParentTasks<T>(List<T> allTasks, List<T> viewChild) where T : GanttTaskBase;

        List<T> CloneGanttList<T>(List<T> source);

        void LastDateUpdateGantt(Guid planningId);

        Task SavedFilter(GanttView ganttView);

        Task ClearFilter(eFilterType filterType);

        void ApplyFilterTimeInterval();

        Guid IsPlnanningInWarehouse(Guid planningId, Guid warehouseId);

        List<T> ApplyActionMenuContextual<T>(List<T> values);

        void Dispose();

        #endregion
    }
}