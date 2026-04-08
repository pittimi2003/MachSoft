using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;
using AreaDto = Mss.WorkForce.Code.Models.DTO.ExtraConfiguration.AreaDto;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices
{
    public class AreaService : IAreaService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<WarehouseService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;

        public AreaService(DataAccess dataAccess, ILogger<WarehouseService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }
        public async Task AddArea(AreaDto areaDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                // Mapear el DTO al modelo de datos
                Area area = MapperAreaFromDto(areaDto);

                if (area == null)
                {
                    throw new InvalidOperationException("The area could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddNew("Areas", area);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Area added successfully: {AreaName}", area.Name);
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the Area. DTO: {@ItemDto}", areaDto);
                throw;
            }
        }

        public async Task DeleteAreaDto(List<AreaDto> lstProcessDto)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (AreaDto areaDto in lstProcessDto)
                {
                    operationDB.AddDelete("Areas", new EntityDto() { Id = areaDto.Id });

                    await _simulateService.SaveChangesInDataBase(operationDB);

                    _logger.LogInformation("Area removed successfully: {AreaName}", areaDto.Name);
                }
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error removing the Areas");
                throw;
            }
        }

        public AreaDto GetAreaById(Guid areaDtoId)
        {
            return MapperArea(_dataAccess.GetAreaById(areaDtoId));
        }

        public IEnumerable<AreaDto> GetAreaDto()
        {
            List<AreaDto> areas = new();

            try
            {
                foreach (var item in _dataAccess.GetAllAreas())
                {
                    areas.Add(MapperArea(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AreaService::GetAreaDto => Error get list of areas in database");
            }

            return areas;
        }

        public async Task UpdateArea(List<AreaDto> lstProcessDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                foreach(var item in lstProcessDto)
                {
                    Area area = MapperAreaFromDto(item);

                    if (area == null)
                    {
                        throw new InvalidOperationException("The area could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddUpdate("Areas", area);

                    // Guardar los cambios en la base de datos
                    await _simulateService.SaveChangesInDataBase(operationDB);

                    _logger.LogInformation("Area added successfully: {AreaName}", area.Name);
                }
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error updating the Area");
                throw;
            }
        }

        public AreaDto MapperArea(Area area)
        {
            return new AreaDto
            {
                Id = area.Id,
                Name = area.Name,
                DelayedTimePerUnit = area.DelayedTimePerUnit ?? 0,
                IsAutomatic = area.IsAutomatic,
                NarrowAisle = area.NarrowAisle,
                Type = area.Type,
                xEnd = area.xEnd ?? 0,
                yEnd = area.yEnd ?? 0,
                xInit = area.xInit ?? 0,
                yInit = area.yInit ?? 0,
                ViewPort = area.ViewPort,
                SelectionAreaDto = area.AlternativeArea != null ? new SelectionAreaDto { Id = area.AlternativeArea.Id, Name = area.AlternativeArea.Name } : null,
                SelectionLayoutDto = new SelectionLayoutDto { Id = area.Layout.Id, Name = area.Layout.Name }
            };
        }

        public SelectionLayoutDto MapLayoutToDto(Layout layout)
        {
            return new SelectionLayoutDto
            {
                Id = layout.Id,
                Name = layout.Name,
            };
        }

        public Area MapperAreaFromDto(AreaDto areaDto)
        {
            try
            {
                var layout = _dataAccess.GetLayoutById(areaDto.SelectionLayoutDto.Id ?? Guid.Empty);
                var alternativeArea = areaDto.SelectionAreaDto is null ? null: _dataAccess.GetAreaNoTackById(areaDto.SelectionAreaDto.Id?? Guid.Empty);

                return new Area { 
                    Layout = layout,
                    Name = areaDto.Name,
                    Type = areaDto.Type,
                    AlternativeArea = alternativeArea,
                    DelayedTimePerUnit = areaDto.DelayedTimePerUnit,
                    IsAutomatic = areaDto.IsAutomatic,
                    Id = areaDto.Id,
                    NarrowAisle = areaDto.NarrowAisle,
                    xEnd = areaDto.xEnd,
                    yEnd = areaDto.yEnd,
                    xInit = areaDto.xInit,
                    yInit = areaDto.yInit,
                    LayoutId = layout.Id,
                    AlternativeAreaId = alternativeArea?.Id,
                    ViewPort = areaDto.ViewPort,
                };
            }
            catch (Exception ex) 
            {
                throw new Exception();
            }
        }
    }
}
