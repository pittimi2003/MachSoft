using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class PlanningAlertsService : ICatalogueService<PlanningAlertsDto>
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<PlanningAlertsService> _logger;
        private readonly ISimulateService _simulateService;


        public PlanningAlertsService(DataAccess dataAccess, ILogger<PlanningAlertsService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }

        public async Task DeleteItems(List<PlanningAlertsDto> lstItems)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstItems)
                {
                    operation.AddDelete("Loadings", new EntityDto() { Id = item.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<PlanningAlertsDto> GetItems()
        {
            List<PlanningAlertsDto> resp = new();

            try
            {
                foreach (var item in _dataAccess.GetAllAlertResponses())
                {
                    resp.Add(MapperToModel(item));
                }

                foreach (var item in resp)
                {
                    if (_dataAccess.GetWorkOrderPlanningById(item.EntityId) is WorkOrderPlanning workOrderPlanning)
                    {
                        item.InputOrderId = workOrderPlanning.InputOrderId ?? Guid.Empty;
                        item.WorckOrderPlanningContains = true;
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadingsService::GetItems => Error get list of Loadings in database");
            }

            return resp;
        }

        public async Task AddItem(PlanningAlertsDto dto)
        {
            try
            {
                OperationDB operation = new OperationDB();
                var item = MapDtoToDto(dto);
                operation.AddNew("Loadings", item);
                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadingsService::AddItem => Error adding Loadings to database");
                throw;
            }
        }

        public async Task UpdateItems(List<PlanningAlertsDto> lstDto)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstDto)
                {
                    var itemUpdate = MapDtoToDto(item);
                    operation.AddUpdate("Loadings", itemUpdate);
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"LoadingsService::UpdateItems => Error updating Loading");
            }
        }

        private PlanningAlertsDto MapperToModel(AlertResponse item)
        {
            PlanningAlertsDto resp = new()
            {
                Id = item.Id,
                AlertId = item.AlertId,
                PlanningId = item.PlanningId,
                TriggerDate = item.TriggerDate,
                EntityId = item.EntityId,
                ItemPlanningContains = item.PlanningId != Guid.Empty,
                WorckOrderPlanningContains= false,
            };

            return resp;
        }

        private Alert MapDtoToDto(PlanningAlertsDto item)
        {
            return null;
            
        }
    }
}
