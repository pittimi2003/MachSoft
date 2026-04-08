using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services
{
    public class WarehouseService : IWarehouseService
    {

        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;


        public WarehouseService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }

        public async Task DeleteWarehouse(List<WarehouseDto> lstWarehouse)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstWarehouse)
                {
                    // Buscar los flows asociados al warehouse
                    var flows = _dataAccess.GetFlowsByWarehouse(item.Id);

                    foreach (var flow in flows)
                    {
                        // Custom
                        var customFlows = _dataAccess.GetCustomFlowsByFlow(flow.Id);
                        foreach (var cf in customFlows)
                            operation.AddDelete("CustomFlowGraphs", new EntityDto() { Id = cf.Id });

                        // Outbound
                        var outboundFlows = _dataAccess.GetOutboundFlowsByFlow(flow.Id);
                        foreach (var of in outboundFlows)
                            operation.AddDelete("OutboundFlowGraphs", new EntityDto() { Id = of.Id });

                        // Inbound
                        var inboundFlows = _dataAccess.GetInboundFlowsByFlow(flow.Id);
                        foreach (var inf in inboundFlows)
                            operation.AddDelete("InboundFlowGraphs", new EntityDto() { Id = inf.Id });

                        // Finalmente el Flow
                        operation.AddDelete("Flow", new EntityDto() { Id = flow.Id });
                    }


                    // Finalmente, borrar el warehouse
                    operation.AddDelete("Warehouses", new EntityDto() { Id = item.Id });
                }


                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<WarehouseDto> GetWarehouses()
        {
            List<WarehouseDto> warehouses = new();

            try
            {
                foreach (var item in _dataAccess.GetWarehouses())
                {
                    warehouses.Add(MapperWarehouse(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WarehouseService::GetWarehouses => Error get list of warehouses in database");
            }

            return warehouses;
        }

        public WarehouseDto GetWarehousesById(Guid warehouseId)
        {
            return MapperWarehouse(_dataAccess.GetWarehouseById2(warehouseId));
        }

        public async Task AddWarehouse(WarehouseDto dto)
        {
            try
            {
                OperationDB operation = new OperationDB();
                var warehouse = MapDtoToWarehouse(dto);
                warehouse.OrganizationId = GetOnlyOrganization().Id;
                operation.AddNew("Warehouses", warehouse);
                operation.AddNew("ProcessPriorityOrder", new ProcessPriorityOrder { Id = Guid.NewGuid(), Code = "Committed Time", Priority = 0,  IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("ProcessPriorityOrder", new ProcessPriorityOrder { Id = Guid.NewGuid(), Code = "Priority", Priority = 1, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("ProcessPriorityOrder", new ProcessPriorityOrder { Id = Guid.NewGuid(), Code = "Release Time", Priority = 2, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("ProcessPriorityOrder", new ProcessPriorityOrder { Id = Guid.NewGuid(), Code = "Creation Time", Priority = 3, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("OrderPriority", new OrderPriority { Id = Guid.NewGuid(), Code = "Urgent", Priority = 0, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("OrderPriority", new OrderPriority { Id = Guid.NewGuid(), Code = "High", Priority = 1, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("OrderPriority", new OrderPriority { Id = Guid.NewGuid(), Code = "Normal", Priority = 2, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("OrderPriority", new OrderPriority { Id = Guid.NewGuid(), Code = "Low", Priority = 3, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("OrderPriority", new OrderPriority { Id = Guid.NewGuid(), Code = "Very Low", Priority = 4, IsActive = true, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("SLAConfigs", new SLAConfig { Id = Guid.NewGuid(), Code = "On Time Orders Percentage", Value = 0.95, WarehouseId = warehouse.Id, Warehouse = null });
                operation.AddNew("SLAConfigs", new SLAConfig { Id = Guid.NewGuid(), Code = "Shipped Stock", Value = 0.95, WarehouseId = warehouse.Id, Warehouse = null });

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WarehouseService::AddWarehouse => Error adding warehouse to database");
                throw;
            }
        }

        public Organization GetOnlyOrganization()
        {
            return _dataAccess.GetOnlyOrganization();
        }

        public async Task UpdateWarehouse(List<WarehouseDto> lstWarehouseDto)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstWarehouseDto) 
                {
                    var warehouse = MapDtoToWarehouse(item);
                    warehouse.OrganizationId = GetOnlyOrganization().Id;
                    operation.AddUpdate("Warehouses", warehouse);
                }                
                
                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WarehouseService::UpdateWarehouse => Error updating warehouse");
            }
        }

        private WarehouseDto MapperWarehouse(Warehouse warehouse)
        {
            WarehouseDto warehouseDto = new()
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Code = warehouse.Code,
                Adress = new Adress()
                {
                    AdressLine = warehouse.AddressLine,
                    City = warehouse.City,
                    Country = warehouse.Country,
                    State = warehouse.State,
                    ZipCode = warehouse.ZIPCode,
                    CompleteAdress = warehouse.Address,
                    Comment = warehouse.AddressComment
                },
                Contact = new Contact()
                {
                    Name = warehouse.ContactName,
                    Comment = warehouse.ContactComment,
                    Email = warehouse.Email,
                    Fax = warehouse.Fax,
                    Extension = warehouse.Extension,
                    Telephone = warehouse.Telephone,
                    TelephoneOther = warehouse.Telephone2,

                },
                Description = warehouse.Description,
                TimeZone = warehouse.TimeZone_,
                Mode = warehouse.Mode.ToString()
            };
        
            return warehouseDto;
        }

        private Warehouse MapDtoToWarehouse(WarehouseDto dto)
        {
            return new Warehouse
            {
                Id = dto.Id,
                Name = dto.Name,
                Code = dto.Code,
                AddressLine = dto.Adress.AdressLine,
                City = dto.Adress.City,
                CountryId = dto.Adress.Country.Id,
                State = dto.Adress.State,
                ZIPCode = dto.Adress.ZipCode,
                Address = dto.Adress.CompleteAdress,
                AddressComment = dto.Adress.Comment,
                ContactName = dto.Contact.Name,
                ContactComment = dto.Contact.Comment,
                MeasureSystem = "en",
                Email = dto.Contact.Email,
                Fax = dto.Contact.Fax,
                Extension = dto.Contact.Extension,
                Telephone = dto.Contact.Telephone,
                Telephone2 = dto.Contact.TelephoneOther,
                Description = dto.Description,
                TimeZoneId = dto.TimeZone.Id,
                Mode = Enum.Parse<SimulationMode>(dto.Mode)
            };
        }

    }
}
