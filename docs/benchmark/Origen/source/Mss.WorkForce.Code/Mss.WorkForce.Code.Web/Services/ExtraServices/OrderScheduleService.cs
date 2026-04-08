using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class OrderScheduleService : ICatalogueService<Models.DTO.ExtraConfiguration.OrderScheduleDto>
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public OrderScheduleService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddItem(Models.DTO.ExtraConfiguration.OrderScheduleDto item)
        {
            try
            {
                operationDB = new OperationDB();

                OrderSchedule bp = MapperOrderScheduleFromDto(item);
                operationDB.AddNew("OrderSchedules", bp);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("AvailableWorker added successfully: {AvailableWorker}", bp.ToString());

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the AvailableWorker");
                throw;
            }
        }

        public async Task DeleteItems(List<Models.DTO.ExtraConfiguration.OrderScheduleDto> lstItems)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (Models.DTO.ExtraConfiguration.OrderScheduleDto breakProfile in lstItems)
                {
                    operationDB.AddDelete("OrderSchedules", new EntityDto() { Id = breakProfile.Id });

                    await _simulateService.SaveChangesInDataBase(operationDB);

                    _logger.LogInformation("OrderSchedule removed successfully: {AreaName}", breakProfile.ToString());
                }
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error removing the BreakProfile");
                throw;
            }
        }

        public IEnumerable<Models.DTO.ExtraConfiguration.OrderScheduleDto> GetItems()
        {
            List<Models.DTO.ExtraConfiguration.OrderScheduleDto> breakProfileDto = new();

            try
            {
                foreach (var item in _dataAccess.GetAllOrderSchedules())
                {
                    breakProfileDto.Add(MapperOrderSchedule(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RolService::OrderScheduleDto => Error get list of roles in database");
            }

            return breakProfileDto;
        }

        public Models.DTO.ExtraConfiguration.OrderScheduleDto MapperOrderSchedule(OrderSchedule os)
        {
            return new Models.DTO.ExtraConfiguration.OrderScheduleDto { 
                Id = os.Id,
                EndHour = os.EndHour,
                IsOut = os.IsOut,
                InitHour = os.InitHour,
                NumberVehicles = os.NumberVehicles,
                SelectionLoadDto = MapLoad(os.Load),
                SelectionWarehouseDto = MapWarehouse(os.Warehouse),
                SelectionVehicleDto = MapVehicle(os.Vehicle),
            };
        }

        public SelectionVehicleDto MapVehicle(VehicleProfile vp)
        {
            return new SelectionVehicleDto
            {
                Id = vp.Id,
                Name = vp.Name,
            };
        }
        public SelectionWarehouseDto MapWarehouse(Warehouse vp)
        {
            return new SelectionWarehouseDto
            {
                Id = vp.Id,
                Name = vp.Name,
            };
        }
        public SelectionLoadDto MapLoad(LoadProfile vp)
        {
            return new SelectionLoadDto
            {
                Id = vp.Id,
                Name = vp.Name,
            };
        }

        public async Task UpdateItems(List<Models.DTO.ExtraConfiguration.OrderScheduleDto> lstItems)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (var item in lstItems)
                {

                    OrderSchedule bp = MapperOrderScheduleFromDto(item);

                    operationDB.AddUpdate("OrderSchedules", bp);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("BreakProflie updated successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updated the OrderScheduleDto");
                throw;
            }
        }

        public OrderSchedule MapperOrderScheduleFromDto(Models.DTO.ExtraConfiguration.OrderScheduleDto osdto)
        {
            var vehicle = _dataAccess.GetVehicleFromDto(osdto.SelectionVehicleDto.Id ?? new Guid());
            var load = _dataAccess.GetLoadFromDto(osdto.SelectionLoadDto.Id??new Guid());
            var warehouse = _dataAccess.GetWarehouseById(osdto.SelectionWarehouseDto.Id ?? new Guid());

            return new OrderSchedule
            {
                Id = osdto.Id,
                EndHour = osdto.EndHour,
                InitHour = osdto.InitHour,
                Load = load,
                LoadId = load.Id,
                NumberVehicles = osdto.NumberVehicles,
                Vehicle = vehicle,
                VehicleId = vehicle.Id,
                Warehouse = warehouse,
                WarehouseId = warehouse.Id,
                IsOut = osdto.IsOut,
            };
        }
    }
}
