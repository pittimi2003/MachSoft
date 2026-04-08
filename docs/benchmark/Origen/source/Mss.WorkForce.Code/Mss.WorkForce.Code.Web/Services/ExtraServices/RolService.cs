using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;
using Rol = Mss.WorkForce.Code.Models.Models.Rol;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class RolService : IRolService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public RolService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddRol(RolDto rolDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                Rol rol = MapperRolFromDto(rolDto);

                if (rol == null)
                {
                    throw new InvalidOperationException("The role could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddNew("Roles", rol);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Role added successfully: {AreaName}", rol.Name);

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Area");
                throw;
            }
        }

        public async Task DeleteRolDto(List<RolDto> lstRolDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                foreach (var rol in lstRolDto)
                {

                    if (rol == null)
                    {
                        throw new InvalidOperationException("The role could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddDelete("Roles", new EntityDto() { Id = rol.Id });
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Role added successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Role");
                throw;
            }
        }

        public IEnumerable<RolDto> GetRol()
        {
            List<RolDto> rol = new();

            try
            {
                foreach (var item in _dataAccess.GetAllRoles())
                {
                    rol.Add(MapperRol(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RolService::GetRolDto => Error get list of roles in database");
            }

            return rol;
        }

        public RolDto GetRolById(Guid rolDtoId)
        {
            return MapperRol(_dataAccess.GetRolById(rolDtoId));
        }


        public async Task UpdateRol(List<RolDto> lstRolDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                foreach (var item in lstRolDto)
                {
                    Rol rol = MapperRolFromDto(item);

                    if (rol == null)
                    {
                        throw new InvalidOperationException("The role could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddUpdate("Roles", rol);
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Role updated successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Area");
                throw;
            }
        }

        private RolDto MapperRol(Rol rol)
        {
            return new RolDto
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

        public Rol MapperRolFromDto(RolDto dto)
        {
            var warehouse = _dataAccess.GetWarehouseById(dto.SelectionWarehouseDto.Id ?? new Guid());
            return new Rol
            {
                Name = dto.Name,
                Id = dto.Id,
                Warehouse = warehouse,
                WarehouseId = warehouse.Id
            };
        }
    }
}
