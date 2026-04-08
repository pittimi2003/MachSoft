using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.Interfaces;

namespace Mss.WorkForce.Code.Web.Services
{
    public class SimulationLogicService : ISimulationLogicService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<SimulationLogicService> _logger;
        private readonly ISimulateService _simulateService;


        public SimulationLogicService(DataAccess dataAccess, ILogger<SimulationLogicService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }

        public IEnumerable<SLAConfigDto> GetSLAConfigs() 
        { 
            List<SLAConfigDto> sLAConfigs = new List<SLAConfigDto>();

            try
            {
                foreach (var item in _dataAccess.GetAllSLAConfigs())
                {
                    sLAConfigs.Add(MapSLAConfig(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SimulationLogicService::GetSLAConfigs => Error get list of SLAConfigs in database");
            }

            return sLAConfigs;
        }

        public async Task UpdateProcessPriorityOrder(List<ProcessPriorityOrderDto> lstProcessPriorityOrder)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstProcessPriorityOrder)
                {
                    var processPriorityOrder = MapDtoToProcessPriorityOrder(item);
                    operation.AddUpdate("ProcessPriorityOrder", processPriorityOrder);
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SimulationLogicService::UpdateProcessPriorityOrder => Error updating ProcessPriorityOrder");
            }
        }

        public async Task UpdateOrderPriority(List<OrderPriorityDto> lstOrderPriority)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstOrderPriority)
                {
                    var orderPriority = MapDtoToOrderPriority(item);
                    operation.AddUpdate("OrderPriority", orderPriority);
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SimulationLogicService::UpdateOrderPriority => Error updating OrderPriority");
            }
        }

        public async Task UpdateSLAConfig(SLAConfigDto slaConfigDto)
        {
            try
            {
                OperationDB operation = new OperationDB();
                var slaConfig = MapDtoToSLAConfig(slaConfigDto);
                operation.AddUpdate("SLAConfigs", slaConfig);
                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SimulationLogicService::UpdateSLAConfig => Error updating SLAConfig");
            }
        }

        private ProcessPriorityOrder MapDtoToProcessPriorityOrder(ProcessPriorityOrderDto dto)
        {
            return new ProcessPriorityOrder
            {
                Id = dto.Id,
                Code = dto.Code,
                Priority = dto.Priority,
                WarehouseId = dto.WarehouseId,
                IsActive = dto.IsActive,
            };
        }

        private OrderPriority MapDtoToOrderPriority(OrderPriorityDto dto)
        {
            return new OrderPriority
            {
                Id = dto.Id,
                Code = dto.Code,
                Priority = dto.Priority,
                WarehouseId = dto.WarehouseId,
                IsActive = dto.IsActive,
            };
        }

        private SLAConfig MapDtoToSLAConfig(SLAConfigDto dto)
        {
            return new SLAConfig
            {
                Id = dto.Id,
                Code = dto.Code,
                Value = dto.Value,
                WarehouseId = dto.WarehouseId,
            };
        }

        private SLAConfigDto MapSLAConfig(SLAConfig sLAConfig)
        {
            return new SLAConfigDto
            {
                Id = sLAConfig.Id,
                Code = sLAConfig.Code,
                Value = sLAConfig.Value,
                WarehouseId = sLAConfig.WarehouseId,
            };
        }

        public double? GetPlanning (Guid planningId, Guid warehouseId)
        {
            return _dataAccess.GetPlanning(planningId, warehouseId);
        }

        public double? GetLastPlanning(Guid warehouseId)
        {
            return _dataAccess.GetLastPlanning(warehouseId);
        }
    }
}
