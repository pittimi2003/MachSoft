using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class TeamService : ITeamService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public TeamService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddTeam(TeamDto Team)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                Team shift = MapperTeamFromDto(Team);

                if (shift == null)
                {
                    throw new InvalidOperationException("The Teams could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddNew("Teams", shift);

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

        public async Task DeleteTeam(List<TeamDto> lstTeam)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                foreach (TeamDto team in lstTeam)
                {
                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddDelete("Teams", new EntityDto { Id = team.Id});
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Teams deleted successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error deleting the Teams");
                throw;
            }
        }

        public IEnumerable<TeamDto> GetTeam()
        {
            List<TeamDto> team = new();

            try
            {
                foreach (var item in _dataAccess.GetAllTeams())
                {
                    team.Add(MapperTeam(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RolService::GetRolDto => Error get list of roles in database");
            }

            return team;
        }

        public TeamDto GetTeamById(Guid TeamId)
        {
            return MapperTeam(_dataAccess.GetTeamById(TeamId));
        }

        public async Task UpdateTeam(List<TeamDto> lstTeam)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                foreach (var item in lstTeam)
                {
                    Team team = MapperTeamFromDto(item);

                    if (team == null)
                    {
                        throw new InvalidOperationException("The Teams could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddUpdate("Teams", team);
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

        private TeamDto MapperTeam(Team rol)
        {
            return new TeamDto
            {
                Id = rol.Id,
                Name = rol.Name,
                SelectionWarehouseDto = MapWarehouseToDto(rol.Warehouse)
            };
        }

        public SelectionWarehouseDto MapWarehouseToDto(Warehouse warehouse)
        {
            return new SelectionWarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
            };
        }

        public Team MapperTeamFromDto(TeamDto team)
        {
            var warehouse = _dataAccess.GetWarehouseById(team.SelectionWarehouseDto.Id ?? new Guid());
            return new Team
            {
                Id = team.Id,
                Name = team.Name,
                Warehouse = warehouse,
                WarehouseId = warehouse.Id
            };
        }
    }
}
