using DevExpress.Data.Utils;
using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services
{
    public class ResourceService : IResourceService
    {

        #region Fields

        private readonly DataAccess _dataAccess;
        private readonly ISimulateService _simulateService;

        #endregion

        #region Constructors

        public ResourceService(DataAccess dataAccess, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _simulateService = simulateService;
        }

        #endregion

        #region Properties

        [Inject]
        private ILogger<ResourceService> _logger { get; set; }

        #endregion

        #region Methods

        public async Task<bool> BreakProfileSaveChanges(IEnumerable<BreakProfilesDto> breakProfiles, Guid warehouseId)
        {
            OperationDB operation = new OperationDB();

            try
            {
                foreach (var breakProfileDto in breakProfiles)
                {
                    if (breakProfileDto.DataOperationType == OperationType.Insert)
                    {
                        breakProfileDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.BreakProfile, MapperBreakProfiles(breakProfileDto));
                    }
                    else if (breakProfileDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.BreakProfile, MapperBreakProfiles(breakProfileDto));
                    else if (breakProfileDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.BreakProfile, new EntityDto() { Id = breakProfileDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResourceService::WorkersSaveChanges => Error to save changes");
                return false;
            }
        }

        public async Task BreaksSaveChanges(IEnumerable<BreakDto> breaks, Guid warehouseId)
        {
            OperationDB operation = new OperationDB();

            try
            {
                foreach (var breakDto in breaks)
                {
                    if (breakDto.DataOperationType == OperationType.Insert)
                    {
                        operation.AddNew(EntityNamesConst.Break, MapperBreaks(breakDto));
                    }
                    else if (breakDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.Break, MapperBreaks(breakDto));
                    else if (breakDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.Break, new EntityDto() { Id = breakDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResourceService::WorkersSaveChanges => Error to save changes");
            }
        }

        public async Task EquipmentTypeSaveChanges(IEnumerable<TypeEquipmentDto> equipments, Guid warehouseId)
        {
            OperationDB operation = new OperationDB();

            try
            {
                foreach (var equipmentDto in equipments)
                {
                    if (equipmentDto.DataOperationType == OperationType.Insert)
                    {
                        equipmentDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.TypeEquipment, MapperTypeEquipment(equipmentDto));
                    }
                    else if (equipmentDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.TypeEquipment, MapperTypeEquipment(equipmentDto));
                    else if (equipmentDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.TypeEquipment, new EntityDto() { Id = equipmentDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResourceService::EquipmentTypeSaveChanges => Error to save changes");
            }
        }

        public IEnumerable<BreakProfilesDto> GetBreakProfiles(Guid warehouseId)
        {
            List<BreakProfilesDto> breakProfiles = new();

            foreach (var element in _dataAccess.GetBreakProfiles(warehouseId))
                breakProfiles.Add(MapperBreakProfiles(element));

            return breakProfiles;
        }

        public IEnumerable<BreakDto> GetBreaks(Guid warehouseId)
        {
            List<BreakDto> breaks = new();
            var workers = GetWorkers(warehouseId).ToList();
            foreach (var _break in _dataAccess.GetAllBreaks(warehouseId))
                breaks.Add(MapperBreaks(_break, workers));

            return breaks;
        }

        public IEnumerable<ResourceRolProcessDto> GetRolProcesses(Guid warehouseId)
        {            
            var rolesSequence = new List<ResourceRolProcessDto>();
            var workers = GetWorkers(warehouseId).ToList();
            var groupedRoles = _dataAccess.GetRolProcess().GroupBy(p => p.RolId);
            foreach (var group in groupedRoles)
            {
                var role = _dataAccess.GetRoles(warehouseId).FirstOrDefault(x => x.Id == group.Key);
                if (role != null)
                {
                    var dto = new ResourceRolProcessDto
                    {
                        Id = Guid.NewGuid(),
                        RolId = role.Id,
                        RolName = role.Name,
                        Name = role.Name,
                        Process = new Multiselect
                        {
                            Name = string.Join(",", group.OrderBy(x => x.Sequence).Select(p => p.Process.Name)),
                        }
                    };
                    dto.Process.Items = group.OrderBy(x => x.Sequence).ToDictionary( x => x.ProcessId, x => x.Process.Name);
                    dto.IsDependencies = workers.Any(w => w.Rol != null && w.Rol.Id == role.Id);
                    rolesSequence.Add(dto);
                }
            }
            return rolesSequence;
        }

        public IEnumerable<ShiftsDto> GetShifts(Guid warehouseId)
        {
            List<ShiftsDto> shifts = new();
            var workers = GetWorkers(warehouseId).ToList();
            foreach (var shift in _dataAccess.GetShifts(warehouseId))
                shifts.Add(MapperShifts(shift, workers));

            return shifts;
        }

        public IEnumerable<TeamsDto> GetTeams(Guid warehouseId)
        {
            List<TeamsDto> teams = new();
            var workers = GetWorkers(warehouseId).ToList();
            foreach (var team in _dataAccess.GetTeams(warehouseId))
                teams.Add(MapperTeam(team, workers));

            return teams;
        }

        public IEnumerable<TypeEquipmentDto> GetTypeEquipments(Guid warehouseId)
        {
            List<TypeEquipmentDto> equipments = new();
            var groupEquipments = GetEquipmentGroups(warehouseId).ToList();
            foreach (var equipmentData in _dataAccess.GetEquipmentTypes(warehouseId))
                equipments.Add(MapperTypeEquipment(equipmentData, groupEquipments));

            return equipments;
        }

        public IEnumerable<ResourceWorkerScheduleDto> GetWorkers(Guid warehouseId)
        {
            List<ResourceWorkerScheduleDto> workers = new();

            List<BreakProfile> breaks = _dataAccess.GetAllBreakProfiles(warehouseId).ToList();

            foreach (var worker in _dataAccess.GetOperators(warehouseId).ToList())
                workers.Add(MapperWorker(worker, breaks));

            return workers;
        }

        public async Task ShiftSaveChanges(IEnumerable<ShiftsDto> shifts, Guid warehouseId)
        {
            OperationDB operation = new();
            try
            {
                foreach (var shiftDto in shifts)
                {
                    if (shiftDto.DataOperationType == OperationType.Insert)
                    {
                        shiftDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.Shift, MapperShift(shiftDto));
                    }
                    else if (shiftDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.Shift, MapperShift(shiftDto));
                    else if (shiftDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.Shift, new EntityDto() { Id = shiftDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResourceService::ShiftSaveChanges => Error to save changes");
            }
        }

        public async Task TeamsSaveChanges(IEnumerable<TeamsDto> teams, Guid warehouseId)
        {
            OperationDB operation = new();
            try
            {
                foreach (var teamDto in teams)
                {
                    if (teamDto.DataOperationType == OperationType.Insert)
                    {
                        teamDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.Team, MapperTeam(teamDto));
                    }
                    else if (teamDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.Team, MapperTeam(teamDto));
                    else if (teamDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.Team, new EntityDto() { Id = teamDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResourceService::TeamsSaveChanges => Error to save changes");
            }
        }

        public async Task WorkersSaveChanges(IEnumerable<ResourceWorkerScheduleDto> workers, Guid warehouseId)
        {
            try
            {
                foreach (var workerDto in workers)
                {
                    if (workerDto.DataOperationType == OperationType.Insert)
                    {
                        workerDto.WorkerId = Guid.NewGuid();
                        _dataAccess.WorkerProfileInsert(workerDto);

                    }
                    else if (workerDto.DataOperationType == OperationType.Update)
                    {
                        _dataAccess.WorkerProfileUpdate(workerDto);
                    }

                    else if (workerDto.DataOperationType == OperationType.Delete)
                    {
                        _dataAccess.WorkerProfileDelete(workerDto);
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResourceService::WorkersSaveChanges => Error to save changes");
            }
        }

        public async Task RolesProcessSaveChanges(IEnumerable<ResourceRolProcessDto> resourceRolProces, Guid warehouseId)
        {
            try
            {
                foreach (var resourceRolProcessDto in resourceRolProces)
                {
                    if (resourceRolProcessDto.DataOperationType == OperationType.Insert)
                    {
                        Rol rol = MapperRol(resourceRolProcessDto, warehouseId, true);
                        List<RolProcessSequence> rolProcessSequences = MapperRolProcess(resourceRolProcessDto.Process.Items, resourceRolProcessDto, true);
                        _dataAccess.AddRolProcessSequenceSaveChanges(rol, rolProcessSequences);
                    }
                    else if (resourceRolProcessDto.DataOperationType == OperationType.Update)
                    {
                        Rol rol = MapperRol(resourceRolProcessDto, warehouseId, false);
                        List<RolProcessSequence> rolProcessSequences = MapperRolProcess(resourceRolProcessDto.Process.Items, resourceRolProcessDto, false);
                        _dataAccess.UpdateRolProcessSequenceSaveChanges(rol, rolProcessSequences);
                    }
                    else if (resourceRolProcessDto.DataOperationType == OperationType.Delete)
                    {
                        Rol rol = MapperRol(resourceRolProcessDto, warehouseId, false);
                        _dataAccess.DeleteRolProcessSequenceSaveChanges(rol);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RolesProcessSaveChanges::WorkersSaveChanges => Error to save changes");
            }
        }

        private BreakProfilesDto MapperBreakProfiles(BreakProfile entity)
        {
            BreakProfilesDto entityDto = new();

            entityDto.Id = entity.Id;
            entityDto.Name = entity.Name;
            entityDto.Type = entity.Type;
            //entityDto.AllowInInboundFlow = entity.AllowInInboundFlow;
            //entityDto.AllowInOutboundFlow = entity.AllowInOutboundFlow;
            entityDto.WarehouseId = entity.WarehouseId;

            return entityDto;
        }

        private BreakProfile MapperBreakProfiles(BreakProfilesDto entityDto)
        {
            BreakProfile entity = new()
            {
                Id = entityDto.Id,
                Name = entityDto.Name,
                Type = entityDto.Type,
                //AllowInInboundFlow = entityDto.AllowInInboundFlow ?? false,
                //AllowInOutboundFlow = entityDto.AllowInOutboundFlow ?? false,
                WarehouseId = entityDto.WarehouseId,
                Warehouse = null
            };

            return entity;
        }

        private BreakDto MapperBreaks(Break entity, IEnumerable<ResourceWorkerScheduleDto> workers)
        {
            BreakDto entityDto = new();

            entityDto.Id = entity.Id;
            entityDto.Name = entity.Name;
            entityDto.InitBreak = TimeSpan.FromHours(entity.InitBreak);
            entityDto.EndBreak = TimeSpan.FromHours(entity.EndBreak);
            entityDto.IsPaid = entity.IsPaid ?? false;
            entityDto.IsRequiered = entity.IsRequiered ?? false;
            entityDto.BreakProfileId = entity.BreakProfileId;

            if (entity.BreakProfile != null)
            {
                entityDto.BreakProfile.Id = entity.BreakProfile.Id;
                entityDto.BreakProfile.Name = entity.BreakProfile.Name;
            }
            entityDto.IsDependencies = workers.Any(w => w.BreakProfileId == entity.BreakProfileId);
            return entityDto;
        }

        private Break MapperBreaks(BreakDto entityDto)
        {
            Break entity = new()
            {
                Id = entityDto.Id,
                Name = entityDto.Name,
                InitBreak = entityDto.InitBreak.TotalHours,
                EndBreak = entityDto.EndBreak.TotalHours,
                IsPaid = entityDto.IsPaid,
                IsRequiered = entityDto.IsRequiered,
                BreakProfileId = entityDto.BreakProfile.Id,
                BreakProfile = null,
            };

            return entity;
        }

        private List<RolProcessSequence> MapperRolProcess(Dictionary<Guid, string> values, ResourceRolProcessDto resourceRolProcessDto, bool isNew)
        {
            List<RolProcessSequence> rolProcessSequences = new List<RolProcessSequence>();
            int sequence = 1;
            foreach (var item in values)
            {
                rolProcessSequences.Add(new RolProcessSequence
                {
                    Id = Guid.NewGuid(),
                    Name = resourceRolProcessDto.Name,
                    RolId = isNew ? resourceRolProcessDto.Id : resourceRolProcessDto.RolId,
                    Rol = null,
                    ProcessId = item.Key,
                    Process = null,
                    Sequence = sequence
                });

                sequence++;
            }
            return rolProcessSequences;
        }

        private Rol MapperRol(ResourceRolProcessDto resourceRolProcessDto, Guid warehouseId, bool isNew)
        {
            Rol entity = new()
            {
                Id = isNew ? resourceRolProcessDto.Id : resourceRolProcessDto.RolId,
                Name = resourceRolProcessDto.Name,
                WarehouseId = warehouseId,
                Warehouse = null,
            };

            return entity;
        }

        private Shift MapperShift(ShiftsDto entityDto)
        {
            Shift entity = new()
            {
                Id = entityDto.Id,
                Name = entityDto.Name,
                InitHour = entityDto.InitHour.TotalHours,
                EndHour = entityDto.EndHour.TotalHours,
                WarehouseId = entityDto.WarehouseId,
                Warehouse = null
            };

            return entity;
        }

        private ShiftsDto MapperShifts(Shift entity, IEnumerable<ResourceWorkerScheduleDto> workers)
        {
            ShiftsDto entityDto = new();

            entityDto.Id = entity.Id;
            entityDto.Name = entity.Name;
            entityDto.WarehouseId = entity.WarehouseId;
            entityDto.InitHour = TimeSpan.FromHours(entity.InitHour);
            entityDto.EndHour = TimeSpan.FromHours(entity.EndHour);
            entityDto.IsDependencies = workers.Any(w => w.Team != null && w.Shift.Id == entity.Id);
            return entityDto;
        }

        private TeamsDto MapperTeam(Team entity, IEnumerable<ResourceWorkerScheduleDto> workers)
        {
            TeamsDto entityDto = new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                WarehouseId = entity.WarehouseId,
                IsDependencies = workers.Any(w => w.Team != null && w.Team.Id == entity.Id)
            };
            return entityDto;
        }

        private Team MapperTeam(TeamsDto entityDto)
        {
            Team entity = new()
            {
                Id = entityDto.Id,
                Name = entityDto.Name,
                Description = entityDto.Description,
                WarehouseId = entityDto.WarehouseId,
                Warehouse = null,

            };

            return entity;
        }

        private TypeEquipment MapperTypeEquipment(TypeEquipmentDto entityDto)
        {

            TypeEquipment entity = new()
            {
                Id = entityDto.Id,
                Name = entityDto.Name,
                Description = entityDto.Description,
                Capacity = entityDto.Capacity,
                Quantity = entityDto.Quantity,
                LoadingWaitTime = entityDto.LoadingWaitTime,
                WarehouseId = entityDto.WarehouseId,
                Warehouse = null,
            };

            return entity;
        }

        private TypeEquipmentDto MapperTypeEquipment(TypeEquipment entity, IEnumerable<EquipmentGroupsDto> groupEquipments)
        {
            TypeEquipmentDto entityDto = new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Capacity = entity.Capacity,
                Quantity = entity.Quantity,
                LoadingWaitTime = entity.LoadingWaitTime ?? 0,
                WarehouseId = entity.WarehouseId,
                IsDependencies = groupEquipments.Any(g => g.TypeEquipmentCatalogueNoFilteredDto.Id == entity.Id)
            };
            return entityDto;
        }

        private ResourceWorkerScheduleDto MapperWorker(Schedule entity, List<BreakProfile> breaks)
        {
            ResourceWorkerScheduleDto entityDto = new();

            entityDto.Id = entity.Id;

            if (entity.AvailableWorker != null && entity.AvailableWorker.Worker != null)
            {
                entityDto.Available = entity.Available??false;

                entityDto.WorkerId = entity.AvailableWorker.Worker.Id;
                entityDto.Name = entity.AvailableWorker.Worker.Name;
                entityDto.WorkerNumber = entity.AvailableWorker.Worker.WorkerNumber ?? 0;

                if (entity.AvailableWorker.Worker.Rol != null)
                {
                    entityDto.Rol.Id = entity.AvailableWorker.Worker.Rol.Id;
                    entityDto.Rol.Name = entity.AvailableWorker.Worker.Rol.Name;
                }

                if (entity.AvailableWorker.Worker.Team != null)
                {
                    entityDto.Team.Id = entity.AvailableWorker.Worker.Team.Id;
                    entityDto.Team.Name = entity.AvailableWorker.Worker.Team.Name;
                }

            }

            if (entity.Shift != null)
            {
                entityDto.Shift.Id = entity.Shift.Id;
                entityDto.Shift.Name = entity.Shift.Name;

                //entityDto.InitHour = DateTime.Today.AddHours(entity.Shift.InitHour);
                //entityDto.EndHour = DateTime.Today.AddHours(entity.Shift.EndHour);
            }

            if (entity.BreakProfile != null)
            {
                var entityBreak = breaks.FirstOrDefault(x => x.Id == entity.BreakProfile.Id);

                entityDto.BreakProfileId = entity.BreakProfileId;

                if (entityBreak != null)
                {
                    entityDto.Break.Id = entityBreak.Id;
                    entityDto.Break.Name = entityBreak.Name;
                }
            }

            return entityDto;
        }

        private Worker MapperWorker(ResourceWorkerScheduleDto entityDto, Guid warehouseId)
        {
            Worker entity = new()
            {
                Id = entityDto.WorkerId,
                Name = entityDto.Name,
                RolId = entityDto.Rol.Id,
                TeamId = entityDto.Team.Id,
                Rol = null,
                Team = null
            };

            return entity;
        }

        public IEnumerable<EquipmentGroupsDto> GetEquipmentGroups(Guid warehouseId)
        {
            List<EquipmentGroupsDto> equipmentGroups = new();

            foreach (var groupData in _dataAccess.GetEquipmentGroupsWithType(warehouseId))
                equipmentGroups.Add(MapperEquipmentGroups(groupData));

            return equipmentGroups;
        }

        public async Task EquipmentGroupTypeSaveChanges(IEnumerable<EquipmentGroupsDto> equipmentGroups, Guid warehouseId)
        {
            OperationDB operation = new();
            try
            {
                foreach (var equipmentGroupDto in equipmentGroups)
                {
                    if (equipmentGroupDto.DataOperationType == OperationType.Insert)
                    {
                        operation.AddNew(EntityNamesConst.EquipmentGroup, MapperEquipmentGroups(equipmentGroupDto, warehouseId));
                    }
                    else if (equipmentGroupDto.DataOperationType == OperationType.Update)
                    {
                        operation.AddUpdate(EntityNamesConst.EquipmentGroup, MapperEquipmentGroups(equipmentGroupDto, warehouseId));
                    }
                    else if (equipmentGroupDto.DataOperationType == OperationType.Delete)
                    {
                        operation.AddDelete(EntityNamesConst.EquipmentGroup, new EntityDto() { Id = equipmentGroupDto.Id });
                    }
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResourceService::EquipmentGroupTypeSaveChanges => Error to save changes");
            }
        }

        private EquipmentGroupsDto MapperEquipmentGroups(EquipmentGroup entity)
        {
            return new EquipmentGroupsDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Equipments = entity.Equipments,
                TypeEquipmentCatalogueNoFilteredDto = new CatalogEntity(EntityNamesConst.TypeEquipment)
                {
                    Id = entity.TypeEquipment?.Id ?? Guid.Empty,
                    Name = entity.TypeEquipment?.Name ?? string.Empty
                },
                Area = new CatalogEntity(EntityNamesConst.Area)
                {
                    Id = entity.Area?.Id ?? Guid.Empty,
                    Name = entity.Area?.Name ?? string.Empty
                }
            };
        }
        private EquipmentGroup MapperEquipmentGroups(EquipmentGroupsDto dto, Guid warehouseId)
        {
            EquipmentGroup entity = new()
            {
                Id = dto.Id,
                Name = dto.Name,
                Equipments = dto.Equipments,
                TypeEquipmentId = dto.TypeEquipmentCatalogueNoFilteredDto?.Id ?? Guid.Empty,
                AreaId = dto.Area?.Id ?? Guid.Empty,
                TypeEquipment = null,
                Area = null,
                ViewPort = dto.ViewPort,
            };

            return entity;
        }



        #endregion

    }
}
