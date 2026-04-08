using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services
{
    public class RolProcessSequenceService : ICatalogueService<RolProcessSequencesDto>
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<RolProcessSequenceService> _logger;
        private readonly ISimulateService _simulateService;


        public RolProcessSequenceService(DataAccess dataAccess, ILogger<RolProcessSequenceService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }

        public async Task DeleteItems(List<RolProcessSequencesDto> lstItems)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstItems)
                {
                    operation.AddDelete("RolProcessSequences", new EntityDto() { Id = item.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RolProcessSequencesDto> GetItems()
        {
            List<RolProcessSequencesDto> resp = new();

            try
            {
                foreach (var item in _dataAccess.GetRolProcessSequences())
                {
                    resp.Add(MapperToModel(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RolProcessSequenceService::GetItems => Error get list of RolProcessSequence in database");
            }

            return resp;
        }

        public async Task AddItem(RolProcessSequencesDto dto)
        {
            try
            {
                OperationDB operation = new OperationDB();
                var item = MapDtoToDto(dto);
                operation.AddNew("RolProcessSequences", item);
                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RolProcessSequenceService::AddItem => Error adding RolProcessSequence to database");
                throw;
            }
        }

        public async Task UpdateItems(List<RolProcessSequencesDto> lstDto)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstDto)
                {
                    var itemUpdate = MapDtoToDto(item);
                    operation.AddUpdate("RolProcessSequences", itemUpdate);
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RolProcessSequenceService::UpdateItems => Error updating RolProcessSequence");
            }
        }

        private RolProcessSequencesDto MapperToModel(RolProcessSequence item)
        {
            RolProcessSequencesDto resp = new()
            {
                Id = item.Id,
                Name = item.Name,
                Rol = new SelectionRolDto()
                {
                    Id = item.Rol.Id,
                    Name = item.Rol.Name,
                },
                Process = new ProcessCatalogueDto()
                {
                    Id = item.Process.Id,
                    Name = item.Process.Name
                },
                Sequence = item.Sequence,
            };

            return resp;
        }

        private RolProcessSequence MapDtoToDto(RolProcessSequencesDto item)
        {
            return new RolProcessSequence
            {
                Id = item.Id,
                Name = item.Name,
                RolId = (Guid)item.Rol.Id,
                Rol = _dataAccess.GetAllRoles().FirstOrDefault(x => x.Id == (Guid)item.Rol.Id),
                ProcessId = (Guid)item.Process.Id,
                Process = _dataAccess.GetAllProcess().FirstOrDefault(x => x.Id == (Guid)item.Process.Id),
                Sequence = (int)item.Sequence,
            };
        }
    }
}
