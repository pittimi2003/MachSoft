using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;
using ShiftDto = Mss.WorkForce.Code.Models.DTO.ExtraConfiguration.ShiftDto;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class ShiftService : IShiftService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public ShiftService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddShift(ShiftDto shiftDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                Shift shift = MapperShiftFromDto(shiftDto);

                if (shift == null)
                {
                    throw new InvalidOperationException("The shift could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddNew("Shifts", shift);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("shift added successfully: {AreaName}", shift.Name);

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Area");
                throw;
            }
        }

        public async Task DeleteShiftDto(List<ShiftDto> lstShiftDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                foreach (var shiftDto in lstShiftDto)
                {
                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddDelete("Shifts", new EntityDto { Id = shiftDto.Id});
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("shift added successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Shift");
                throw;
            }
        }

        public ShiftDto GetShiftById(Guid shiftDtoId)
        {
            return MapperShift(_dataAccess.GetShiftById(shiftDtoId));
        }

        public IEnumerable<ShiftDto> GetShiftDto()
        {
            List<ShiftDto> shifts = new();

            try
            {
                foreach (var item in _dataAccess.GetAllShifts())
                {
                    shifts.Add(MapperShift(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ShiftService::GetShiftDto => Error get list of Shifts in database");
            }

            return shifts;
        }

        public async Task UpdateShift(List<ShiftDto> lstShiftDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                foreach (var shiftDto in lstShiftDto)
                {
                    Shift shift = MapperShiftFromDto(shiftDto);

                    if (shift == null)
                    {
                        throw new InvalidOperationException("The shift could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddUpdate("Shifts", shift);
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("shift added successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Area");
                throw;
            }
        }

        private ShiftDto MapperShift(Shift rol)
        {
            return new ShiftDto
            {
                Id = rol.Id,
                Name = rol.Name,
                InitHour = rol.InitHour,
                EndHour = rol.EndHour,
                SelectionWarehouseDto = MapLayoutToDto(rol.Warehouse)
            };
        }

        public SelectionWarehouseDto MapLayoutToDto(Warehouse warehouse)
        {
            return new SelectionWarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
            };
        }

        public Shift MapperShiftFromDto(ShiftDto shiftDto) 
        {
            var warehouse = _dataAccess.GetWarehouseById(shiftDto.SelectionWarehouseDto.Id ?? new Guid());
            return new Shift { 
                Name = shiftDto.Name, 
                Id = shiftDto.Id,
                EndHour = shiftDto.EndHour,
                InitHour = shiftDto.InitHour,
                Warehouse = warehouse,
                WarehouseId = warehouse.Id
            };
        }

    }
}
