using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using TimeZone = Mss.WorkForce.Code.Models.Models.TimeZone;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IContextConfig
    {
        #region Methods

        Guid GetWarehouseId();
        Organization GetOnlyOrganization();
        IEnumerable<Warehouse> GetWarehouse(Guid OrganizationId);
        IEnumerable<SiteModel> GetSites(Guid OrganizationId);
        IEnumerable<ThousandsSeparator> GetThousandsSeparator();
        IEnumerable<DecimalSeparator> GetDecimalSeparator();
        IEnumerable<SystemOfMeasurement> GetSystemOfMeasurements();
        IEnumerable<Language> GetLanguages();
        IEnumerable<DateFormat> GetDateFormats();
        IEnumerable<HourFormat> GetHourFormats();
        IEnumerable<Country> GetCountries();
        IEnumerable<Warehouse> GetWarehouses();
        IEnumerable<User> GetUsers();
        IEnumerable<TimeZone> GetTimeZones();
        IEnumerable<Layout> GetLayouts();
        IEnumerable<Area> GetArea();
        IEnumerable<Area> GetAreas();
        IEnumerable<Area> GetAreasByWarehouse(Guid IdWarehouse);
        IEnumerable<Zone> GetZones();
        IEnumerable<AvailableWorker> GetAvailableWorkers();
        IEnumerable<Worker> GetWorkers();
        IEnumerable<Team> GetTeams();
        IEnumerable<Team> GetTeams(Guid warehouseId);
        IEnumerable<Shift> GetShifts();
        IEnumerable<Shift> GetShifts(Guid warehouseId);
        IEnumerable<Break> GetBreaks();
        IEnumerable<Break> GetBreaksByWarehouse(Guid warehouseId);
        IEnumerable<BreakProfile> GetBreakProfiles();
        IEnumerable<BreakProfile> GetBreakProfiles(Guid warehouseId);
        IEnumerable<Rol> GetRoles();
        IEnumerable<Rol> GetRoles(Guid warehouseId);
        IEnumerable<TypeEquipment> GetTypeEquipmentCatalogue(Guid warehouseId);
        IEnumerable<TypeEquipment> GetTypeEquipmentCatalogueNoFiltered();
        IEnumerable<Process> GetProcesses();
        IEnumerable<Process> GetProcessesByWarehouse(Guid whId);
        IEnumerable<LoadProfile> GetLoadProfileCatalogue(Guid warehouseId);
        IEnumerable<LoadProfile> GetLoadProfileCatalogueNoFiltered();
        IEnumerable<VehicleProfile> GetVehicleProfileCatalogue(Guid warehouseId);
        IEnumerable<VehicleProfile> GetVehicleProfileCatalogueNoFiltered();
        IEnumerable<LoadProfile> GetLoadCatalogue();
        IEnumerable<VehicleProfile> GetVehicleCatalogue();

        IEnumerable<Schedule> GetSchedule(Guid TemporalidadId);
        IEnumerable<Warehouse> GetWarehouseProjectsByUserId(Guid UserGuid);
        IEnumerable<Warehouse> GetWarehouseEditProjectsByUserId(Guid UserGuid, Guid layoutId);
        
        IEnumerable<Warehouse> GetWarehousesByUserId(Guid UserGuid);
        

        #endregion
    }
}
