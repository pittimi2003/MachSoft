using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class WorkerService : IWorkerService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public WorkerService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddWorkerDto(WorkerDto WorkerDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                Worker shift = MapperWorkerFromDto(WorkerDto);

                if (shift == null)
                {
                    throw new InvalidOperationException("The Teams could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddNew("Workers", shift);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Teams added successfully: {AreaName}", shift.Name);

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Teams");
                throw;
            }
        }

        public async Task DeleteWorkerDto(List<WorkerDto> lstWorkerDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                foreach (WorkerDto workerDto in lstWorkerDto)
                {
                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddDelete("Workers", workerDto);
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("WorkerDto removing successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error removing the WorkerDto");
                throw;
            }
        }

        public IEnumerable<WorkerDto> GetAvailableWorkerDto()
        {
            List<WorkerDto> worker = new();

            try
            {
                foreach (var item in _dataAccess.GetAllWorkers())
                {
                    worker.Add(MapperaWorker(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RolService::GetRolDto => Error get list of roles in database");
            }

            return worker;
        }

        public WorkerDto GetWorkerDtoById(Guid WorkerDtoId)
        {
            return MapperaWorker(_dataAccess.GetWorkerById(WorkerDtoId));
        }

        public async Task UpdateWorkerDto(List<WorkerDto> lstWorkerDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                foreach (WorkerDto workerDto in lstWorkerDto)
                {
                    Worker worker = MapperWorkerFromDto(workerDto);

                    if (worker == null)
                    {
                        throw new InvalidOperationException("The Teams could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddUpdate("Workers", worker);
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Teams added successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Teams");
                throw;
            }
        }

        private WorkerDto MapperaWorker(Worker worker)
        {
            return new WorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                WorkerNumber = worker.WorkerNumber ?? 0,
                SelectionRolDto = MappearRolToDto(worker.Rol),
                SelectionTeamDto = MappearTeamToDto(worker.Team),
            };
        }

        public SelectionRolDto MappearRolToDto(Rol rol)
        {
            return new SelectionRolDto
            {
                Id = rol.Id,
                Name = rol.Name,
            };
        }

        public SelectionTeamDto MappearTeamToDto(Team team)
        {
            return new SelectionTeamDto
            {
                Id = team.Id,
                Name = team.Name,
            };
        }

        public Worker MapperWorkerFromDto(WorkerDto worker)
        {
            var team = _dataAccess.GetTeamAsNoTrackedById(worker.SelectionTeamDto.Id??new Guid());
            var rol = _dataAccess.GetRolAsNoTrackedById(worker.SelectionRolDto.Id ?? new Guid());

            return new Worker
            {
                Name = worker.Name,
                Rol = rol,
                Team = team,
                Id = worker.Id,
                RolId = rol.Id,
                TeamId = team.Id,
                WorkerNumber = worker.WorkerNumber,
            };


        }
    }
}
