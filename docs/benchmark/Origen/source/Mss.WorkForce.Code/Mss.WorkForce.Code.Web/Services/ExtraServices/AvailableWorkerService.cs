using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class AvailableWorkerService : IAvailableWorkerService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public AvailableWorkerService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddAvailableWorker(AvailableWorkerDto AvailableWorker)
        {
            try
            {
                operationDB = new OperationDB();

                AvailableWorker bp = MapperAvailableWorkersFromDto(AvailableWorker);
                operationDB.AddNew("AvailableWorkers", bp);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("AvailableWorker added successfully: {AvailableWorker}", AvailableWorker.Name);

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the AvailableWorker");
                throw;
            }
        }

        public async Task DeleteAvailableWorker(List<AvailableWorkerDto> lstAvailableWorker)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (AvailableWorkerDto availableWorker in lstAvailableWorker)
                {
                    operationDB.AddDelete("AvailableWorker", new EntityDto {Id = availableWorker.Id });
                }

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("AvailableWorker removed successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error removing the AvailableWorker");
                throw;
            }
        }

        public IEnumerable<AvailableWorkerDto> GetAvailableWorker()
        {
            List<AvailableWorkerDto> availableWorkers = new();

            try
            {
                foreach (var item in _dataAccess.GetAllAvailableWorkers())
                {
                    availableWorkers.Add(MapperAvailableWorkers(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AreaService::GetAreaDto => Error get list of areas in database");
            }

            return availableWorkers;
        }

        public AvailableWorkerDto GetAvailableWorkerById(Guid AvailableWorkerId)
        {
            return MapperAvailableWorkers(_dataAccess.GetAvailableWorkerById(AvailableWorkerId));
        }

        public async Task UpdateAvailableWorker(List<AvailableWorkerDto> lstAvailableWorker)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (AvailableWorkerDto availableWorker in lstAvailableWorker)
                {

                    AvailableWorker bp = MapperAvailableWorkersFromDto(availableWorker);
                    operationDB.AddUpdate("AvailableWorkers", bp);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("AvailableWorker updated successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the AvailableWorker");
                throw;
            }
        }

        public AvailableWorkerDto MapperAvailableWorkers(AvailableWorker avW)
        {
            return new AvailableWorkerDto
            {
                Name = avW.Name,
                Id = avW.Id,
                SelectionWorkerDto = MapperSelectionWorker(avW.Worker),
            };
        }

        public SelectionWorkerDto MapperSelectionWorker(Worker worker)
        {
            return new SelectionWorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
            };
        }

        public AvailableWorker MapperAvailableWorkersFromDto(AvailableWorkerDto availableWorkerDto)
        {
            var worker = _dataAccess.GetWorkerAsNoTrackingById(availableWorkerDto.Id);

            return new AvailableWorker
            {
                Name = availableWorkerDto.Name,
                Id= availableWorkerDto.Id,
                Worker = worker,
                WorkerId = worker.Id,
            };
        }
    }
}
