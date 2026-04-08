using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;

namespace Mss.WorkForce.Code.Web.Services
{
    public class ContextConfig : IContextConfig
    {

        #region Fields

        private readonly DataAccess _dataAccess;

        #endregion

        #region Constructors

        public ContextConfig(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        #endregion

        #region Methods

        public IEnumerable<Area> GetArea() => _dataAccess.GetAllAreas().OrderBy(x => x.Name);

        public IEnumerable<Area> GetAreas() => _dataAccess.GetAreas().OrderBy(x => x.Name);
        public IEnumerable<Area> GetAreasByWarehouse(Guid IdWarehouse) => _dataAccess.GetAreasByIdWarehouse(IdWarehouse).OrderBy(x => x.Name);

        public IEnumerable<AvailableWorker> GetAvailableWorkers() => _dataAccess.GetAllAvailableWorkers().OrderBy(x => x.Name);

        public IEnumerable<Break> GetBreaks() => _dataAccess.GetAllBreaks().OrderBy(x => x.Name);

        public IEnumerable<Break> GetBreaksByWarehouse(Guid whId) => _dataAccess.GetAllBreaks(whId).OrderBy(x => x.Name);

        public IEnumerable<BreakProfile> GetBreakProfiles() => _dataAccess.GetAllBreakProfiles().OrderBy(x => x.Name);

        public IEnumerable<BreakProfile> GetBreakProfiles(Guid warehouseId) => _dataAccess.GetBreakProfiles(warehouseId).OrderBy(x => x.Name);

        public IEnumerable<Country> GetCountries() => _dataAccess.GetCountries().OrderBy(x => x.Name);

        public IEnumerable<DateFormat> GetDateFormats() => _dataAccess.GetDateFormats().OrderBy(x => x.Name);

        public IEnumerable<HourFormat> GetHourFormats() => _dataAccess.GetHourFormats().OrderBy(x => x.Name);

        public IEnumerable<DecimalSeparator> GetDecimalSeparator() => _dataAccess.GetDecimalSeparator().OrderBy(x => x.Name);

        public IEnumerable<Language> GetLanguages() => _dataAccess.GetLanguages();

        public IEnumerable<Layout> GetLayouts() => _dataAccess.GetAllLayouts().OrderBy(x => x.Name);

        public IEnumerable<LoadProfile> GetLoadProfileCatalogue(Guid warehouseId) => _dataAccess.GetLoadProfiles(warehouseId).OrderBy(x => x.Name);

        public IEnumerable<LoadProfile> GetLoadProfileCatalogueNoFiltered() => _dataAccess.GetLoadProfilesNoFiltered().OrderBy(x => x.Name);

        public Organization GetOnlyOrganization()
        {
            return _dataAccess.GetOnlyOrganization();
        }

        public IEnumerable<Process> GetProcesses() => _dataAccess.GetAllProcess().OrderBy(x => x.Name);

        public IEnumerable<Process> GetProcessesByWarehouse(Guid whId) => _dataAccess.GetAllProcessByWarehouse(whId).OrderBy(x => x.Name);

        public IEnumerable<Rol> GetRoles() => _dataAccess.GetAllRoles().OrderBy(x => x.Name);

        public IEnumerable<Rol> GetRoles(Guid warehouseId) => _dataAccess.GetRoles(warehouseId).OrderBy(x => x.Name);

        public IEnumerable<Shift> GetShifts() => _dataAccess.GetAllShifts().OrderBy(x => x.Name);

        public IEnumerable<Shift> GetShifts(Guid warehouseId) => _dataAccess.GetShifts(warehouseId).OrderBy(x => x.Name);

        public IEnumerable<SiteModel> GetSites(Guid OrganizationId)
        {
            return _dataAccess.GetWarehouse(OrganizationId).
                Select(entity => new SiteModel
                {
                    Id = entity.Id,
                    Name = entity.Name,
                });
        }

        public IEnumerable<Zone> GetZones() => _dataAccess.GetZones().OrderBy(x => x.Name);

        public IEnumerable<SystemOfMeasurement> GetSystemOfMeasurements() => _dataAccess.GetSystemOfMeasurements().OrderBy(x => x.Name);

        public IEnumerable<Team> GetTeams() => _dataAccess.GetAllTeams().OrderBy(x => x.Name);

        public IEnumerable<Team> GetTeams(Guid warehouseId) => _dataAccess.GetTeams(warehouseId).OrderBy(x => x.Name);

        public IEnumerable<ThousandsSeparator> GetThousandsSeparator() => _dataAccess.GetThousandsSeparator().OrderBy(x => x.Name);

        public IEnumerable<Models.Models.TimeZone> GetTimeZones() => _dataAccess.GetTimeZones().OrderBy(x => x.Name);

        public IEnumerable<TypeEquipment> GetTypeEquipmentCatalogue(Guid warehouseId) => _dataAccess.GetEquipmentTypes(warehouseId).OrderBy(x => x.Name);

        public IEnumerable<TypeEquipment> GetTypeEquipmentCatalogueNoFiltered() => _dataAccess.GetEquipmentTypesNoFiltered().OrderBy(x => x.Name);

        public IEnumerable<VehicleProfile> GetVehicleProfileCatalogue(Guid warehouseId) => _dataAccess.GetVehicleProfiles(warehouseId).OrderBy(x => x.Name);
        public IEnumerable<VehicleProfile> GetVehicleProfileCatalogueNoFiltered() => _dataAccess.GetVehicleProfilesNoFiltered().OrderBy(x => x.Name);

        public IEnumerable<Warehouse> GetWarehouse(Guid OrganizationId)
        {
            return _dataAccess.GetWarehouse(OrganizationId);
        }

        public Guid GetWarehouseId()
        {
            return _dataAccess.GetWarehouseId();
        }

        public IEnumerable<Warehouse> GetWarehouses() => _dataAccess.GetWarehouses().OrderBy(x => x.Name);

        public IEnumerable<User> GetUsers() => _dataAccess.GetUsers().OrderBy(x => x.Code);

        public IEnumerable<Worker> GetWorkers() => _dataAccess.GetAllWorkers().OrderBy(x => x.Name);

        public IEnumerable<LoadProfile> GetLoadCatalogue() => _dataAccess.GetLoadProfiles2().OrderBy(x => x.Name);

        public IEnumerable<VehicleProfile> GetVehicleCatalogue() => _dataAccess.GetVehicleProfiles2().OrderBy(x => x.Name);

        public IEnumerable<Schedule> GetSchedule(Guid TemporalidadId) => _dataAccess.GetAllSchedule(TemporalidadId);

        public IEnumerable<Warehouse> GetWarehouseProjectsByUserId(Guid UserGuid) => _dataAccess.GetWarehouseProjectsByUserId(UserGuid);

        public IEnumerable<Warehouse> GetWarehouseEditProjectsByUserId(Guid UserGuid, Guid layoutId) => _dataAccess.GetWarehouseEditProjectsByUserId(UserGuid, layoutId);

        public IEnumerable<Warehouse> GetWarehousesByUserId(Guid UserGuid) => _dataAccess.GetWarehouseByUserId(UserGuid);

        #endregion
    }
}
