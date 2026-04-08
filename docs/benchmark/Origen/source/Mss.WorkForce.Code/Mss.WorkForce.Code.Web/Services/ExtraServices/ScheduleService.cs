using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class ScheduleService : IScheduleService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public ScheduleService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddScheduleDto(ScheduleDto ScheduleDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                Schedule rol = MapperScheduleFromDto(ScheduleDto);

                if (rol == null)
                {
                    throw new InvalidOperationException("The Schedules could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddNew("Schedules", rol);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Schedules added successfully: {AreaName}", rol.Name);

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Schedules");
                throw;
            }
        }

        public async Task DeleteScheduleDto(List<ScheduleDto> lstScheduleDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                foreach (ScheduleDto scheduleDto in lstScheduleDto)
                {
                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddDelete("Schedules", new EntityDto {Id = scheduleDto.Id });
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Schedules added successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Schedules");
                throw;
            }
        }

        public IEnumerable<ScheduleDto> GetAvailableScheduleDto()
        {
            List<ScheduleDto> schedule = new();

            try
            {
                foreach (var item in _dataAccess.GetAllSchedule(null))
                {
                    schedule.Add(MapperSchedule(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schedules::Schedules => Error get list of roles in database");
            }

            return schedule;
        }

        public ScheduleDto GetScheduleDtoById(Guid ScheduleDtoId)
        {
            return MapperSchedule(_dataAccess.GetScheduleById(ScheduleDtoId));
        }

        public async Task UpdateScheduleDto(List<ScheduleDto> lstScheduleDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                foreach (ScheduleDto scheduleDto in lstScheduleDto)
                {
                    Schedule rol = MapperScheduleFromDto(scheduleDto);

                    if (rol == null)
                    {
                        throw new InvalidOperationException("The Schedules could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddUpdate("Schedules", rol);
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Schedules added successfully");

            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Area");
                throw;
            }
        }

        private ScheduleDto MapperSchedule(Schedule schedule)
        {
            return new ScheduleDto
            {
                Id = schedule.Id,
                Name = schedule.Name,
                Date = schedule.Date,
                SelectionAvailableWorkerDto = MapAvailableWorkerToDto(schedule.AvailableWorker),
                SelectionBreakProfileDto = MapBreakProfileToDto(schedule.BreakProfile),
                SelectionShiftDto = MapShiftToDto(schedule.Shift),
            };
        }

        public SelectionAvailableWorkerDto MapAvailableWorkerToDto(AvailableWorker worker)
        {
            return new SelectionAvailableWorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
            };
        }

        public SelectionBreakProfileDto MapBreakProfileToDto(BreakProfile BreakProfile)
        {
            return new SelectionBreakProfileDto
            {
                Id = BreakProfile.Id,
                Name = BreakProfile.Name,
            };
        }

        public SelectionShiftDto MapShiftToDto(Shift Shift)
        {
            return new SelectionShiftDto
            {
                Id = Shift.Id,
                Name = Shift.Name,
            };
        }

        public Schedule MapperScheduleFromDto(ScheduleDto scheduleDto)
        {
            var avWorker = _dataAccess.GetAllAvailableWorkeAsNotracking(scheduleDto.SelectionAvailableWorkerDto.Id ?? new Guid());
            var avShift = _dataAccess.GetShiftAsNoTracking(scheduleDto.SelectionShiftDto.Id ?? new Guid());
            var avBreakProfile = _dataAccess.GetBreakProfileAsNoTracking(scheduleDto.SelectionBreakProfileDto.Id ?? new Guid());

            return new Schedule
            {
                Id = scheduleDto.Id,
                AvailableWorker = avWorker,
                BreakProfile = avBreakProfile,
                Name = scheduleDto.Name,
                Shift = avShift,
                AvailableWorkerId = avWorker.Id,
                BreakProfileId = avBreakProfile.Id,
                Date = scheduleDto.Date,
                ShiftId = avShift.Id,
            };
        }
    }
}
