using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.DTO.Designer.LocationZones;
using Mss.WorkForce.Code.Models.DTO.Designer.Process;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.Resources;
using Polly;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static DevExpress.Utils.HashCodeHelper;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;
using AreaDto = Mss.WorkForce.Code.Models.DTO.AreaDto;
using Buffer = Mss.WorkForce.Code.Models.Models.Buffer;
using CustomFlowGraphDto = Mss.WorkForce.Code.Models.DTO.Designer.CustomFlowGraphDto;
using InboundFlowGraphDto = Mss.WorkForce.Code.Models.DTO.Designer.InboundFlowGraphDto;
using OutboundFlowGraphDto = Mss.WorkForce.Code.Models.DTO.Designer.OutboundFlowGraphDto;
using Packing = Mss.WorkForce.Code.Models.Models.Packing;
using Route = Mss.WorkForce.Code.Models.Models.Route;
using ZoneDto = Mss.WorkForce.Code.Models.DTO.Designer.ZoneDto;


namespace Mss.WorkForce.Code.Web.Services
{
    public class DesignerServices : IDesignerServices
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<UserService> _logger;
        private readonly ISimulateService _simulateService;

        private OperationDB operationDB;
        private LayoutDto actualLayoutName;
        private readonly Dictionary<string, int> _zoneCounters = new();
        private readonly IInitialDataService _InitialDataService;

        public DesignerServices(DataAccess dataAccess, ILogger<UserService> logger, ISimulateService simulateService, IInitialDataService initialDataService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
            _InitialDataService = initialDataService;
        }

        #region Layout Implementation
        public async Task AddLayout(LayoutDto itemDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();


                var userFormat = _InitialDataService.GetUserFormat();
                var unitSystem = userFormat.UnitSystem;

                itemDto.Height = UnitConverter.ConvertFeetToMeters(itemDto.HeightConvert, unitSystem);
                itemDto.Width = UnitConverter.ConvertFeetToMeters(itemDto.WidthConvert, unitSystem);

                // Mapear el DTO al modelo de datos
                var layout = MapperLayout(itemDto);

                if (layout == null)
                {
                    throw new InvalidOperationException("The layout could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddNew("Layouts", layout);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Layout added successfully: {LayoutName}", layout.Name);
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the Layout. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }
        public async Task UpdateLayout(LayoutDto itemDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                // Mapear el DTO al modelo de datos
                var layout = MapperLayout(itemDto);

                if (layout == null)
                {
                    throw new InvalidOperationException("The layout could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddUpdate("Layouts", layout);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Layout added successfully: {LayoutName}", layout.Name);
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the Layout. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }
        public async Task UpdateListLayout(List<LayoutDto> itemListDto)
        {
            try
            {
                operationDB = new OperationDB();
                actualLayoutName = new LayoutDto() { Viewport = string.Empty };

                var userFormat = _InitialDataService.GetUserFormat();
                var unitSystem = userFormat.UnitSystem;

                foreach (var item in itemListDto)
                {
                    item.Height = UnitConverter.ConvertFeetToMeters(item.HeightConvert, unitSystem);
                    item.Width = UnitConverter.ConvertFeetToMeters(item.WidthConvert, unitSystem);

                    var layout = MapperLayout(item);
                    actualLayoutName = item;
                    if (layout == null)
                        throw new InvalidOperationException("The layout could not be mapped correctly.");

                    operationDB.AddUpdate("Layouts", layout);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Layout successfully updated: {LayoutName}", actualLayoutName.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the Layout. DTO: {actualLayoutName.Name}");
                throw;
            }
        }
        public async Task DeleteLayout(LayoutDto itemDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                // Mapear el DTO al modelo de datos
                var layout = MapperLayout(itemDto);

                if (layout == null)
                {
                    throw new InvalidOperationException("The layout could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddDelete("Layouts", layout);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Layout added successfully: {LayoutName}", layout.Name);
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the Layout. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }
        public async Task DeleteListLayout(IEnumerable<LayoutDto> itemListDto)
        {
            try
            {
                operationDB = new OperationDB();
                actualLayoutName = new LayoutDto() { Viewport = string.Empty };
                foreach (var item in itemListDto)
                {
                    var layout = MapperLayout(item);
                    actualLayoutName = item;
                    if (layout == null)
                        throw new InvalidOperationException("The layout could not be mapped correctly.");
                    var warehouseId = layout.WarehouseId;
                    var flows = _dataAccess.GetFlowsByWarehouse(warehouseId);
                    foreach (var flow in flows)
                    {
                        switch (flow.Type)
                        {
                            case "Inbound":
                                {
                                    var inboundGraphs = _dataAccess.DeleteInboundFlowGraphsByFlow(flow.Id, warehouseId);
                                    foreach (var g in inboundGraphs)
                                        operationDB.AddDelete("InboundFlowGraphs", g);
                                    break;
                                }

                            case "Outbound":
                                {
                                    var outboundGraphs = _dataAccess.DeleteOutboundFlowGraphsByFlow(flow.Id, warehouseId);
                                    foreach (var g in outboundGraphs)
                                        operationDB.AddDelete("OutboundFlowGraphs", g);
                                    break;
                                }

                            case "Custom":
                                {
                                    var customGraphs = _dataAccess.DeleteCustomFlowGraphsByFlow(flow.Id);
                                    foreach (var g in customGraphs)
                                        operationDB.AddDelete("CustomFlowGraphs", g);
                                    break;
                                }

                            default:
                                _logger.LogWarning($"Tipo de Flow desconocido: {flow.Type}");
                                break;
                        }
                        operationDB.AddDelete("Flow", flow);
                    }
                    operationDB.AddDelete("Layouts", layout);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Layout deleted correctly: {actualLayoutName.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when deleting Layout. DTO: {actualLayoutName.Name}");
                throw;
            }
        }
        public LayoutDto? GetLayoutDto(Guid Id)
        {
            try
            {
                var layout = _dataAccess.GetLayoutById(Id); // Get the Layout from the database

                if (layout == null)
                {
                    _logger.LogWarning($"No Layout found with the Id: {Id}");
                    return null;
                }

                return MapperLayoutDto(layout); //Map Layout to LayoutDto and return it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the database Layout.");
                return null;
            }
        }
        public IEnumerable<LayoutDto> GetLayoutsDto(Guid userGuid)
        {
            try
            {
                var userFormat = _InitialDataService.GetUserFormat();
                var unitSystem = userFormat.UnitSystem;

                return _dataAccess.GetLayouts(userGuid)
                                  .Select(MapperLayoutDto)
                                  .Select(layout =>
                                  {
                                      layout.HeightConvert = UnitConverter.ConvertMetersToFeet(layout.Height, unitSystem);
                                      layout.WidthConvert = UnitConverter.ConvertMetersToFeet(layout.Width, unitSystem);
                                      return layout;
                                  });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the list of layouts from the database.");
                return Enumerable.Empty<LayoutDto>(); // Returns an empty list in case of error
            }
        }
        public Layout? GetLayout(Guid Id)
        {
            try
            {
                var layout = _dataAccess.GetLayoutById(Id); // Get the Layout from the database

                if (layout == null)
                {
                    _logger.LogWarning($"No Layout found with the Id: {Id}");
                    return null;
                }

                return layout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the database Layout.");
                return null;
            }
        }
        #endregion

        #region Mapper's Layout       
        private Layout MapperLayout(LayoutDto itemDto)
        {
            // Obtener el Warehouse asociado
            var warehouse = _dataAccess.GetWarehouses().FirstOrDefault(x => x.Id == itemDto.LayoutWarehouseDto.Id);

            if (warehouse == null)
            {
                throw new InvalidOperationException($"No warehouse with ID was found {itemDto.LayoutWarehouseDto.Id}.");
            }

            //Valor por defecto.
            var nextValue = GetLayoutsDto(_InitialDataService.GetDatauser()?.Id ?? Guid.Empty).Count();

            // Crear el nuevo Layout
            return new Layout
            {
                Id = itemDto.Id,
                WarehouseId = (Guid)itemDto.LayoutWarehouseDto.Id,
                Warehouse = warehouse,
                Name = string.IsNullOrWhiteSpace(itemDto.Name)
                ? $"Layout_{nextValue + 1}" // Si `Name` está vacío, usa el valor por defecto.
                : itemDto.Name, // Si no, toma el nombre definido en el DTO.
                Viewport = itemDto.Viewport,
                Width = itemDto.Width,
                Height = itemDto.Height,
                CreationDate = itemDto.CreationDate
            };
        }
        private LayoutDto MapperLayoutDto(Layout layout)
        {
            if (layout.Warehouse == null)
                throw new InvalidOperationException($"No warehouse with ID was found {layout.WarehouseId}.");

            LayoutDto? resp = LayoutDto.NewDto();

            resp.Id = layout.Id;
            resp.Name = layout.Name;
            resp.Viewport = layout.Viewport;
            resp.Width = layout.Width.Value;
            resp.Height = layout.Height.Value;
            resp.CreationDate = layout.CreationDate;
            resp.LayoutWarehouseDto.Id = layout.WarehouseId;
            resp.LayoutWarehouseDto.Name = layout.Warehouse.Name;
            return resp;
        }
        #endregion

        #region Objects Implementation
        public async Task AddListObjects(IEnumerable<ObjectsDto> itemListDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();
                foreach (var item in itemListDto)
                {
                    // Mapear el DTO al modelo de datos
                    var Object = MapperObjects(item);

                    if (Object == null)
                    {
                        throw new InvalidOperationException("The object could not be mapped correctly.");
                    }

                    // Agregar el nuevo layout a la base de datos
                    operationDB.AddNew("Objects", Object);

                    _logger.LogInformation($"Object added successfully: {Object}", Object.Id);
                }

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, $"Error adding the Objects. List:  {itemListDto.ToList()}");
                throw;
            }
        }

        public async Task DeleteListObjects(IEnumerable<ObjectsDto> itemListDto)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in itemListDto)
                {
                    var objects = MapperObjects(item);
                    if (objects == null)
                        throw new InvalidOperationException("The Objects could not be mapped correctly.");

                    operationDB.AddDelete("Objects", objects);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Objects deleted correctly: {itemListDto.ToList()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when deleting Objects. List:  {itemListDto.ToList()}");
                throw;
            }
        }

        public async Task UpdateListObjects(IEnumerable<ObjectsDto> itemListDto)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (var item in itemListDto)
                {
                    var objects = MapperObjects(item);

                    if (objects == null)
                        throw new InvalidOperationException("The ObjectsDto could not be mapped correctly.");

                    operationDB.AddUpdate("Objects", objects);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Layout successfully updated: {Objects}", itemListDto.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the Objects. DTO: {itemListDto.Count()}");
                throw;
            }
        }

        public IEnumerable<ObjectsDto> GetObjects(Guid Id)
        {
            try
            {
                // Obtener la lista de objetos desde la base de datos
                var objects = _dataAccess.GetObjectsByLayout(Id);

                // Si no hay objetos, registrar advertencia y devolver una lista vacía
                if (objects == null || !objects.Any())
                {
                    _logger.LogWarning("No objects were found with the Layout Id: {LayoutId}", Id);
                    return Enumerable.Empty<ObjectsDto>();
                }

                // Mapear la lista de objetos a DTOs y devolverla
                return objects.Select(MapperObjectsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving objects for Layout Id: {LayoutId}", Id);
                return Enumerable.Empty<ObjectsDto>(); // Retornar lista vacía en caso de error
            }
        }

        #endregion

        #region Mapper's Objects
        private Objects MapperObjects(ObjectsDto itemDto)
        {
            // Obtener el layout asociado
            var layout = _dataAccess.GetLayoutById((Guid)itemDto.LayoutId);

            if (layout == null)
                throw new InvalidOperationException($"No layout with ID was found {(Guid)itemDto.LayoutId}.");

            // Crear el nuevo Objects
            itemDto.Id = itemDto.Id ?? Guid.NewGuid();
            return new Objects
            {
                Id = itemDto.Id ?? Guid.Empty,
                Name = itemDto.Name ?? string.Empty,
                Type = itemDto.CanvasObjectType,
                Text = itemDto.Text,
                xInit = itemDto.X,
                yInit = itemDto.Y,
                xEnd = itemDto.X2,
                yEnd = itemDto.Y2,
                Width = itemDto.Width,
                Height = itemDto.Height,
                Top = itemDto.Top,
                Left = itemDto.Left,
                Angle = itemDto.Angle ?? 0,
                Layout = layout,
                LayoutId = (Guid)itemDto.LayoutId,
                ViewPort = JsonSerializer.Serialize(itemDto)
            };
        }

        private ObjectsDto MapperObjectsDto(Objects objects)
        {
            if (objects.Layout == null)
                throw new InvalidOperationException($"No layout with ID was found {objects.LayoutId}.");

            var resp = ObjectsDto.NewDto();
            //to do
            resp.Id = objects.Id;
            resp.CanvasObjectType = objects.Type;
            resp.Name = objects.Name;
            resp.Text = objects.Text;
            resp.Top = (float)objects.Top;
            resp.Left = (float)objects.Left;
            resp.X = (float)objects.xInit;
            resp.Y = (float)objects.yInit;
            resp.X2 = (float)objects.xEnd;
            resp.Y2 = (float)objects.yEnd;
            resp.Height = (float)objects.Height;
            resp.Width = (float)objects.Width;
            resp.Angle = (float)objects.Angle;
            resp.LayoutId = objects.LayoutId;
            resp.Viewport = objects.ViewPort;

            return resp;
        }
        #endregion

        #region Areas Implementation
        public async Task AddArea(AreaDto itemDto)
        {
            try
            {
                // Initialize database operation
                operationDB = new();

                // Mapping the DTO to the data model
                Area? area = MapperArea(itemDto);

                if (area == null)
                    throw new InvalidOperationException("The area could not be mapped correctly.");

                // Add the new layout to the database
                operationDB.AddNew("Areas", area);

                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Area added successfully: {AreaName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                // Log the error with additional details
                _logger.LogError(ex, "Error adding the Area. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateListAreas(IEnumerable<AreaDto> itemDtoList)
        {
            try
            {
                operationDB = new();
                foreach (AreaDto? item in itemDtoList)
                {
                    Area? area = MapperArea(item);

                    if (area == null)
                        throw new InvalidOperationException("The area could not be mapped correctly.");

                    operationDB.AddUpdate("Areas", area);
                }
                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully updated areas.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error updating areas. List:  {itemDtoList.ToList()}");
                throw;
            }
        }

        public async Task AddListAreas(IEnumerable<AreaDto> itemDtoList)
        {
            try
            {
                operationDB = new();
                foreach (AreaDto? item in itemDtoList)
                {
                    Area? area = MapperArea(item);

                    if (area == null)
                        throw new InvalidOperationException("The area could not be mapped correctly.");

                    operationDB.AddNew("Areas", area);
                }
                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully add areas.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error updating areas. List:  {itemDtoList.ToList()}");
                throw;
            }
        }


        public async Task UpdateArea(AreaDto itemDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                // Mapear el DTO al modelo de datos
                Area? area = MapperArea(itemDto);

                if (area == null)
                {
                    throw new InvalidOperationException("The layout could not be mapped correctly.");
                }

                // Agregar el nuevo layout a la base de datos
                operationDB.AddUpdate("Areas", area);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Layout added successfully: {LayoutName}", area.Name);
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the Layout. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task DeleteListAreas(IEnumerable<AreaDto> itemDtoList)
        {
            try
            {
                operationDB = new();
                foreach (AreaDto? item in itemDtoList)
                {
                    Area? area = MapperArea(item);

                    if (area == null)
                        throw new InvalidOperationException("The area could not be mapped correctly.");

                    operationDB.AddDelete("Areas", area);
                }
                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully deleted areas.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error deleted areas. List:  {itemDtoList.ToList()}");
                throw;
            }
        }

        public AreaDto? GetAreasById(Guid itemId)
        {
            try
            {
                Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(itemId); // Get the area from the database

                // If area is null, return null
                if (area == null)
                {
                    _logger.LogWarning($"No Area found with the Id: {itemId}");
                    return null;
                }

                return MapperAreaDto(area); // Map Area to AreaDto
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Area from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<AreaDto> GetAreasDtoByLayout(Guid layoutId)
        {
            try
            {
                // Obtener los layouts y mapearlos a LayoutDto utilizando LINQ
                IEnumerable<Area> areas = _dataAccess.GetAreasByLayout(layoutId); // IEnumerable<Area>
                List<AreaDto> areaDtosList = new();

                foreach (Area area in areas.OrderBy(x => x.Name))
                {
                     AreaDto areaDto = new()
                    {
                        Id = area.Id,
                        Name = area?.Name,
                        LayoutId = area?.LayoutId,
                        Layout = area?.Layout,

                        Inbound = false,
                        Outbound = false,
                        AreaType = GetAreaTypeEnum(area.Type),
                        AreaTypeName = area.Type,
                        NarrowAisle = area.NarrowAisle,
                        IsAutomatic = area.IsAutomatic,
                        AlternativeAreaId = area.AlternativeAreaId,
                        DelayedTimePerUnit = TimeSpan.FromHours((double)area.DelayedTimePerUnit)
                    };

                    areaDtosList.Add(areaDto);
                }

                return areaDtosList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the list of areas from the database.");
                return Enumerable.Empty<AreaDto>(); // Retorna una lista vacía en caso de error
            }
        }

        public string GetVieportsByTypeObject(Guid idObject, string TypeObject) 
        {
            var idArea = Guid.NewGuid();
            switch (TypeObject) 
            {
                case "Area":
                    idArea = idObject;
                    break;
            }

            return GetViwportByArea(idArea);
        }

        public string GetViwportByArea(Guid idArea) 
        {
            var areasArray = new JsonArray();
            var objectsArray = new JsonArray();
            var routesArray = new JsonArray();
            var routesSet = new HashSet<RouteDto>(); // This list prevents duplicate entries in Routes
            var equipmentsArray = new JsonArray();
            var zonesArray = new JsonArray();
            var processArray = new JsonArray();
            var shelfsArray = new JsonArray();
            var processDirectionsArray = new JsonArray();
            var stepsArray = new JsonArray();
            var flowsArray = new JsonArray();

            Area area = _dataAccess.GetAreaByAreaId(idArea);
            if (area != null) 
            {
                areasArray.Add(JsonDocument.Parse(area.ViewPort).RootElement.Clone());
                
                  // Search Routes in Areas
                IEnumerable<Route>? routes = _dataAccess.GetRoutesByAreaIdAsNoTracking(area.Id).GroupBy(r => r.Id).Select(g => g.First());
                foreach (Route route in routes)
                {
                    var jsonString = MapperRouteDto(route);
                    if (routesSet.Add(jsonString)) 
                        routesArray.Add(JsonSerializer.SerializeToElement(jsonString));
                }
                IEnumerable<EquipmentGroup> equipmentList = _dataAccess.GetEquipmentsByAreaIdAsNoTracking(area.Id);
                foreach (EquipmentGroup equipmentGroup in equipmentList)
                {
                    equipmentsArray.Add(JsonSerializer.SerializeToElement(MapperEquipmentDesignerDto(equipmentGroup)));
                }

                IEnumerable<Zone> zoneList = _dataAccess.GetZonesWithAreaByAreaIdNoTracking(area.Id);
                foreach (Zone zone in zoneList.Where(e => !string.IsNullOrEmpty(e.ViewPort)))
                {
                    if (zone.Type != "Shelf" && zone.Type != "Rack" && zone.Type != "DriveIn" && zone.Type != "ChaoticStorage" && zone.Type != "AutomaticStorage")
                        zonesArray.Add(JsonDocument.Parse(zone.ViewPort).RootElement.Clone());
                    else
                    {
                        switch (zone.Type)
                        {
                            case "Rack":
                                {
                                    var rackes = _dataAccess.GetRacksWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (rackes == null)
                                        break;
                                    if (!string.IsNullOrWhiteSpace(rackes.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(rackes.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in Rack (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }

                                    break;
                                }
                            case "DriveIn":
                                {
                                    var driveIns = _dataAccess.GetDriveInsWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (driveIns == null)
                                        break;
                                    if (!string.IsNullOrWhiteSpace(driveIns.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(driveIns.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in DriveIn (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }
                                    break;
                                }
                            case "ChaoticStorage":
                                {
                                    var chaotics = _dataAccess.GetChaoticWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (chaotics == null)
                                        break;
                                    if (!string.IsNullOrWhiteSpace(chaotics.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(chaotics.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in ChaoticStorage (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }

                                    break;
                                }
                            case "AutomaticStorage":
                                {
                                    var automatics = _dataAccess.GetAutomaticStoragesWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (automatics == null)
                                        break;

                                    if (!string.IsNullOrWhiteSpace(automatics.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(automatics.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in AutomaticStorage (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }

                var processes = GetProcesByType(area.Id);
                var processIds = processes
                       .Where(p => p.Id.HasValue)
                       .Select(p => p.Id!.Value)
                       .ToHashSet();

                var pds = _dataAccess.GetProcessDirectionPropertyByProcessIdsAsNoTracking(processIds);

                foreach (var pd in pds)
                {
                    processDirectionsArray.Add(
                        JsonSerializer.SerializeToElement(MapperProcessDirectionPropertyDto(pd))
                    );
                }
                foreach (ProcessDto processDto in processes)
                {
                    processArray.Add(JsonSerializer.SerializeToElement(processDto));
                    IEnumerable<Step> steps = _dataAccess.GetStepsListWithProcessByProcessIdNoTracking(processDto.Id ?? Guid.Empty); // Gets the Steps from the Process
                    foreach (var step in steps)
                    {
                        stepsArray.Add(JsonSerializer.SerializeToElement(MapperStepDto(step))); // Add Steps to each Process
                    }
                }
            }

            var finalObject = new JsonObject
            {
                ["Areas"] = areasArray,
                ["Objects"] = objectsArray,
                ["Routes"] = routesArray,
                ["Equipments"] = equipmentsArray,
                ["Stations"] = zonesArray,
                ["Process"] = processArray,
                ["Shelf"] = shelfsArray,
                ["ProcessDirections"] = processDirectionsArray,
                ["Steps"] = stepsArray,
                ["Flows"] = flowsArray
            };

            return finalObject.ToJsonString();
        }

        #endregion

        #region Mapper's Area
        public Area MapperArea(AreaDto areaDto)
        {
            Layout? layout = _dataAccess.GetLayoutById(areaDto.LayoutId ?? Guid.Empty);

            if (layout == null)
                throw new InvalidOperationException($"No layout with ID was found {areaDto.LayoutId}.");

            Area? alternativeArea = areaDto.AlternativeArea is null ? null : _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(areaDto.AlternativeArea.Id ?? Guid.Empty);

            return new Area
            {
                Id = (Guid)areaDto.Id,
                Name = areaDto.Name ?? string.Empty,
                Type = GetAreaTypeString(areaDto.AreaType ?? AreaType.Dock),
                xInit = areaDto.X,
                yInit = areaDto.Y,
                xEnd = areaDto.Width,
                yEnd = areaDto.Height,
                DelayedTimePerUnit = areaDto.DelayedTimePerUnit.Hours,
                IsAutomatic = areaDto.IsAutomatic,
                NarrowAisle = areaDto.NarrowAisle,
                AlternativeAreaId = alternativeArea?.Id,
                AlternativeArea = alternativeArea,
                LayoutId = (Guid)areaDto.LayoutId,
                Layout = layout,
                ViewPort = JsonSerializer.Serialize(areaDto)
            };
        }

        public AreaDto MapperAreaDto(Area area)
        {
            AreaDto? alternativeArea = new();
            AreaDto? areaViewport = new();

            if (null == area.Layout)
                throw new InvalidOperationException($"No layout with ID was found {area.LayoutId}.");

            if (area.AlternativeAreaId.HasValue && area.AlternativeAreaId != Guid.Empty)
            {
                if (null == area.AlternativeArea)
                    area.AlternativeArea = _dataAccess.GetAreaById(area.AlternativeAreaId ?? Guid.Empty);

                alternativeArea = MapperAreaDto(area.AlternativeArea);

                if (alternativeArea == null)
                    throw new InvalidOperationException($"No alternativeArea with ID was found {alternativeArea.AlternativeAreaId}.");
            }

            if (!string.IsNullOrEmpty(area.ViewPort))
                areaViewport = JsonSerializer.Deserialize<AreaDto>(area.ViewPort);

            return new AreaDto
            {
                Id = area.Id,
                CanvasObjectType = areaViewport?.CanvasObjectType,
                Name = areaViewport?.Name ?? area.Name,
                X = areaViewport.X,
                Y = areaViewport.Y,
                Height = areaViewport.Height,
                Width = areaViewport.Width,
                StatusObject = areaViewport.StatusObject ?? string.Empty,
                LayoutId = area.LayoutId,
                Layout = area.Layout,
                Inbound = false,
                Outbound = false,
                AreaType = GetAreaTypeEnum(area.Type),
                AreaTypeName = area.Type,
                NarrowAisle = area.NarrowAisle,
                IsAutomatic = area.IsAutomatic,
                AlternativeAreaId = area.AlternativeAreaId,
                AlternativeArea = alternativeArea,
                DelayedTimePerUnit = TimeSpan.FromHours((double)area.DelayedTimePerUnit)
            };
        }

        private AreaDto GetAreaDto(Guid? alternativeAreaId)
        {
            AreaDto areaDto = null;

            try
            {
                Area area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking((Guid)alternativeAreaId);

                areaDto = new()
                {
                    Id = area.Id,
                    Name = area.Name,
                    Inbound = false,
                    Outbound = false,
                    AreaType = (AreaType)Enum.Parse(typeof(AreaType), area.Type),
                    NarrowAisle = area.NarrowAisle,
                    IsAutomatic = area.IsAutomatic,
                    AlternativeAreaId = area.AlternativeAreaId ?? Guid.Empty,
                    DelayedTimePerUnit = TimeSpan.FromMinutes(area.DelayedTimePerUnit ?? 0),
                    X = (float)(area.xInit ?? 0),
                    Y = (float)(area.yInit ?? 0),
                    Height = (float)(area.yEnd - area.yInit ?? 0),
                    Width = (float)(area.xEnd - area.xInit ?? 0),
                    LayoutId = area.LayoutId
                };
                return areaDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the list of areas from the database.");
                return areaDto;
            }
        }


        #endregion

        #region Viewports
        public string GetViewportsByLayoutId(Guid layoutId, Guid warehouseId)
        {
            var areasArray = new JsonArray();
            var objectsArray = new JsonArray();
            var routesArray = new JsonArray();
            var routesSet = new HashSet<RouteDto>(); // This list prevents duplicate entries in Routes
            var equipmentsArray = new JsonArray();
            var zonesArray = new JsonArray();
            var processArray = new JsonArray();
            var shelfsArray = new JsonArray();
            var processDirectionsArray = new JsonArray();
            var stepsArray = new JsonArray();
            var flowsArray = new JsonArray();

            IEnumerable<Area>? areas = _dataAccess.GetAreasByLayout(layoutId);
            IEnumerable<Objects>? objects = _dataAccess.GetObjectsByLayout(layoutId);
            IEnumerable<ProcessDirectionProperty> processDirections = _dataAccess.GetAllProcessDirectionPropertyByLayoutId(layoutId);
            OutboundFlowGraph? outboundFlow = _dataAccess.GetOutboundFlowGraphByWarehouseId(warehouseId);
            InboundFlowGraph? inboundFlow = _dataAccess.GetInboundFlowGraphByWarehouseid(warehouseId);

            foreach (Objects? obj in objects.Where(o => !string.IsNullOrEmpty(o.ViewPort)))
            {
                objectsArray.Add(JsonDocument.Parse(obj.ViewPort).RootElement.Clone());
            }

            foreach (var item in processDirections)
            {
                processDirectionsArray.Add(JsonSerializer.SerializeToElement(MapperProcessDirectionPropertyDto(item)));
            }

            foreach (Area area in areas)
            {
                if (string.IsNullOrEmpty(area.ViewPort))
                {
                    var areaDto = AreaDto.NewViewPortAreaGrid();

                    areaDto.Id = area.Id;                 
                    areaDto.Name = area.Name ?? string.Empty;
                    areaDto.AreaType = GetAreaTypeEnum(area.Type);

                    area.ViewPort = JsonSerializer.Serialize(new
                    {
                        areaDto.Id,
                        areaDto.AreaType,
                        areaDto.AlternativeAreaId,
                        areaDto.CanvasObjectType,
                        areaDto.Name,
                        areaDto.X,
                        areaDto.Y,
                        areaDto.Height,
                        areaDto.Width,
                        areaDto.StatusObject,
                    });
                }
             
                areasArray.Add(JsonDocument.Parse(area.ViewPort).RootElement.Clone());

                // Search Routes in Areas
                IEnumerable<Route>? routes = _dataAccess.GetRoutesByAreaIdAsNoTracking(area.Id).GroupBy(r => r.Id).Select(g => g.First());
                foreach (Route route in routes)
                {
                    var jsonString = MapperRouteDto(route);
                    if (routesSet.Add(jsonString)) // HashSet automatically avoids duplicate records through the Id
                        routesArray.Add(JsonSerializer.SerializeToElement(jsonString));
                }

                // Search Equipments in Areas
                IEnumerable<EquipmentGroup> equipmentList = _dataAccess.GetEquipmentsByAreaIdAsNoTracking(area.Id);
                foreach (EquipmentGroup equipmentGroup in equipmentList)
                {
                    equipmentsArray.Add(JsonSerializer.SerializeToElement(MapperEquipmentDesignerDto(equipmentGroup)));
                }

                IEnumerable<Zone> zoneList = _dataAccess.GetZonesWithAreaByAreaIdNoTracking(area.Id);
                foreach (Zone zone in zoneList)
                {
                    if (string.IsNullOrEmpty(zone.ViewPort))
                    {
                        var NewZoneDto = ZoneDto.NewViewPortZonaGrid();
                        NewZoneDto.Id = zone.Id;
                        NewZoneDto.Name = zone.Name;
                        NewZoneDto.AreaId = zone.AreaId;
                        NewZoneDto.ZoneType = ZoneTypeGetMethods.GetStringZoneType(zone.Type);
                        zone.ViewPort = JsonSerializer.Serialize(new
                        {                          
                            zone.Id,
                            zone.Name,
                            NewZoneDto.X,
                            NewZoneDto.Y,
                            NewZoneDto.Height,
                            NewZoneDto.Width,
                            NewZoneDto.ZoneType,
                            zone.AreaId,
                            NewZoneDto.StatusObject,
                            NewZoneDto.Orientation,
                            NewZoneDto.CanvasObjectType
                        });
                    }
                    if (zone.Type != "Shelf" && zone.Type != "Rack" && zone.Type != "DriveIn" && zone.Type != "ChaoticStorage" && zone.Type != "AutomaticStorage")
                    {

                        zonesArray.Add(JsonDocument.Parse(zone.ViewPort).RootElement.Clone());
                    }
                    else
                    {
                        switch (zone.Type)
                        {
                            case "Rack":
                                {
                                    var rackes = _dataAccess.GetRacksWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (rackes == null)
                                        break;
                                    if (!string.IsNullOrWhiteSpace(rackes.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(rackes.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in Rack (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }
                                    else
                                    {
                                        var Newrackes = RackDto.NewViewportDto();
                                        Newrackes.ZoneId = zone.Id;
                                        Newrackes.Name = zone.Name;
                                        Newrackes.Id = rackes.Id;
                                        Newrackes.ViewPort = JsonSerializer.Serialize(new
                                        {
                                            Newrackes.ZoneId,
                                            Newrackes.NumCrossAisles,
                                            Newrackes.NumShelves,
                                            Newrackes.IsVertical,
                                            Newrackes.CanvasObjectType,
                                            zone.AreaId,
                                            Newrackes.QuantityHeigth,
                                            Newrackes.QuantityWidth,
                                            Newrackes.LocationTypes,
                                            Newrackes.Id,
                                            zone.Name,
                                            Newrackes.X,
                                            Newrackes.Y,
                                            Newrackes.Height,
                                            Newrackes.Width,
                                            Newrackes.StatusObject
                                        });
                                        using var doc = JsonDocument.Parse(Newrackes.ViewPort);
                                        shelfsArray.Add(doc.RootElement.Clone());
                                    }

                                    break;
                                }
                            case "DriveIn":
                                {
                                    var driveIns = _dataAccess.GetDriveInsWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (driveIns == null)
                                        break;
                                    if (!string.IsNullOrWhiteSpace(driveIns.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(driveIns.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in DriveIn (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }
                                    else
                                    {
                                        var NewDriveins = DriveInDto.NewViewportDriveInDto();
                                        NewDriveins.ZoneId = zone.Id;
                                        NewDriveins.Name = zone.Name;
                                        NewDriveins.Id = driveIns.Id;
                                        NewDriveins.ViewPort = JsonSerializer.Serialize(new
                                        {
                                            NewDriveins.ZoneId,
                                            NewDriveins.NumCrossAisles,
                                            NewDriveins.NumShelves,
                                            NewDriveins.IsVertical,
                                            NewDriveins.LocationTypes,                                          
                                            zone.AreaId,
                                            NewDriveins.Id,
                                            NewDriveins.CanvasObjectType,                                   
                                            zone.Name,
                                            NewDriveins.X,
                                            NewDriveins.Y,
                                            NewDriveins.Height,
                                            NewDriveins.Width,
                                            NewDriveins.StatusObject
                                        });
                                        using var doc = JsonDocument.Parse(NewDriveins.ViewPort);
                                        shelfsArray.Add(doc.RootElement.Clone());
                                    }
                                    break;
                                }
                            case "ChaoticStorage":
                                {
                                    var chaotics = _dataAccess.GetChaoticWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (chaotics == null)
                                        break;
                                    if (!string.IsNullOrWhiteSpace(chaotics.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(chaotics.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in ChaoticStorage (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }
                                    else
                                    {
                                        var NewChaoticStorage = ChaoticStorageDto.NewViewPortDto();
                                        NewChaoticStorage.ZoneId = zone.Id;
                                        NewChaoticStorage.Name = zone.Name;
                                        NewChaoticStorage.Id = chaotics.Id;
                                        NewChaoticStorage.ViewPort = JsonSerializer.Serialize(new
                                        {
                                            NewChaoticStorage.ZoneId,
                                            NewChaoticStorage.LocationTypes,
                                            zone.AreaId,
                                            NewChaoticStorage.Id,
                                            NewChaoticStorage.CanvasObjectType,
                                            zone.Name,
                                            NewChaoticStorage.X,
                                            NewChaoticStorage.Y,
                                            NewChaoticStorage.Height,
                                            NewChaoticStorage.Width,
                                            NewChaoticStorage.StatusObject
                                        });
                                        using var doc = JsonDocument.Parse(NewChaoticStorage.ViewPort);
                                        shelfsArray.Add(doc.RootElement.Clone());
                                    }

                                    break;
                                }
                            case "AutomaticStorage":
                                {
                                    var automatics = _dataAccess.GetAutomaticStoragesWithZoneByZoneIdAsNoTracking(zone.Id);
                                    if (automatics == null)
                                        break;

                                    if (!string.IsNullOrWhiteSpace(automatics.ViewPort))
                                    {
                                        try
                                        {
                                            using var doc = JsonDocument.Parse(automatics.ViewPort);
                                            shelfsArray.Add(doc.RootElement.Clone());
                                        }
                                        catch (JsonException ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse ViewPort in AutomaticStorage (ZoneId: {ZoneId})", zone.Id);
                                        }
                                    }
                                    else
                                    {
                                        var NewAutomaticStorage = AutomaticStorageDto.NewViewPortDto();
                                        NewAutomaticStorage.ZoneId = zone.Id;
                                        NewAutomaticStorage.Name = zone.Name;
                                        NewAutomaticStorage.Id = automatics.Id;
                                        NewAutomaticStorage.ViewPort = JsonSerializer.Serialize(new
                                        {
                                            NewAutomaticStorage.ZoneId,
                                            NewAutomaticStorage.NumCrossAisles,
                                            NewAutomaticStorage.NumShelves,
                                            NewAutomaticStorage.IsVertical,
                                            NewAutomaticStorage.LocationTypes,
                                            zone.AreaId,
                                            NewAutomaticStorage.Id,
                                            NewAutomaticStorage.CanvasObjectType,
                                            zone.Name,
                                            NewAutomaticStorage.X,
                                            NewAutomaticStorage.Y,
                                            NewAutomaticStorage.Height,
                                            NewAutomaticStorage.Width,
                                            NewAutomaticStorage.StatusObject
                                        });
                                        using var doc = JsonDocument.Parse(NewAutomaticStorage.ViewPort);
                                        shelfsArray.Add(doc.RootElement.Clone());
                                    }
                                    break;
                                }
                        }
                    }
                }

                List<ProcessDto>? processes = GetProcesByType(area.Id);
                foreach (ProcessDto processDto in processes)
                {
                    processArray.Add(JsonSerializer.SerializeToElement(processDto));
                    IEnumerable<Step> steps = _dataAccess.GetStepsListWithProcessByProcessIdNoTracking(processDto.Id ?? Guid.Empty); // Gets the Steps from the Process
                    foreach (var step in steps)
                    {
                        stepsArray.Add(JsonSerializer.SerializeToElement(MapperStepDto(step))); // Add Steps to each Process
                    }
                }
            }

            if (inboundFlow != null)
                flowsArray.Add(JsonSerializer.SerializeToElement(MapperFlowDto(inboundFlow, FlowType.Inbound)));

            if (outboundFlow != null)
                flowsArray.Add(JsonSerializer.SerializeToElement(MapperFlowDto(outboundFlow, FlowType.Outbound)));

            var finalObject = new JsonObject
            {
                ["Areas"] = areasArray,
                ["Objects"] = objectsArray,
                ["Routes"] = routesArray,
                ["Equipments"] = equipmentsArray,
                ["Stations"] = zonesArray,
                ["Process"] = processArray,
                ["Shelf"] = shelfsArray,
                ["ProcessDirections"] = processDirectionsArray,
                ["Steps"] = stepsArray,
                ["Flows"] = flowsArray
            };

            return finalObject.ToJsonString();
        }
        #endregion

        public async Task CloneLayout(LayoutDto itemDto)
        {
            try
            {
                // Obtener los layouts y mapearlos a LayoutDto utilizando LINQ
                await _simulateService.CloneLayout(itemDto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clonning layout");
            }
        }

        #region Route Implementation
        public async Task AddRouteDto(RouteDto itemDto)
        {
            try
            {
                operationDB = new();

                Route? route = MapperRoute(itemDto);

                if (route == null)
                    throw new InvalidOperationException("The route could not be mapped correctly.");

                // Add the new route to the database
                operationDB.AddNew("Routes", route);

                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Route added successfully: {RouteName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                // Log the error with additional details
                _logger.LogError(ex, "Error adding the Route. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateListRoutesDto(IEnumerable<RouteDto> itemDtoList)
        {
            try
            {
                operationDB = new();
                foreach (RouteDto? item in itemDtoList)
                {
                    Route? route = MapperRoute(item);

                    if (route == null)
                        throw new InvalidOperationException("The route could not be mapped correctly.");

                    operationDB.AddUpdate("Routes", route);
                }
                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully updated routes.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error updating routes. List:  {itemDtoList}");
                throw;
            }
        }

        public async Task UpdateRouteDto(RouteDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();

                Route? route = MapperRoute(itemDto);

                if (route == null)
                    throw new InvalidOperationException("The route could not be mapped correctly.");

                operationDB.AddUpdate("Routes", route);

                // Save changes in DataBase
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Route added successfully: {route.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding the route. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeleteListRoutes(IEnumerable<Route> itemList)
        {
            try
            {
                operationDB = new();
                foreach (Route? route in itemList)
                {
                    if (route == null)
                        throw new InvalidOperationException("The route could not be mapped correctly.");

                    operationDB.AddDelete("Routes", route);
                }
                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully deleted routes.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error deleted routes. List:  {itemList}");
                throw;
            }
        }

        public IEnumerable<Route?> GetRoutesByIdArea(Guid areaId)
        {
            try
            {
                return _dataAccess.GetRoutesByAreaIdNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Route from the database.");
                return Enumerable.Empty<Route>(); // Returns null in case of error
            }
        }

        public List<RouteDto?> GetRoutesDtoByIdArea(Guid areaId)
        {
            try
            {
                List<RouteDto> routesDto = new();
                var routes = _dataAccess.GetRoutesByAreaIdNoTracking(areaId);
                foreach (Route route in routes)
                {
                    routesDto.Add(MapperRouteDto(route));
                }
                return routesDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Route from the database.");
                return null; // Returns null in case of error
            }
        }

        public RouteDto? GetRoutesDtoById(Guid itemId)
        {
            try
            {
                Route? route = _dataAccess.GetRouteByIdAsNoTracking(itemId); // Get the Route from the database

                // If Route is null, return null
                if (route == null)
                {
                    _logger.LogWarning($"No Route found with the Id: {itemId}");
                    return null;
                }

                return MapperRouteDto(route); // Map Route to RouteDto
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Route from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<Route>? GetRoutesListByRouteDtoList(HashSet<RouteDto> itemsList)
        {
            try
            {
                List<Route> routeList = new();
                foreach (var routeDto in itemsList)
                {
                    // Get the Route from the database
                    Route? route = _dataAccess.GetRouteByIdWithAreaAsNoTracking(routeDto.Id ?? Guid.Empty);

                    // If Route is null, return null
                    if (route == null)
                    {
                        _logger.LogWarning($"No Route found with the Id: {routeDto.Id}");
                        return null;
                    }

                    routeList.Add(route);
                }
                return routeList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Route from the database.");
                return null; // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's Route
        private Route MapperRoute(RouteDto routeDto)
        {
            Area? areaInbound = _dataAccess.GetAreaByAreaId((Guid)routeDto.InboundAreaId);
            Area? areaOutbound = _dataAccess.GetAreaByAreaId((Guid)routeDto.OutboundAreaId);

            if (areaInbound == null)
                throw new InvalidOperationException($"No area with ID was found {routeDto.InboundArea.Id}.");
            if (areaOutbound == null)
                throw new InvalidOperationException($"No area with ID was found {routeDto.OutboundArea.Id}.");

            return new Route
            {
                Id = routeDto.Id ?? Guid.NewGuid(),
                Name = routeDto.Name ?? string.Empty,
                DepartureAreaId = areaOutbound.Id,
                DepartureArea = areaOutbound,
                ArrivalAreaId = areaInbound.Id,
                ArrivalArea = areaInbound,
                Bidirectional = routeDto.Bidirectional,
                TimeMin = routeDto.TimeMin,
                ViewPort = routeDto.ViewPort
            };
        }

        private RouteDto MapperRouteDto(Route route)
        {
            if (null == route.ArrivalArea)
                throw new InvalidOperationException($"No area with ID was found {route.ArrivalAreaId}.");
            if (null == route.DepartureArea)
                throw new InvalidOperationException($"No area with ID was found {route.DepartureAreaId}.");

            AreaDto? areaArrival = MapperAreaDto(route.ArrivalArea);
            AreaDto? areaDeparture = MapperAreaDto(route.DepartureArea);

            return new RouteDto
            {
                Id = route.Id,
                CanvasObjectType = "Routes",
                Name = route.Name,
                Bidirectional = route.Bidirectional,
                OutboundAreaId = route.DepartureAreaId,
                OutboundArea = areaDeparture,
                InboundAreaId = route.ArrivalAreaId,
                InboundArea = areaArrival,
                TimeMin = route.TimeMin,
                ViewPort = route.ViewPort // The Viewport stores the coordinates of the points on the route and obtains data after making movements at the points
            };
        }
        #endregion

        #region Equipments Implementation
        public async Task<Guid> AddEquipmentDesignerDto(EquipmentDesignerModalDto itemDto, bool isCreateNewArea)
        {
            try
            {
                operationDB = new();
                AreaDto areaDto = null;
                Guid newArea = Guid.Empty;

                if (isCreateNewArea)
                {
                    areaDto = itemDto.Area ?? throw new InvalidOperationException("The area could not be found."); // Extract the common AreaDto from the first DockDto's Zone
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }
                EquipmentGroup? equipment = MapperEquipmentModal(itemDto, areaDto);

                if (equipment == null)
                    throw new InvalidOperationException("The equipment could not be mapped correctly.");

                if (newArea == Guid.Empty)
                    newArea = equipment.AreaId;

                operationDB.AddNew("EquipmentGroups", equipment); // Add the new equipment to the database

                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes to the database

                _logger.LogInformation("Equipment added successfully: {EquipmentName}", itemDto.Id);
                return newArea;
            }
            catch (Exception ex)
            {
                // Log the error with additional details
                _logger.LogError(ex, "Error adding the Equipment. DTO: {@ItemDto}", itemDto.Id);
                throw;
            }
        }

        public async Task AddEquipmentDesignerDtoCopy(EquipmentDesignerDto equipmentDto)
        {
            try
            {
                operationDB = new();
                AreaDto areaDto = null;

                EquipmentGroup? equipment = AddEquipmentSetData(equipmentDto);

                if (equipment == null)
                    throw new InvalidOperationException("The equipment could not be mapped correctly.");

                operationDB.AddNew("EquipmentGroups", equipment); // Add the new equipment to the database

                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes to the database

                _logger.LogInformation("Equipment added successfully: {EquipmentName}", equipmentDto.Id);
            }
            catch (Exception ex)
            {
                // Log the error with additional details
                _logger.LogError(ex, "Error adding the Equipment. DTO: {@ItemDto}", equipmentDto.Id);
                throw;
            }
        }

        public async Task UpdateEquipmentDesignerDto(EquipmentDesignerDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();

                EquipmentGroup? equipment = MapperEquipment(itemDto);

                if (equipment == null)
                    throw new InvalidOperationException("The equipment could not be mapped correctly.");

                operationDB.AddUpdate("EquipmentGroups", equipment);

                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes in DataBase

                _logger.LogInformation($"Correctly upgraded equipment: {equipment.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the equipment. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeleteListEquipmentGroup(IEnumerable<EquipmentGroup> itemList)
        {
            try
            {
                operationDB = new();
                foreach (EquipmentGroup? equipment in itemList)
                {
                    if (equipment == null)
                        throw new InvalidOperationException("The equipment could not be mapped correctly.");

                    operationDB.AddDelete("EquipmentGroups", equipment);
                }
                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes to the database
                _logger.LogInformation($"List of successfully deleted equipments.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleted equipments. List:  {itemList.ToList()}"); //Log the error with additional details
                throw;
            }
        }

        public IEnumerable<EquipmentGroup?> GetEquipmentGroupByIdArea(Guid areaId)
        {
            try
            {
                return _dataAccess.GetEquipmentsByAreaIdNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the equipment from the database.");
                return Enumerable.Empty<EquipmentGroup>(); // Returns null in case of error
            }
        }

        public IEnumerable<EquipmentDesignerDto?> GetEquipmentDesignerDtoByIdArea(Guid areaId)
        {
            try
            {
                IEnumerable<EquipmentGroup> equipment = _dataAccess.GetEquipmentsByAreaIdAsNoTracking(areaId);

                if (equipment == null || !equipment.Any())
                {
                    _logger.LogWarning("No Equipment found for AreaId: {areaId}", areaId);
                    return Enumerable.Empty<EquipmentDesignerDto>();
                }

                return equipment.Select(MapperEquipmentDesignerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the equipment from the database.");
                return Enumerable.Empty<EquipmentDesignerDto>(); // Returns null in case of error
            }
        }

        public EquipmentDesignerDto GetEquipmentDesignerDtoById(Guid itemId)
        {
            try
            {
                // Get the Route from the database
                EquipmentGroup? equipment = _dataAccess.GetEquipmentGroupWithTypeEquipmentByIdAsNoTracking(itemId);

                // If Route is null, return null
                if (equipment == null)
                {
                    _logger.LogWarning($"No equipment found with the Id: {itemId}");
                    return null;
                }

                // Map Route to RouteDto
                return MapperEquipmentDesignerDto(equipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Route from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<TypeEquipment?> GetTypeEquipmentByWarehouseIdNoTracking(Guid layoutId)
        {
            Layout? layout = _dataAccess.GetLayoutById(layoutId);
            return _dataAccess.GetEquipmentTypesByWarehouseIdNoTracking(layout.WarehouseId).OrderBy(x => x.Name);
        }

        public IEnumerable<EquipmentGroup> GetEquipmentGroupsListByEquipmentDesignerDtoList(IEnumerable<EquipmentDesignerDto> equipmentDesignerDtos)
        {
            try
            {
                List<EquipmentGroup> equipmentGroupsList = new();

                foreach (EquipmentDesignerDto equipmentGroup in equipmentDesignerDtos)
                {
                    EquipmentGroup? equipment = _dataAccess.GetEquipmentGroupsByIdAsNoTracking(equipmentGroup.Id ?? Guid.Empty); // Get the equipment from the database

                    if (null == equipment)
                    {
                        _logger.LogWarning($"No Equipment found with the Id: {equipment.Id}");
                        return null; // If Equipment is null, return null
                    }

                    equipmentGroupsList.Add(equipment);
                }
                return equipmentGroupsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Equipment from the database.");
                return null; // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's Equipment
        private EquipmentDesignerDto MapperEquipmentDesignerDto(EquipmentGroup equipment)
        {
            if (null == equipment.Area)
                throw new InvalidOperationException($"No area with ID was found {equipment.AreaId}.");

            if (null == equipment.TypeEquipment)
                throw new InvalidOperationException($"No type equipment with ID was found {equipment.TypeEquipmentId}.");

            return new EquipmentDesignerDto
            {
                Id = equipment.Id,
                Name = equipment.Name,
                NEquipment = equipment.Equipments,
                TypeEquipment = equipment.TypeEquipment,
                TypeEquipmentId = equipment.TypeEquipment.Id,
                NameTypeEquipment = equipment.TypeEquipment.Name,
                Area = MapperAreaDto(equipment.Area),
                AreaId = equipment.AreaId
            };
        }

        private EquipmentGroup MapperEquipment(EquipmentDesignerDto itemDto)
        {
            Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(itemDto.AreaId ?? Guid.Empty);

            if (area == null)
                throw new InvalidOperationException($"No area with ID was found {itemDto.AreaId}.");

            TypeEquipment? type = _dataAccess.GetTypeEquipmentByIdNoTracking(itemDto.TypeEquipmentId ?? Guid.Empty);

            if (type == null)
                throw new InvalidOperationException($"No type equipment with ID was found {itemDto.TypeEquipmentId}.");

            return new EquipmentGroup
            {
                Id = itemDto.Id ?? Guid.Empty,
                Name = itemDto.Name ?? string.Empty,
                Equipments = itemDto.NEquipment,
                TypeEquipment = type,
                TypeEquipmentId = itemDto.TypeEquipmentId ?? Guid.Empty,
                Area = area,
                AreaId = itemDto.AreaId ?? Guid.Empty,
                ViewPort = string.Empty
            };
        }

        private EquipmentGroup AddEquipmentSetData(EquipmentDesignerDto itemDto)
        {
            Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(itemDto.AreaId ?? Guid.Empty);

            if (area == null)
                throw new InvalidOperationException($"No area with ID was found {itemDto.AreaId}.");

            TypeEquipment? type = _dataAccess.GetTypeEquipmentByIdNoTracking(itemDto.TypeEquipmentId ?? Guid.Empty);

            if (type == null)
                throw new InvalidOperationException($"No type equipment with ID was found {itemDto.TypeEquipmentId}.");

            return new EquipmentGroup
            {
                Id = itemDto.Id ?? Guid.Empty,
                Name = itemDto.Name ?? string.Empty,
                Equipments = itemDto.NEquipment,
                TypeEquipment = type,
                TypeEquipmentId = itemDto.TypeEquipmentId ?? Guid.Empty,
                Area = area,
                AreaId = itemDto.AreaId ?? Guid.Empty,
                ViewPort = string.Empty
            };
        }

        private EquipmentGroup MapperEquipmentModal(EquipmentDesignerModalDto itemDto, AreaDto areaDto)
        {
            Guid? areaId = null;
            if (areaDto != null)
            {
                areaId = areaDto.Id;
            }
            Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(itemDto.AreaId ?? Guid.Empty);

            if (area == null && areaId == null)
                throw new InvalidOperationException($"No area with ID was found {itemDto.AreaId}.");

            TypeEquipment? type = _dataAccess.GetTypeEquipmentByIdNoTracking(itemDto.TypeEquipmentId ?? Guid.Empty);

            if (type == null)
                throw new InvalidOperationException($"No type equipment with ID was found {itemDto.TypeEquipmentId}.");

            return new EquipmentGroup
            {
                Id = itemDto.Id ?? Guid.Empty,
                Name = string.Empty,
                Equipments = itemDto.NEquipment,
                TypeEquipment = type,
                TypeEquipmentId = itemDto.TypeEquipmentId ?? Guid.Empty,
                Area = area,
                AreaId = areaId ?? itemDto.AreaId ?? Guid.Empty,
                ViewPort = string.Empty
            };
        }


        #endregion

        #region Zones Implementation

        public async Task AddZoneDto(Models.DTO.Designer.ZoneDto itemDto)
        {
            try
            {
                operationDB = new();

                Zone? zone = MapperZone(itemDto);

                if (zone == null)
                    throw new InvalidOperationException("The Zone could not be mapped correctly.");

                operationDB.AddNew("Zones", zone); // Add the new Zone to the database

                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes to the database

                _logger.LogInformation("Zone added successfully: {ZoneName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the Zone. DTO: {@ItemDto}", itemDto); // Log the error with additional details
                throw;
            }
        }

        public async Task UpdateZoneDto(Models.DTO.Designer.ZoneDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();

                Zone? zone = MapperZone(itemDto);

                if (zone == null)
                    throw new InvalidOperationException("The zone could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);

                // Save changes in DataBase
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Zone added successfully: {zone.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding the zone. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task UpdateListZones(IEnumerable<Models.DTO.Designer.ZoneDto> itemDtoList)
        {
            try
            {
                if (itemDtoList == null)
                    throw new ArgumentNullException(nameof(itemDtoList));

                var list = itemDtoList
                    .Where(x => x != null)
                    .ToList();

                var groupedByArea = list
                    .GroupBy(x => x.AreaId)
                    .ToList();

                foreach (var group in groupedByArea)
                {
                    var areaId = group.Key;
                    operationDB = new();

                    foreach (var item in group)
                    {
                        var zone = MapperZone(item);
                        if (zone == null)
                        {
                            _logger.LogWarning("ZoneDto with Id '{ZoneId}' could not be mapped and was skipped.", item.Id);
                            continue;
                        }

                        operationDB.AddUpdate("Zones", zone);
                    }

                    await _simulateService.SaveChangesInDataBase(operationDB);
                    _logger.LogInformation("Successfully updated {Count} zones for AreaId: {AreaId}.", group.Count(), areaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating grouped zones. Total Count: {Count}", itemDtoList?.Count() ?? 0);
                throw;
            }
        }

        public async Task DeleteListZones(IEnumerable<Zone> itemList)
        {
            try
            {
                operationDB = new();
                foreach (Zone? zone in itemList)
                {
                    if (zone == null)
                        throw new InvalidOperationException("The zone could not be mapped correctly.");

                    operationDB.AddDelete("Zones", zone);
                }
                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully deleted zones.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error deleted zones. List:  {itemList}");
                throw;
            }
        }

        public IEnumerable<Zone> GetZonesByAreaNoTracking(Guid areaId)
        {
            return _dataAccess.GetZonesWithAreaByAreaIdNoTracking(areaId);
        }

        public IEnumerable<Models.DTO.Designer.ZoneDto> GetZonesDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                List<Models.DTO.Designer.ZoneDto?> zoneDtosList = new List<Models.DTO.Designer.ZoneDto?>();
                IEnumerable<Zone> zoneList = _dataAccess.GetZonesWithAreaByAreaIdNoTracking(areaId);
                foreach (var zone in zoneList)
                {
                    Models.DTO.Designer.ZoneDto zoneDto = MapperZoneDto(zone);
                    zoneDtosList.Add(zoneDto);
                }
                return zoneDtosList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the list of zones from the database.");
                return Enumerable.Empty<Models.DTO.Designer.ZoneDto>(); // Returns null in case of error
            }
        }

        public Models.DTO.Designer.ZoneDto? GetZoneDtoById(Guid zoneId)
        {
            try
            {
                Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(zoneId);
                if (null == zone)
                {
                    _logger.LogWarning($"No zone found with the Id: {zoneId}");
                    return null;
                }
                return MapperZoneDto(zone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the zone from the database.");
                return null; // Returns null in case of error
            }
        }

        public Zone? GetZoneByIdNoTracking(Guid zoneId)
        {
            try
            {
                Zone? zone = _dataAccess.GetZoneByIdNoTracking(zoneId);
                if (null == zone)
                {
                    _logger.LogWarning($"No Zone found with the Id: {zoneId}");
                    return null;
                }

                Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                if (null == area)
                {
                    _logger.LogWarning($"No area found with the Id: {zone.AreaId}");
                    return null;
                }
                zone.Area = area;

                return zone;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Zone from the database.");
                return null; // Returns null in case of error
            }
        }
        #endregion

        #region Mapper Zone
        private Zone MapperAddZone(Models.DTO.Designer.ZoneDto zoneDto, bool haveAreaData = false)
        {
            Area area = new Area // Initialize the variable area
            {
                Name = string.Empty,
                Type = GetAreaTypeString(zoneDto?.Area.AreaType ?? AreaType.Dock),
                LayoutId = Guid.Empty,
                Layout = new Layout
                {
                    Name = string.Empty,
                    Warehouse = new Warehouse
                    {
                        Name = string.Empty,
                        Code = string.Empty,
                        MeasureSystem = string.Empty
                    }
                }
            };

            zoneDto.StatusObject = "Update";
            zoneDto.Orientation = zoneDto.Orientation ?? OrientationType.None;

            return new Zone
            {
                Id = zoneDto.Id ?? Guid.NewGuid(),
                Name = zoneDto.Name ?? string.Empty,
                Type = ZoneTypeGetMethods.GetZoneTypeString(zoneDto.ZoneType),
                xInit = zoneDto.X,
                yInit = zoneDto.Y,
                xEnd = zoneDto.Width,
                yEnd = zoneDto.Height,
                MaxStockToBook = zoneDto.MaxStockToBook,
                IsLimitStock = zoneDto.IsLimitStock,
                MaxContainers = zoneDto.MaxContainers,
                MaxStock = zoneDto.MaxStock,
                MaxEquipments = zoneDto.MaxEquipments,
                InitStockToBook = zoneDto.InitStockToBook,
                AreaId = zoneDto.AreaId,
                Area = area,
                ViewPort = JsonSerializer.Serialize(new
                {
                    zoneDto.Id,
                    zoneDto.Name,
                    zoneDto.X,
                    zoneDto.Y,
                    zoneDto.Width,
                    zoneDto.Height,
                    zoneDto.ZoneType,
                    zoneDto.AreaId,
                    zoneDto.StatusObject,
                    zoneDto.Orientation,
                    zoneDto.CanvasObjectType
                })
            };
        }

        private Zone MapperUpdateZone(Models.DTO.Designer.ZoneDto zoneDto)
        {
            // Basic validation: we need an Id to update
            if (zoneDto.Id == null || zoneDto.Id == Guid.Empty)
                throw new InvalidOperationException("The zone cannot be updated without a valid ID.");

            Zone? existingZone = _dataAccess.GetZoneByIdNoTracking((Guid)zoneDto.Id);
            if (existingZone == null)
                throw new InvalidOperationException($"The zone with ID {zoneDto.Id} was not found.");

            // Verify the area receiving the DTO
            Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zoneDto.AreaId);
            if (area == null)
                throw new InvalidOperationException($"The area with ID {zoneDto.AreaId} was not found");



            // Sobrescribir propiedades (incluido el nombre si así lo deseas)
            // Si NO quieres cambiar el nombre, quita la siguiente línea.
            if (!string.IsNullOrWhiteSpace(zoneDto.Name))
                existingZone.Name = zoneDto.Name;

            existingZone.Type = ZoneTypeGetMethods.GetZoneTypeString(zoneDto.ZoneType);
            existingZone.xInit = zoneDto.X;
            existingZone.yInit = zoneDto.Y;
            existingZone.xEnd = zoneDto.Width;
            existingZone.yEnd = zoneDto.Height;
            existingZone.MaxStockToBook = zoneDto.MaxStockToBook;
            existingZone.IsLimitStock = zoneDto.IsLimitStock;
            existingZone.MaxStock = zoneDto.MaxStock;
            existingZone.MaxContainers = zoneDto.MaxContainers;
            existingZone.InitStockToBook = zoneDto.InitStockToBook;
            existingZone.MaxEquipments = zoneDto.MaxEquipments;

            // Si el área puede cambiar, actualiza también el AreaId y la navegación
            existingZone.AreaId = zoneDto.AreaId;
            existingZone.Area = area;

            existingZone.ViewPort = JsonSerializer.Serialize(new
            {
                zoneDto.Id,
                existingZone.Name,
                zoneDto.X,
                zoneDto.Y,
                zoneDto.Width,
                zoneDto.Height,
                zoneDto.ZoneType,
                zoneDto.AreaId,
                zoneDto.StatusObject,
                zoneDto.Orientation,
                zoneDto.CanvasObjectType
            });

            // Retorna la estación actualizada.
            // (En tu capa de acceso a datos seguramente harás el guardado/commit más adelante)
            return existingZone;
        }

        private Zone MapperZone(Models.DTO.Designer.ZoneDto zoneDto)
        {
            Area area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zoneDto.AreaId);

            if (null == area)
                throw new InvalidOperationException($"No area with ID was found {zoneDto.AreaId}.");

            return new Zone
            {
                Id = zoneDto.Id ?? Guid.Empty,
                Name = zoneDto.Name,
                Type = ZoneTypeGetMethods.GetZoneTypeString(zoneDto.ZoneType),
                xInit = zoneDto.X,
                yInit = zoneDto.Y,
                xEnd = zoneDto.Width,
                yEnd = zoneDto.Height,
                MaxStockToBook = zoneDto.MaxStockToBook,
                IsLimitStock = zoneDto.IsLimitStock,
                MaxStock = zoneDto.MaxStock,
                InitStockToBook = zoneDto.InitStockToBook,
                AreaId = zoneDto.AreaId,
                Area = area,
                ViewPort = JsonSerializer.Serialize(new
                {
                    zoneDto.Id,
                    zoneDto.Name,
                    zoneDto.X,
                    zoneDto.Y,
                    zoneDto.Width,
                    zoneDto.Height,
                    zoneDto.ZoneType,
                    zoneDto.AreaId,
                    zoneDto.StatusObject,
                    zoneDto.Orientation,
                    zoneDto.CanvasObjectType
                })
            };
        }

        private Models.DTO.Designer.ZoneDto MapperZoneDto(Zone zone)
        {
            if (null == zone.Area)
                throw new InvalidOperationException($"No zone with ID was found {zone.AreaId}.");
            ZoneDto zoneDto = JsonSerializer.Deserialize<ZoneDto>(zone.ViewPort);
            return new Models.DTO.Designer.ZoneDto
            {
                Id = zone.Id,
                CanvasObjectType = "Station",
                Name = zone.Name,
                X = (float)(zone.xInit ?? 0),
                Y = (float)(zone.yInit ?? 0),
                Height = (float)(zone.yEnd ?? 0),
                Width = (float)(zone.xEnd ?? 0),
                LayoutId = zone.Area.LayoutId,
                Layout = zone.Area.Layout,
                StatusObject = string.Empty,
                ZoneType = ZoneTypeGetMethods.GetStringZoneType(zone.Type),
                MaxStockToBook = zone.MaxStockToBook,
                IsLimitStock = zone.IsLimitStock,
                MaxContainers = zone.MaxContainers ?? 0,
                MaxStock = zone.MaxStock,
                InitStockToBook = zone.InitStockToBook,
                AreaId = zone.AreaId,
                Area = MapperAreaDto(zone.Area),
                ViewPort = zone.ViewPort,
                Orientation = zoneDto.Orientation,
                MaxEquipments = zone.MaxEquipments ?? 0
            };
        }
        #endregion

        #region Aisle Implementation
        public async Task<Guid> AddAisle(List<AisleDto> aisleDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB();
                Guid newArea = Guid.Empty;

                // Validate that the input list is not null or empty
                if (aisleDtoList == null || aisleDtoList.Count == 0)
                    throw new ArgumentException("The aisle list is empty.", nameof(aisleDtoList));

                if (isCreateNewArea)
                {
                    // Extract the common AreaDto from the first DockDto's Zone
                    AreaDto areaDto = aisleDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found.");
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }

                CalculateName(aisleDtoList, dto => dto.Zone!);

                foreach (var aisleDto in aisleDtoList)
                {
                    Zone zone = MapperAddZone(aisleDto.Zone ?? new(), isCreateNewArea); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;
                    if (zone == null)
                        throw new InvalidOperationException("The zone could not be mapped correctly.");

                    Aisle aisle = MapperAisle(aisleDto, zone);
                    if (aisle == null)
                        throw new InvalidOperationException("The aisle could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;
                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("Aisles", aisle);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Aisles added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Aisle list.");
                throw;
            }
        }

        public async Task UpdateAisle(AisleDto aisleDto)
        {
            try
            {
                operationDB = new OperationDB();

                Zone zone = MapperUpdateZone(aisleDto.Zone);
                if (zone == null)
                    throw new InvalidOperationException("The zone could not be mapped correctly.");

                var aisle = MapperUpdateAisle(aisleDto);
                if (aisle == null)
                    throw new InvalidOperationException("The aisle could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("Aisles", aisle);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Aisle updated successfully: {AisleId}", aisle.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Aisle. DTO: {@aisleDto}", aisleDto);
                throw;
            }
        }

        public async Task DeleteAisle(Aisle aisle)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", aisle.Zone);
                operationDB.AddDelete("Aisles", aisle);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Aisle deleted successfully: {AisleId}", aisle.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Aisle. DTO: {@aisleDto}", aisle);
                throw;
            }
        }

        public async Task DeleteListAisle(IEnumerable<Aisle> aisleList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in aisleList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);

                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("Aisles", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of aisles deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of Aisles. DTO List: {@aisleDtoList}", aisleList);
                throw;
            }
        }

        public IEnumerable<AisleDto> GetAislesByZoneByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var aisles = _dataAccess.GetAislesByAreaIdAsNoTracking(areaId);
                if (aisles == null || !aisles.Any())
                {
                    _logger.LogWarning("No Aisles found for AreaId: {areaId}", areaId);
                    return Enumerable.Empty<AisleDto>();
                }
                return aisles.Select(a => MapperAisleDto(a));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Aisles for AreaId: {areaId}", areaId);
                return Enumerable.Empty<AisleDto>();
            }
        }

        public AisleDto GetAisleDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var aisle = _dataAccess.GetAisleWithZoneByZoneIdAsNoTracking(zoneId);
                if (aisle == null)
                {
                    _logger.LogWarning("No Docks found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperAisleDto(aisle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Docks for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public AisleDto? GetAisleDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var aisle = _dataAccess.GetAisleWithZoneByAisleIdAsNoTracking(itemId);
                if (aisle == null)
                {
                    _logger.LogWarning("No Aisle found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperAisleDto(aisle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Aisle with Id: {itemId}", itemId);
                return null;
            }
        }

        public Aisle? GetAisleByIdNoTracking(Guid itemId)
        {
            try
            {
                var aisle = _dataAccess.GetAisleByIdNoTracking(itemId);
                if (aisle == null)
                {
                    _logger.LogWarning("No Aisle found with Id: {itemId}", itemId);
                    return null;
                }
                return aisle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Aisle with Id: {itemId}", itemId);
                return null;
            }
        }

        public Aisle? GetAisleByIdZoneAsNoTracking(Guid itemId)
        {
            try
            {
                var aisle = _dataAccess.GetAisleWithZoneByZoneIdAsNoTracking(itemId);
                if (aisle == null)
                {
                    _logger.LogWarning("No Aisle found with Id: {itemId}", itemId);
                    return null;
                }
                return aisle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Aisle with Id: {itemId}", itemId);
                return null;
            }
        }

        public IEnumerable<Aisle?> GetAislesByIdArea(Guid areaId)
        {
            try
            {
                return _dataAccess.GetAislesByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<Aisle>(); // Returns null in case of error
            }
        }
        #endregion

        #region Dock Implementation
        public async Task<Guid> AddDock(List<DockDto> dockDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB(); // Create a new operation context to group DB operations
                Guid newArea = Guid.Empty;

                // Validate that the input list is not null or empty
                if (dockDtoList == null || dockDtoList.Count == 0)
                    throw new ArgumentException("The dock list is empty.", nameof(dockDtoList));

                if (isCreateNewArea)
                {
                    // Extract the common AreaDto from the first DockDto's Zone
                    AreaDto areaDto = dockDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found.");
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }

                CalculateName(dockDtoList, dto => dto.Zone!);

                // Iterate through each dock to map and add its related zone and dock data
                foreach (var dockDto in dockDtoList)
                {
                    Zone zone = MapperAddZone(dockDto.Zone ?? new(), isCreateNewArea); // Map the Zone from DTO; use in-memory counter when haveAreaData is true

                    if (zone == null)
                        throw new InvalidOperationException("The Zone could not be mapped correctly.");

                    Dock dock = MapperDock(dockDto, zone); // Map the Dock with the associated zone
                    if (dock == null)
                        throw new InvalidOperationException("The Dock could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;
                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("Docks", dock);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Docks added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Dock list.");
                throw;
            }
        }

        public async Task AddDockCopy(DockDto dockDto)
        {
            try
            {
                operationDB = new OperationDB();

                if (dockDto == null)
                    throw new ArgumentNullException(nameof(dockDto), "The dock data is null.");

                // Asignar nombre calculado (si aplica)
                CalculateName(new List<DockDto> { dockDto }, dto => dto.Zone!);

                Zone zone = MapperAddZone(dockDto.Zone ?? new());
                if (zone == null)
                    throw new InvalidOperationException("The Zone could not be mapped correctly.");

                Dock dock = MapperDock(dockDto, zone);
                if (dock == null)
                    throw new InvalidOperationException("The Dock could not be mapped correctly.");

                operationDB.AddNew("Zones", zone);
                operationDB.AddNew("Docks", dock);

                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Dock added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Dock.");
                throw;
            }
        }

        public async Task UpdateDock(DockDto dockDto)
        {
            try
            {
                operationDB = new OperationDB();
                CalculateName(new List<ZoneDto> { dockDto.Zone }, dto => dto);

                Zone zone = MapperUpdateZone(dockDto.Zone);
                if (zone == null)
                    throw new InvalidOperationException("The Zone could not be mapped correctly.");

                Dock dock = MapperUpdateDock(dockDto);
                if (dock == null)
                    throw new InvalidOperationException("The Dock could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("Docks", dock);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Dock updated successfully: {DockId}", dock.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Dock. DTO: {@dockDto}", dockDto);
                throw;
            }
        }

        public async Task DeleteDock(Dock dock)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", dock.Zone);
                operationDB.AddDelete("Docks", dock);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Dock deleted successfully: {DockId}", dock.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Dock. DTO: {@dockDto}", dock);
                throw;
            }
        }

        public async Task DeleteListDock(IEnumerable<Dock> dockList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in dockList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);
                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("Docks", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of Docks deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of Docks. DTO List: {@dockDtoList}", dockList);
                throw;
            }
        }

        public IEnumerable<DockDto> GetDocksDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var docks = _dataAccess.GetDocksWithZoneByAreaIdAsNoTracking(areaId);
                if (docks == null || !docks.Any())
                {
                    _logger.LogWarning("No Docks found for AreaId: {areaId}", areaId);
                    return Enumerable.Empty<DockDto>();
                }
                return docks.Select(MapperDockDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Docks for AreaId: {areaId}", areaId);
                return Enumerable.Empty<DockDto>();
            }
        }

        public (int nextInbound, int nextOutbound) GetNextDockRangesInboundOutbound(Guid areaId)
        {
            int maxInboundRange = _dataAccess.GetMaxInboundRangeByAreaId(areaId);
            int maxOutboundRange = _dataAccess.GetMaxOutboundRangeByAreaId(areaId);
            return (maxInboundRange, maxOutboundRange);
        }

        public IEnumerable<DockZoneSettingsDto> GetDockDtosByLayoutId(Guid layoutId) 
        {
            try
            {
                var docks = _dataAccess.GetDocksWithZoneByLayoutIdAsNoTracking(layoutId);
                if (docks == null || !docks.Any())
                {
                    _logger.LogWarning("No Docks found for LayoutId: {layoutId}", layoutId);
                    return Enumerable.Empty<DockZoneSettingsDto>();
                }
                return docks
                        .Select(x => new DockZoneSettingsDto
                        {
                            Id = (Guid)x.GetType().GetProperty("DockId").GetValue(x, null),
                            Name = (string)x.GetType().GetProperty("ZoneName").GetValue(x, null)
                        })
                        .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Docks for LayoutId: {layoutId}", layoutId);
                throw;
            }

        }

        public DockDto GetDocksDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var dock = _dataAccess.GetDockWithZoneByZoneId(zoneId);
                if (dock == null)
                {
                    _logger.LogWarning("No Docks found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperDockDto(dock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Docks for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public DockDto? GetDockDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var dock = _dataAccess.GetDockWithZoneByIdAsNoTracking(itemId);
                if (dock == null)
                {
                    _logger.LogWarning("No Dock found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperDockDto(dock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Dock with Id: {itemId}", itemId);
                return null;
            }
        }

        public Dock? GetDockByIdNoTracking(Guid itemId)
        {
            try
            {
                var dock = _dataAccess.GetDockByIdNoTracking(itemId);
                if (dock == null)
                {
                    _logger.LogWarning("No dock found with Id: {itemId}", itemId);
                    return null;
                }
                return dock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Dock with Id: {itemId}", itemId);
                return null;
            }
        }

        public Dock? GetDockByIdZoneAsNoTracking(Guid itemId)
        {
            try
            {
                var dock = _dataAccess.GetDockWithZoneByZoneId(itemId);
                if (dock == null)
                {
                    _logger.LogWarning("No dock found with Id: {itemId}", itemId);
                    return null;
                }
                return dock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Dock with Id: {itemId}", itemId);
                return null;
            }
        }

        public IEnumerable<Dock?> GetDocksByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetDocksByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<Dock>(); // Returns null in case of error
            }
        }

        private static int ExtractTrailingNumber(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return -1;

            // Extracts the number at the end of the name after a “_”.
            Match match = Regex.Match(name, @"_(\d+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                return number;

            return -1; // Si no se encontró número
        }
        #endregion

        #region Stages Implementation

        public async Task<Guid> AddStage(List<StageDto> stageDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB();
                Guid newArea = Guid.Empty;

                if (stageDtoList == null || stageDtoList.Count == 0)
                    throw new ArgumentNullException("The stage list is empty.", nameof(stageDtoList));

                if (isCreateNewArea)
                {
                    AreaDto areaDto = stageDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found.");
                    Area area = MapperArea(areaDto);
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area);
                }

                CalculateName(stageDtoList, dto => dto.Zone!);

                foreach (var stageDto in stageDtoList)
                {
                    Zone zone = MapperAddZone(stageDto.Zone ?? new(), isCreateNewArea);

                    if (zone == null)
                        throw new InvalidOperationException("The Zone could not be mapped correctly.");

                    Stage stage = MapperStage(stageDto, zone);
                    if (stage == null)
                        throw new InvalidOperationException("The Stage could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;
                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("Stages", stage);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Stages added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Stage list.");
                throw;
            }
        }

        public async Task AddStageCopy(StageDto stageDto)
        {
            try
            {
                operationDB = new OperationDB();

                if (stageDto == null)
                    throw new ArgumentNullException(nameof(stageDto), "The stage data is null.");

                CalculateName(new List<StageDto> { stageDto }, dto => dto.Zone!);

                Zone zone = MapperAddZone(stageDto.Zone ?? new());
                if (zone == null)
                    throw new InvalidOperationException("The Zone could not be mapped correctly.");

                Stage stage = MapperStage(stageDto, zone);
                if (stage == null)
                    throw new InvalidOperationException("The Stages could not be mapped correctly.");

                operationDB.AddNew("Zones", zone);
                operationDB.AddNew("Stages", stage);

                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Stage added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Stage.");
                throw;
            }
        }

        public async Task UpdateStage(StageDto stageDto)
        {
            try
            {
                operationDB = new OperationDB();
                CalculateName(new List<ZoneDto> { stageDto.Zone }, dto => dto);

                Zone zone = MapperUpdateZone(stageDto.Zone);
                if (zone == null)
                    throw new InvalidOperationException("The Zone could not be mapped correctly.");

                Stage stage = MapperUpdateStage(stageDto);
                if (stage == null)
                    throw new InvalidOperationException("The Stage could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("Stages", stage);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Stage updated successfully: {StageId}", stage.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Stage. DTO: {@stageDto}", stageDto);
                throw;
            }
        }

        public async Task DeleteStage(Stage stage)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", stage.Zone);
                operationDB.AddDelete("Stages", stage);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Stage deleted successfully: {StageId}", stage.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Stage. DTO: {@stageDto}", stage);
                throw;
            }
        }

        public async Task DeleteListStage(IEnumerable<Stage> stageList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in stageList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);
                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("Stages", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of Stages deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of Stages. DTO List: {@stageDtoList}", stageList);
                throw;
            }
        }

        public IEnumerable<StageDto> GetStagesDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var stages = _dataAccess.GetStagesWithZoneByAreaIdAsNoTracking(areaId);
                if (stages == null || !stages.Any())
                {
                    _logger.LogWarning("No Stages found for AreaId: {areaId}", areaId);
                    return Enumerable.Empty<StageDto>();
                }
                return stages.Select(MapperStageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Stages for AreaId: {areaId}", areaId);
                return Enumerable.Empty<StageDto>();
            }
        }

        public StageDto GetStagesDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var stage = _dataAccess.GetStageWithZoneByZoneId(zoneId);
                if (stage == null)
                {
                    _logger.LogWarning("No Stages found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperStageDto(stage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Stages for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public StageDto? GetStageDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var stage = _dataAccess.GetStageWithZoneByIdAsNoTracking(itemId);
                if (stage == null)
                {
                    _logger.LogWarning("No Stage found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperStageDto(stage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Dock with Id: {itemId}", itemId);
                return null;
            }
        }

        public Stage? GetStageByIdNoTracking(Guid itemId)
        {
            try
            {
                var stage = _dataAccess.GetStageByIdNoTracking(itemId);
                if (stage == null)
                {
                    _logger.LogWarning("No stage found with Id: {itemId}", itemId);
                    return null;
                }
                return stage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Stage with Id: {itemId}", itemId);
                return null;
            }
        }

        public IEnumerable<Stage?> GetStagesByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetStagesByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<Stage>();
            }
        }

        public Stage? GetStagesByIdZoneAsNoTracking(Guid itemId)
        {
            try
            {
                var stage = _dataAccess.GetStageWithZoneByZoneIdAsNoTracking(itemId);
                if (stage == null)
                {
                    _logger.LogWarning("No stage found with Id: {itemId}", itemId);
                    return null;
                }
                return stage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stage with Id: {itemId}", itemId);
                return null;
            }
        }

        #endregion

        #region Buffer Implementation
        public async Task<Guid> AddBuffer(List<BufferDto> bufferDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB();
                Guid newArea = Guid.Empty;
                // Validate that the input list is not null or empty
                if (bufferDtoList == null || bufferDtoList.Count == 0)
                    throw new ArgumentException("The buffer list is empty.", nameof(bufferDtoList));

                if (isCreateNewArea)
                {
                    AreaDto areaDto = bufferDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found."); // Extract the common AreaDto from the first DockDto's Zone
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }

                CalculateName(bufferDtoList, dto => dto.Zone!);

                foreach (var bufferDto in bufferDtoList)
                {
                    Zone zone = MapperAddZone(bufferDto.Zone ?? new(), isCreateNewArea); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;
                    if (zone == null)
                        throw new InvalidOperationException("The zone could not be mapped correctly.");

                    Buffer buffer = MapperBuffer(bufferDto, zone);
                    if (buffer == null)
                        throw new InvalidOperationException("The Buffer could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;

                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("Buffers", buffer);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Buffers added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Buffer list.");
                throw;
            }
        }

        public async Task AddBufferCopy(BufferDto bufferDtoList)
        {
            try
            {
                operationDB = new OperationDB();

                if (bufferDtoList == null)
                    throw new ArgumentNullException(nameof(bufferDtoList), "The buffer data is null.");

                // Asignar nombre calculado (si aplica)
                CalculateName(new List<BufferDto> { bufferDtoList }, dto => dto.Zone!);

                Zone zone = MapperAddZone(bufferDtoList.Zone ?? new());
                if (zone == null)
                    throw new InvalidOperationException("The Zone could not be mapped correctly.");

                Buffer buffer = MapperBuffer(bufferDtoList, zone);
                if (buffer == null)
                    throw new InvalidOperationException("The Buffer could not be mapped correctly.");

                operationDB.AddNew("Zones", zone);
                operationDB.AddNew("Buffers", buffer);

                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Buffers added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Buffers.");
                throw;
            }
        }

        public async Task UpdateBuffer(BufferDto bufferDto)
        {
            try
            {
                operationDB = new OperationDB();

                Zone zone = MapperUpdateZone(bufferDto.Zone);
                if (zone == null)
                    throw new InvalidOperationException("The zone could not be mapped correctly.");

                Buffer buffer = MapperUpdateBuffer(bufferDto);
                if (buffer == null)
                    throw new InvalidOperationException("The Buffer could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("Buffers", buffer);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Buffer updated successfully: {BufferId}", buffer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Buffer. DTO: {@bufferDto}", bufferDto);
                throw;
            }
        }

        public async Task DeleteBuffer(Buffer buffer)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", buffer.Zone);
                operationDB.AddDelete("Buffers", buffer);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Buffer deleted successfully: {BufferId}", buffer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Buffer. DTO: {@bufferDto}", buffer);
                throw;
            }
        }

        public async Task DeleteListBuffer(IEnumerable<Buffer> bufferList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in bufferList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);
                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("Buffers", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of Buffers deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of Buffers. DTO List: {@bufferDtoList}", bufferList);
                throw;
            }
        }

        public IEnumerable<BufferDto> GetBuffersDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var buffers = _dataAccess.GetBuffersWithZoneByAreaIdAsNoTracking(areaId);
                if (buffers == null || !buffers.Any())
                {
                    _logger.LogWarning("No Buffers found for AreaId: {areaId}", areaId);
                    return Enumerable.Empty<BufferDto>();
                }
                return buffers.Select(MapperBufferDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Buffers for AreaId: {areaId}", areaId);
                return Enumerable.Empty<BufferDto>();
            }
        }

        public BufferDto? GetBufferDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var buffer = _dataAccess.GetBufferWithZoneByIdAsNoTracking(itemId);
                if (buffer == null)
                {
                    _logger.LogWarning("No Buffer found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperBufferDto(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Buffer with Id: {itemId}", itemId);
                return null;
            }
        }

        public Buffer? GetBufferByIdNoTracking(Guid itemId)
        {
            try
            {
                var buffer = _dataAccess.GetBufferByIdNoTracking(itemId);
                if (buffer == null)
                {
                    _logger.LogWarning("No buffer found with Id: {itemId}", itemId);
                    return null;
                }
                return buffer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Buffer with Id: {itemId}", itemId);
                return null;
            }
        }

        public Buffer? GetBufferByIdZoneAsNoTracking(Guid itemId)
        {
            try
            {
                var buffer = _dataAccess.GetBufferWithZoneByZoneIdAsNoTracking(itemId);
                if (buffer == null)
                {
                    _logger.LogWarning("No dock found with Id: {itemId}", itemId);
                    return null;
                }
                return buffer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Dock with Id: {itemId}", itemId);
                return null;
            }
        }

        public BufferDto GetBufferDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var buffer = _dataAccess.GetBufferWithZoneByZoneIdAsNoTracking(zoneId);
                if (buffer == null)
                {
                    _logger.LogWarning("No buffer found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperBufferDto(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buffer for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public IEnumerable<Buffer?> GetBuffersByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetBuffersByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<Buffer>(); // Returns null in case of error
            }
        }

        public BufferDto GetBuffersDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var buffer = _dataAccess.GetBufferWithZoneByZoneId(zoneId);
                if (buffer == null)
                {
                    _logger.LogWarning("No buffers found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperBufferDto(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buffer for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }
        #endregion

        #region Mapper Methods for Aisle, Dock, Stage and Buffer

        private Aisle MapperUpdateAisle(AisleDto aisleDto)
        {
            // 1. Validación básica: se requiere un Id para actualizar
            if (aisleDto.Id == Guid.Empty)
                throw new InvalidOperationException("The aisle cannot be updated without a valid ID.");

            // 2. Obtener el Aisle existente
            Aisle? existingAisle = _dataAccess.GetAisleByIdNoTracking(aisleDto.Id);
            if (existingAisle == null)
                throw new InvalidOperationException($"Aisle with ID {aisleDto.Id} was not found.");

            // 3. Asignar todas las propiedades desde el DTO
            existingAisle.AdditionalTimePerUnitEntry = aisleDto.AdditionalTimePerUnitEntry;
            existingAisle.AdditionalTimePerUnitExit = aisleDto.AdditionalTimePerUnitExit;
            existingAisle.MaxTasks = aisleDto.MaxTasks;
            existingAisle.AisleChangeTime = aisleDto.AisleChangeTime;
            existingAisle.NarrowAisle = aisleDto.NarrowAisle;
            existingAisle.MaxPickers = aisleDto.MaxPickers;
            existingAisle.EndPicking = aisleDto.EndPicking;
            existingAisle.Bidirectional = aisleDto.Bidirectional;
            existingAisle.WidthPerDirection = aisleDto.WidthPerDirection;
            existingAisle.MaxMHEPickOrPutaway = aisleDto.MaxMHEPickOrPutaway;
            existingAisle.ReplenishmentControl = aisleDto.ReplenishmentControl;
            existingAisle.MaxMovement = aisleDto.MaxMovement;
            existingAisle.ViewPort = aisleDto.ViewPort;

            return existingAisle;
        }

        private Aisle MapperAisle(AisleDto dto, Zone station)
        {
            // Mapea el DTO a la entidad Aisle.
            return new Aisle
            {
                Id = dto.Id,
                AdditionalTimePerUnitEntry = dto.AdditionalTimePerUnitEntry,
                AdditionalTimePerUnitExit = dto.AdditionalTimePerUnitExit,
                MaxTasks = dto.MaxTasks,
                AisleChangeTime = dto.AisleChangeTime,
                NarrowAisle = dto.NarrowAisle,
                MaxPickers = dto.MaxPickers,
                EndPicking = dto.EndPicking,
                Bidirectional = dto.Bidirectional,
                WidthPerDirection = dto.WidthPerDirection,
                MaxMHEPickOrPutaway = dto.MaxMHEPickOrPutaway,
                ReplenishmentControl = dto.ReplenishmentControl,
                MaxMovement = dto.MaxMovement,
                ZoneId = station.Id,
                Zone = station,
                ViewPort = string.Empty
            };
        }

        private AisleDto MapperAisleDto(Aisle aisle)
        {
            if (aisle.Zone == null)
                throw new InvalidOperationException($"No zone with ID was found {aisle.ZoneId}.");

            if (aisle.Zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {aisle.Zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(aisle.Zone.Area);

            // Mapea la entidad Aisle al DTO.
            return new AisleDto
            {
                Id = aisle.Id,
                AdditionalTimePerUnitEntry = aisle.AdditionalTimePerUnitEntry,
                AdditionalTimePerUnitExit = aisle.AdditionalTimePerUnitExit,
                MaxTasks = aisle.MaxTasks,
                AisleChangeTime = aisle.AisleChangeTime,
                NarrowAisle = aisle.NarrowAisle,
                MaxPickers = aisle.MaxPickers,
                EndPicking = aisle.EndPicking,
                Bidirectional = aisle.Bidirectional,
                WidthPerDirection = aisle.WidthPerDirection,
                MaxMHEPickOrPutaway = aisle.MaxMHEPickOrPutaway,
                ReplenishmentControl = aisle.ReplenishmentControl,
                MaxMovement = aisle.MaxMovement,
                ZoneId = aisle.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(aisle.Zone, areaDto),
                ViewPort = aisle.ViewPort
            };
        }

        private Dock MapperUpdateDock(DockDto dockDto)
        {
            // Validación básica: se requiere un ID para actualizar
            if (dockDto.Id == Guid.Empty)
                throw new InvalidOperationException("You cannot update the dock without a valid ID.");

            // Obtener el dock existente
            Dock? existingDock = _dataAccess.GetDockByIdNoTracking(dockDto.Id);
            if (existingDock == null)
                throw new InvalidOperationException($"Dock with ID {dockDto.Id} was not found.");

            // Actualizar las propiedades en la entidad existente
            existingDock.OperatesFromBuffer = dockDto.OperatesFromBuffer;
            existingDock.OverloadHandling = dockDto.OverloadHandling;
            existingDock.InboundRange = dockDto.InboundRange;
            existingDock.OutboundRange = dockDto.OutboundRange;
            existingDock.MaxStockCrossdocking = dockDto.MaxStockCrossdocking;
            existingDock.AllowInbound = dockDto.AllowInbound;
            existingDock.AllowOutbound = dockDto.AllowOutbound;
            existingDock.ViewPort = JsonSerializer.Serialize(dockDto);

            return existingDock;
        }

        private Dock MapperDock(DockDto dockDto, Zone zone)
        {
            // Mapea el DTO a la entidad Dock.
            return new Dock
            {
                Id = dockDto.Id,
                OperatesFromBuffer = dockDto.OperatesFromBuffer,
                OverloadHandling = dockDto.OverloadHandling,
                InboundRange = dockDto.InboundRange,
                OutboundRange = dockDto.OutboundRange,
                MaxStockCrossdocking = dockDto.MaxStockCrossdocking,
                AllowInbound = dockDto.AllowInbound,
                AllowOutbound = dockDto.AllowOutbound,
                ZoneId = zone.Id,
                Zone = zone,
                ViewPort = string.Empty
            };
        }

        private DockDto MapperDockDto(Dock dock)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(dock.ZoneId);
            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {dock.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            // Mapea la entidad Dock al DTO.
            return new DockDto
            {
                Id = dock.Id,
                OperatesFromBuffer = dock.OperatesFromBuffer,
                OverloadHandling = dock.OverloadHandling,
                InboundRange = dock.InboundRange ?? 0,
                OutboundRange = dock.OutboundRange ?? 0,
                MaxStockCrossdocking = dock.MaxStockCrossdocking,
                AllowInbound = dock.AllowInbound,
                AllowOutbound = dock.AllowOutbound,
                ZoneId = dock.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                ViewPort = dock.ViewPort
            };
        }

        private Stage MapperStage(StageDto stageDto, Zone zone)
        {
            return new Stage
            {
                Id = stageDto.Id,
                EntryCapacity = stageDto.EntryCapacity,
                ExitCapacity = stageDto.ExitCapacity,
                MixCarriers = stageDto.MixCarriers,
                ZoneId = zone.Id,
                Zone = zone,
                ViewPort = string.Empty,
                IsIn = stageDto.IsIn,
                IsOut = stageDto.IsOut            
            };
        }

        private StageDto MapperStageDto(Stage stage)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(stage.ZoneId);
            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {stage.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            return new StageDto
            {
                Id = stage.Id,
                EntryCapacity = stage.EntryCapacity ?? 0,
                ExitCapacity = stage.ExitCapacity ?? 0,
                MixCarriers = stage.MixCarriers,
                ZoneId = stage.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                ViewPort = stage.ViewPort,
                IsIn = stage.IsIn,
                IsOut = stage.IsOut
                
            };
        }

        private Stage MapperUpdateStage(StageDto stageDto)
        {
            // Validación básica: se requiere un ID para actualizar
            if (stageDto.Id == Guid.Empty)
                throw new InvalidOperationException("You cannot update the stage without a valid ID.");

            // Obtener el stage existente
            Stage? existingStage = _dataAccess.GetStageByIdNoTracking(stageDto.Id);
            if (existingStage == null)
                throw new InvalidOperationException($"Stage with ID {stageDto.Id} was not found.");

            // Actualizar las propiedades en la entidad existente
            existingStage.EntryCapacity = stageDto.EntryCapacity;
            existingStage.ExitCapacity = stageDto.ExitCapacity;
            existingStage.MixCarriers = stageDto.MixCarriers;
            existingStage.IsIn = stageDto.IsIn;
            existingStage.IsOut = stageDto.IsOut;
            existingStage.ViewPort = JsonSerializer.Serialize(stageDto);

            return existingStage;
        }


        private Buffer MapperUpdateBuffer(BufferDto bufferDto)
        {
            // 1. Validación: se requiere un ID para actualizar
            if (bufferDto.Id == Guid.Empty)
                throw new InvalidOperationException("The buffer cannot be updated without a valid ID.");

            // 2. Obtener el Buffer existente desde la capa de acceso a datos
            Buffer? existingBuffer = _dataAccess.GetBufferByIdNoTracking(bufferDto.Id);
            if (existingBuffer == null)
                throw new InvalidOperationException($"The buffer with the ID {bufferDto.Id} was not found.");

            // 3. Asignar las propiedades desde el DTO
            existingBuffer.EntryCapacity = bufferDto.EntryCapacity;
            existingBuffer.ExitCapacity = bufferDto.ExitCapacity;
            existingBuffer.CapacityByMaterial = bufferDto.CapacityByMaterial;
            existingBuffer.Excess = bufferDto.Excess;
            existingBuffer.Type = GetBufferTypeString(bufferDto.BufferType);
            existingBuffer.NumShelves = bufferDto.NumShelves;
            existingBuffer.NumCrossAisles = bufferDto.NumCrossAisles;
            existingBuffer.IsVertical = bufferDto.IsVertical;
            existingBuffer.ViewPort = bufferDto.ViewPort;

            return existingBuffer;
        }

        private Buffer MapperBuffer(BufferDto dto, Zone station)
        {
            // Mapea el DTO a la entidad Buffer.
            return new Buffer
            {
                Id = dto.Id,
                EntryCapacity = dto.EntryCapacity,
                ExitCapacity = dto.ExitCapacity,
                CapacityByMaterial = dto.CapacityByMaterial,
                Excess = dto.Excess,
                ZoneId = station.Id,
                Zone = station,
                Type = GetBufferTypeString(dto.BufferType),
                NumShelves = dto.NumShelves,
                NumCrossAisles = dto.NumCrossAisles,
                IsVertical = dto.IsVertical,
                ViewPort = string.Empty
            };
        }

        private BufferDto MapperBufferDto(Buffer buffer)
        {
            if (buffer.Zone == null)
                throw new InvalidOperationException($"No zone with ID was found {buffer.ZoneId}.");

            if (buffer.Zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {buffer.Zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(buffer.Zone.Area);

            // Mapea la entidad Buffer al DTO.
            return new BufferDto
            {
                Id = buffer.Id,
                EntryCapacity = buffer.EntryCapacity,
                ExitCapacity = buffer.ExitCapacity,
                CapacityByMaterial = buffer.CapacityByMaterial,
                Excess = buffer.Excess,
                ZoneId = buffer.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(buffer.Zone, areaDto),
                BufferType = GetBufferTypeEnum(buffer.Type ?? GetBufferTypeString(BufferType.None)),
                NumShelves = buffer.NumShelves ?? 0,
                NumCrossAisles = buffer.NumCrossAisles ?? 0,
                IsVertical = buffer.IsVertical ?? false,
                ViewPort = buffer.ViewPort
            };
        }

        #endregion

        #region Process Implementation

        public async Task AddProcess(ProcessDto itemDto)
        {
            try
            {
                // Inicializar la operación de base de datos
                operationDB = new OperationDB();

                // Mapear el DTO al modelo de datos
                Process? process = MapperProcess(itemDto);

                if (process == null)
                    throw new InvalidOperationException("The Process could not be mapped correctly.");

                // Agregar el nuevo Process a la base de datos
                operationDB.AddNew("Processes", process);

                // Guardar los cambios en la base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Process added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                // Registrar el error con detalles adicionales
                _logger.LogError(ex, "Error adding the Process. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task AddProcessList(List<ProcessDto> itemDtoList)
        {
            try
            {
                operationDB = new();
                foreach (ProcessDto? itemDto in itemDtoList)
                {
                    Process? process = MapperProcess(itemDto);

                    if (process == null)
                        throw new InvalidOperationException("The process could not be mapped correctly.");

                    operationDB.AddNew("Processes", process);
                }
                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully add processs.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error updating processs. List:  {itemDtoList.ToList()}");
                throw;
            }
        }

        public async Task UpdateProcess(ProcessDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();

                Process? process = MapperProcess(itemDto);

                if (process == null)
                    throw new InvalidOperationException("The ProcessDto could not be mapped correctly.");

                // Agregar la operación de actualización
                operationDB.AddUpdate("Processes", process);

                // Guardar los cambios
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Process updated successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating the Process. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateListProcesses(IEnumerable<ProcessDto> itemListDto)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (var itemDto in itemListDto)
                {
                    Process? process = MapperProcess(itemDto);
                    if (process == null)
                        throw new InvalidOperationException("The ProcessDto could not be mapped correctly.");

                    operationDB.AddUpdate("Processes", process);
                }

                // Guardar los cambios
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("List of Processes updated successfully. Count: {Count}", itemListDto.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating list of Processes. Count: {Count}", itemListDto.Count());
                throw;
            }
        }

        public async Task DeleteListProcesses(IEnumerable<Process> itemList)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (var itemDto in itemList)
                {
                    Process? process = _dataAccess.GetProcessByIdNoTracking(itemDto.Id);
                    if (process == null)
                        throw new InvalidOperationException("The ProcessDto could not be mapped correctly for deletion.");

                    operationDB.AddDelete("Processes", process);
                }

                // Guardar los cambios
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("List of Processes deleted successfully. Count: {Count}", itemList.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of Processes. Count: {Count}", itemList.Count());
                throw;
            }
        }

        public IEnumerable<ProcessDto> GetProcessesByIdAreaNoTracking(Guid itemId)
        {
            try
            {
                var processes = _dataAccess.GetProcessesWithAreaByAreaIdAsNoTracking(itemId);

                if (processes == null || !processes.Any())
                {
                    _logger.LogWarning("No processes were found in the database.");
                    return Enumerable.Empty<ProcessDto>();
                }

                // Mapear cada Process a ProcessDto
                return processes.Select(p => MapperProcessDto(p!)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving processes from the database.");
                return Enumerable.Empty<ProcessDto>();
            }
        }

        public IEnumerable<ProcessDto> GetProcessesByLayoutIdNoTracking(Guid layoutId)
        {
            try
            {
                var processes = _dataAccess.GetAllProcessWithAreaByLayoutId(layoutId);

                if (processes == null || !processes.Any())
                {
                    _logger.LogWarning("No processes were found in the database.");
                    return Enumerable.Empty<ProcessDto>();
                }

                // Mapear cada Process a ProcessDto
                return processes.Select(p => MapperProcessDto(p!)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving processes from the database.");
                return Enumerable.Empty<ProcessDto>();
            }
        }

        public Process? GetProcessByIdNoTracking(Guid itemId)
        {
            try
            {
                Process? process = _dataAccess.GetProcessByIdNoTracking(itemId);
                if (process == null)
                {
                    _logger.LogWarning("No Process found with the Id: {itemId}", itemId);
                    return null;
                }
                return process;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Process by Id: {itemId}", itemId);
                return null;
            }
        }

        public ProcessDto? GetProcessDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                Process? process = _dataAccess.GetProcessWithAreaByProcessIdAsNoTracking(itemId);
                if (process == null)
                {
                    _logger.LogWarning("No Process found with the Id: {itemId}", itemId);
                    return null;
                }
                return MapperProcessDto(process);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Process by Id: {itemId}", itemId);
                return null;
            }
        }

        public IEnumerable<Process>? GetProcessListByProcessDtoList(IEnumerable<ProcessDto> processDtos)
        {
            try
            {
                List<Process> routeList = new();
                foreach (var routeDto in processDtos)
                {
                    // Get the Process from the database
                    Process? route = _dataAccess.GetProcessByIdNoTracking(routeDto.Id ?? Guid.Empty);

                    // If Process is null, return null
                    if (route == null)
                    {
                        _logger.LogWarning($"No Process found with the Id: {routeDto.Id}");
                        return null;
                    }

                    routeList.Add(route);
                }
                return routeList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Process from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<Process?> GetProcessByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetProcessesByIdAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<Process>(); // Returns null in case of error
            }
        }

        public List<ProcessDto> GetProcesByType(Guid areaId)
        {
            var lstProcessDto = new List<ProcessDto>();
            IEnumerable<Process> processes = _dataAccess.GetProcessesWithAreaByAreaIdAsNoTracking(areaId);

            foreach (var process in processes)
            {
                switch (GetProcessTypeEnum(process.Type))
                {
                    case ProcessType.CustomProcess:
                        var customsProcess = GetCustomProcessDtoByProcess(process.Id);
                        foreach (var item in customsProcess)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Inbound:
                        var inbound = GetInboundListByProcessId(process.Id);
                        foreach (var item in inbound)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Loading:
                        var loading = GetLoadingDtoListByProcessId(process.Id);
                        foreach (var item in loading)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Picking:
                        var picking = GetPickingListByProcessId(process.Id);
                        foreach (var item in picking)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Putaway:
                        var putaway = GetPutawayListByProcessId(process.Id);
                        foreach (var item in putaway)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Shipping:
                        var shippings = GetShippingListByProcessId(process.Id);
                        foreach (var item in shippings)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Reception:
                        var reception = GetReceptionByProcessId(process.Id);
                        foreach (var item in reception)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Replenishment:
                        var replenishment = GetReplenishmentByProcessId(process.Id);
                        foreach (var item in replenishment)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    case ProcessType.Packing:
                        var packing = GetPackingByProcessId(process.Id);
                        foreach (var item in packing)
                        {
                            var processDto = MapperProcessDto(process);
                            processDto.ProcessTypeId = item.Id;
                            lstProcessDto.Add(processDto);
                        }
                        break;
                    default:
                        var processDefaultDto = MapperProcessDto(process);
                        processDefaultDto.ProcessTypeId = Guid.Empty;
                        lstProcessDto.Add(processDefaultDto);
                        break;
                }
            }
            return lstProcessDto;
        }

        public List<ParentFlowDto> GetParentFlows(Guid layoutId)
        {
            return (List<ParentFlowDto>)_dataAccess.GetParentFlows(layoutId);
        }

        #endregion

        #region Mapper's Process

        private Process MapperProcess(ProcessDto itemDto)
        {
            // Ejemplo: Obtener el Area asociado
            Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(itemDto.AreaId);

            if (area == null)
                throw new InvalidOperationException($"No Area found with ID {itemDto.AreaId} for this Process.");

            Layout? layout = _dataAccess.GetLayoutById(area.LayoutId);

            if (layout == null)
                throw new InvalidOperationException($"No layout with ID was found {area.LayoutId}.");

            area.Layout = layout;


            if (area.AlternativeAreaId.HasValue && area.AlternativeAreaId != Guid.Empty)
            {
                Area? alternativeArea = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking((Guid)area.AlternativeAreaId);

                if (alternativeArea == null)
                    throw new InvalidOperationException($"No alternativeArea with ID was found {alternativeArea.AlternativeAreaId}.");

                area.AlternativeArea = alternativeArea;
            }

            // Retornar la entidad Process mapeada
            return new Process
            {
                Id = (Guid)itemDto.Id,
                Name = itemDto.Name ?? string.Empty,
                Type = itemDto.Type.ToString() ?? ProcessType.CustomProcess.ToString(),
                MinTime = itemDto.MinTime,
                PreprocessTime = itemDto.PreprocessTime,
                PostprocessTime = itemDto.PostprocessTime,
                IsWarehouseProcess = itemDto.IsWarehouseProcess,//(itemDto.Type.ToString() == ProcessType.Replenishment.ToString() || itemDto.Type.ToString() == ProcessType.CustomProcess.ToString()) ? true : false, // TODO Ruben 28/01/2026: Se comenta hasta tener la definición completa. Revisar la consulta en DesignerDataGrid.
                IsOut = itemDto.IsOut,
                IsIn = itemDto.IsIn,
                IsInitProcess = itemDto.IsInitProcess,
                PercentageInitProcess = itemDto.PercentageInitProcess,
                AreaId = itemDto.AreaId,
                Area = area,
                FlowId = itemDto.FlowId
                // ViewPort TODO RSCM:18/03/2025
            };
        }

        private ProcessDto MapperProcessDto(Process process)
        {
            if (null == process.Area)
                throw new InvalidOperationException($"No Area found with ID {process.AreaId} for this Process.");

            if (null == process.Area.Layout)
                throw new InvalidOperationException($"No layout with ID was found {process.Area.LayoutId}.");

            AreaDto? areaDto = MapperAreaDto(process.Area);

            ParentFlowDto? flow = MapperParentFlowDto(process.Flow);

            return new ProcessDto
            {
                Id = process.Id,
                Name = process.Name,
                Type = GetProcessTypeEnum(process.Type),
                CanvasObjectType = "Process",
                MinTime = process.MinTime,
                PreprocessTime = process.PreprocessTime,
                PostprocessTime = process.PostprocessTime,
                IsWarehouseProcess = process.IsWarehouseProcess,
                IsOut = process.IsOut,
                IsIn = process.IsIn,
                IsInitProcess = process.IsInitProcess,
                PercentageInitProcess = process.PercentageInitProcess,
                AreaId = process.AreaId,
                AreaDto = areaDto,
                Flow = flow,
                FlowId = process.FlowId
                // ViewPort TODO RSCM:18/03/2025
            };
        }

        private ParentFlowDto MapperParentFlowDto(Flow flow)
        {
            if (flow == null) return null;
            return new ParentFlowDto
            {
                Id = flow.Id,
                Name = flow.Name,
                Type = flow.Type,
            };
        }

        #endregion

        #region Chaotic Implementation
        public async Task<Guid> AddChaoticList(List<ChaoticStorageDto> chaoticStorageDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB();
                Guid newArea = Guid.Empty;

                // Validate that the input list is not null or empty
                if (chaoticStorageDtoList == null || chaoticStorageDtoList.Count == 0)
                    throw new ArgumentException("The chaotic list is empty.", nameof(chaoticStorageDtoList));

                if (isCreateNewArea)
                {
                    AreaDto areaDto = chaoticStorageDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found."); // Extract the common AreaDto from the first ChaoticStorageDto's Zone
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }

                CalculateName(chaoticStorageDtoList, dto => dto.Zone!);

                foreach (var chaoticDto in chaoticStorageDtoList)
                {
                    Zone zone = MapperAddZone(chaoticDto.Zone ?? new(), isCreateNewArea); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                    if (zone == null)
                        throw new InvalidOperationException("The zone could not be mapped correctly.");

                    var chaotic = MapperChaotic(chaoticDto, zone);
                    if (chaotic == null)
                        throw new InvalidOperationException("The Chaotic could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;
                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("ChaoticStorages", chaotic);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("ChaoticStorages added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Chaotic list.");
                throw;
            }
        }

        public async Task AddChaoticStorage(ChaoticStorageDto chaoticStorageDto)
        {
            try
            {
                operationDB = new OperationDB();

                // Validate that the input list is not null or empty
                if (chaoticStorageDto == null)
                    throw new ArgumentException("The chaoticStorage list is empty.", nameof(chaoticStorageDto));

                Zone zone = MapperAddZone(chaoticStorageDto.Zone ?? new(), false); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                if (zone == null)
                    throw new InvalidOperationException("The zone could not be mapped correctly.");

                var chaoticStorage = MapperChaotic(chaoticStorageDto, zone);
                if (chaoticStorage == null)
                    throw new InvalidOperationException("The ChaoticStorage could not be mapped correctly.");

                operationDB.AddNew("Zones", zone);
                operationDB.AddNew("ChaoticStorages", chaoticStorage);
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("ChaoticStorages added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ChaoticStorage list.");
                throw;
            }
        }

        public async Task UpdateChaotic(ChaoticStorageDto chaoticStorageDto)
        {
            try
            {
                operationDB = new OperationDB();

                Zone zone = MapperUpdateZone(chaoticStorageDto.Zone);
                if (zone == null)
                    throw new InvalidOperationException("The Chaotic could not be mapped correctly.");

                ChaoticStorage chaotic = MapperUpdateChaotic(chaoticStorageDto);
                if (chaotic == null)
                    throw new InvalidOperationException("The Chaotic could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("ChaoticStorages", chaotic);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Chaotic updated successfully: {chaoticid}", chaotic.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Chaotic. DTO: {@chaoticStorageDto}", chaoticStorageDto);
                throw;
            }
        }

        public async Task DeleteChaotic(ChaoticStorage chaotic)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", chaotic.Zone);
                operationDB.AddDelete("ChaoticStorages", chaotic);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("chaotic deleted successfully: {chaoticId}", chaotic.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Rack. DTO: {@chaoticDto}", chaotic);
                throw;
            }
        }

        public async Task DeleteListChaoticStorage(IEnumerable<ChaoticStorage> chaoticStorageList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in chaoticStorageList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);

                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("ChaoticStorages", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of ChaoticStorages deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of ChaoticStorages. DTO List: {@chaoticStorageDtoList}", chaoticStorageList);
                throw;
            }
        }

        public IEnumerable<ChaoticStorageDto> GetChaoticStoragesDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var chaoticStorages = _dataAccess.GetChaoticStoragesWithZoneByAreaIdAsNoTracking(areaId);
                return chaoticStorages.Select(MapperChaoticStorageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ChaoticStorages for AreaId: {areaId}", areaId);
                return Enumerable.Empty<ChaoticStorageDto>();
            }
        }

        public ChaoticStorageDto? GetChaoticStorageDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var chaoticStorage = _dataAccess.GetChaoticWithZoneByIdAsNoTracking(itemId);
                if (chaoticStorage == null)
                {
                    _logger.LogWarning("No ChaoticStorage found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperChaoticStorageDto(chaoticStorage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ChaoticStorage with Id: {itemId}", itemId);
                return null;
            }
        }

        public ChaoticStorageDto GetChaoticStorageDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var chaoticStorage = _dataAccess.GetChaoticStoragesWithZoneByZoneIdAsNoTracking(zoneId);
                if (chaoticStorage == null)
                {
                    _logger.LogWarning("No chaoticStorage found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperChaoticStorageDto(chaoticStorage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chaoticStorage for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public IEnumerable<ChaoticStorage?> GetChaoticStoragesByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetChaoticStoragesByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<ChaoticStorage>(); // Returns null in case of error
            }
        }

        public ChaoticStorage? GetChaoticByIdAsNoTracking(Guid chaoticId)
        {
            try
            {
                // Get the Rack from the database
                ChaoticStorage? route = _dataAccess.GetChaoticWithZoneByIdAsNoTracking(chaoticId);
                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Rack from the database.");
                return null; // Returns null in case of error
            }
        }


        #endregion

        #region Mapper's Caotic

        private ChaoticStorageDto MapperChaoticStorageDto(ChaoticStorage chaoticStorage)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(chaoticStorage.ZoneId);

            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {chaoticStorage.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            // Map the ChaoticStorage entity to the DTO.
            return new ChaoticStorageDto
            {
                Id = chaoticStorage.Id,
                CanvasObjectType = "ChaoticStorage",
                ZoneId = chaoticStorage.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                ViewPort = chaoticStorage.ViewPort,
                X = (float)(zone.xInit ?? 0),
                Y = (float)(zone.yInit ?? 0),
                Height = (float)(zone.yEnd ?? 0),
                Width = (float)(zone.xEnd ?? 0)
            };
        }

        #endregion

        #region Mapper's Shelf

        public ShelfDto MapperRackDtoToShelfDtoService(RackDto rackDto)
        {
            ShelfDto shelfDto = new ShelfDto();
            shelfDto = MapperRackDtoToShelfDto(rackDto);
            return shelfDto;
        }

        public List<ShelfDto> MapperRackDtoListToShelfDtoList(List<RackDto> rackDto)
        {
            List<ShelfDto> shelfList = new List<ShelfDto>();
            foreach (RackDto item in rackDto)
            {
                if (!string.IsNullOrEmpty(item.ViewPort))
                {
                    ShelfDto shelfDto = JsonSerializer.Deserialize<ShelfDto>(item.ViewPort);
                    shelfList.Add(shelfDto);
                }
            }
            return shelfList;
        }

        public List<ShelfDto> MapperDriveInDtoListToShelfDtoList(List<DriveInDto> driveInDto)
        {
            List<ShelfDto> shelfList = new List<ShelfDto>();
            foreach (DriveInDto item in driveInDto)
            {
                if (!string.IsNullOrEmpty(item.ViewPort))
                {
                    ShelfDto shelfDto = JsonSerializer.Deserialize<ShelfDto>(item.ViewPort);
                    shelfList.Add(shelfDto);
                }
            }
            return shelfList;
        }

        public List<ShelfDto> MapperChaoticStorageDtoListToShelfDtoList(List<ChaoticStorageDto> chaoticStorageDto)
        {
            List<ShelfDto> shelfList = new List<ShelfDto>();
            foreach (ChaoticStorageDto item in chaoticStorageDto)
            {
                if (!string.IsNullOrEmpty(item.ViewPort))
                {
                    ShelfDto shelfDto = JsonSerializer.Deserialize<ShelfDto>(item.ViewPort);
                    shelfList.Add(shelfDto);
                }
            }
            return shelfList;
        }

        public List<ShelfDto> MapperAutomaticStorageDtoListToShelfDtoList(List<AutomaticStorageDto> automaticStorageDto)
        {
            List<ShelfDto> shelfList = new List<ShelfDto>();
            foreach (AutomaticStorageDto item in automaticStorageDto)
            {
                if (!string.IsNullOrEmpty(item.ViewPort))
                {
                    ShelfDto shelfDto = JsonSerializer.Deserialize<ShelfDto>(item.ViewPort);
                    shelfList.Add(shelfDto);
                }
            }
            return shelfList;
        }

        #endregion

        #region Rack Implementation
        public async Task<Guid> AddRackList(List<RackDto> rackDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB();
                Guid newArea = Guid.Empty;
                // Validate that the input list is not null or empty
                if (rackDtoList == null || rackDtoList.Count == 0)
                    throw new ArgumentException("The rack list is empty.", nameof(rackDtoList));

                if (isCreateNewArea)
                {
                    AreaDto areaDto = rackDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found."); // Extract the common AreaDto from the first DockDto's Zone
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }

                CalculateName(rackDtoList, dto => dto.Zone!);

                foreach (var rackDto in rackDtoList)
                {
                    Zone zone = MapperAddZone(rackDto.Zone ?? new(), isCreateNewArea); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                    if (zone == null)
                        throw new InvalidOperationException("The zone could not be mapped correctly.");

                    var rack = MapperRack(rackDto, zone);
                    if (rack == null)
                        throw new InvalidOperationException("The Rack could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;
                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("Racks", rack);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Racks added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Rack list.");
                throw;
            }
        }

        public async Task AddRack(RackDto rackDto)
        {
            try
            {
                operationDB = new OperationDB();

                // Validate that the input list is not null or empty
                if (rackDto == null)
                    throw new ArgumentException("The rack list is empty.", nameof(rackDto));

                Zone zone = MapperAddZone(rackDto.Zone ?? new(), false); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                if (zone == null)
                    throw new InvalidOperationException("The zone could not be mapped correctly.");

                var rack = MapperRack(rackDto, zone);
                if (rack == null)
                    throw new InvalidOperationException("The Rack could not be mapped correctly.");

                operationDB.AddNew("Zones", zone);
                operationDB.AddNew("Racks", rack);
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Racks added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Rack list.");
                throw;
            }
        }

        public async Task UpdateRack(RackDto rackDto)
        {
            try
            {
                operationDB = new OperationDB();

                Zone zone = MapperUpdateZone(rackDto.Zone);
                if (zone == null)
                {
                    throw new InvalidOperationException("The Rack could not be mapped correctly.");
                }

                Rack rack = MapperUpdateRack(rackDto);
                if (rack == null)
                    throw new InvalidOperationException("The Rack could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("Racks", rack);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Rack updated successfully: {RackId}", rack.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Rack. DTO: {@rackDto}", rackDto);
                throw;
            }
        }

        public async Task DeleteRack(Rack rack)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", rack.Zone);
                operationDB.AddDelete("Racks", rack);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Rack deleted successfully: {RackId}", rack.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Rack. DTO: {@rackDto}", rack);
                throw;
            }
        }

        public async Task DeleteListRack(IEnumerable<Rack> rackList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in rackList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);

                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("Racks", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of Racks deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of Racks. DTO List: {@rackDtoList}", rackList);
                throw;
            }
        }

        public IEnumerable<RackDto> GetRacksDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var racks = _dataAccess.GetRacksWithZoneByAreaIdAsNoTracking(areaId);
                return racks.Select(MapperRackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Racks for AreaId: {areaId}", areaId);
                return Enumerable.Empty<RackDto>();
            }
        }

        public RackDto? GetRackDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var rack = _dataAccess.GetRackWithZoneByRackIdAsNoTracking(itemId);
                if (rack == null)
                {
                    _logger.LogWarning("No Rack found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperRackDto(rack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Rack with Id: {itemId}", itemId);
                return null;
            }
        }

        public Rack? GetRackByIdNoTracking(Guid itemId)
        {
            try
            {
                var rack = _dataAccess.GetRackByIdNoTracking(itemId);
                if (rack == null)
                {
                    _logger.LogWarning("No rack found with Id: {itemId}", itemId);
                    return null;
                }
                return rack;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Rack with Id: {itemId}", itemId);
                return null;
            }
        }

        public Rack? GetRackByIdZoneAsNoTracking(Guid itemId)
        {
            try
            {
                var rack = _dataAccess.GetRackByIdZone(itemId);
                if (rack == null)
                {
                    _logger.LogWarning("No rack found with Id: {itemId}", itemId);
                    return null;
                }
                return rack;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rack with Id: {itemId}", itemId);
                return null;
            }
        }

        public RackDto GetRackDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var rack = _dataAccess.GetRackWithZoneByIdZoneAsNoTraking(zoneId);
                if (rack == null)
                {
                    _logger.LogWarning("No rack found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperRackDto(rack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rack for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public IEnumerable<Rack?> GetRacksByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetRacksByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<Rack>(); // Returns null in case of error
            }
        }

        public IEnumerable<Rack>? GetRacksListByRackDtoList(IEnumerable<RackDto> rackDtos)
        {
            try
            {
                List<Rack> routeList = new();
                foreach (var routeDto in rackDtos)
                {
                    // Get the Rack from the database
                    Rack? route = _dataAccess.GetRackByIdNoTracking(routeDto.Id ?? Guid.Empty);

                    // If Rack is null, return null
                    if (route == null)
                    {
                        _logger.LogWarning($"No Rack found with the Id: {routeDto.Id}");
                        return null;
                    }

                    routeList.Add(route);
                }
                return routeList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Rack from the database.");
                return null; // Returns null in case of error
            }
        }

        public Rack? GetRackByIdAsNoTracking(Guid rackId)
        {
            try
            {
                // Get the Rack from the database
                Rack? route = _dataAccess.GetRackWithZoneByRackIdAsNoTracking(rackId);
                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Rack from the database.");
                return null; // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's Racks
        private Rack MapperRack(RackDto dto, Zone zone)
        {
            // Map the DTO to the Rack entity.
            return new Rack
            {
                Id = dto.Id ?? Guid.Empty,
                ZoneId = dto.ZoneId,
                Zone = zone,
                QuantityHeigth = dto.QuantityHeigth,
                QuantityWidth = dto.QuantityWidth,
                IsVertical = dto.IsVertical,
                NumShelves = dto.NumShelves,
                NumCrossAisles = dto.NumCrossAisles,
                Bidirectional = dto.Bidirectional,
                MaxPickers = dto.MaxPickers,
                NarrowAisle = dto.NarrowAisle,
                ViewPort = JsonSerializer.Serialize(dto)
            };
        }


        private Rack MapperUpdateRack(RackDto rackDto)
        {
            // 1. Validation: an ID is required to update
            if (rackDto.Id == Guid.Empty)
                throw new InvalidOperationException("The rack cannot be updated without a valid ID.");

            // 2. Get existing Rack from data access layer
            Rack? existingRack = _dataAccess.GetRackByIdNoTracking(rackDto.Id ?? Guid.Empty);
            if (existingRack == null)
                throw new InvalidOperationException($"The rack with the ID {rackDto.Id} was not found.");

            // 3. Assign properties from the DTO
            rackDto.StatusObject = "StoredInBase";
            rackDto.AreaId = rackDto.Zone.AreaId;

            return new Rack
            {
                Id = rackDto.Id ?? Guid.Empty,
                QuantityHeigth = rackDto.QuantityHeigth,
                QuantityWidth = rackDto.QuantityWidth,
                NumCrossAisles = rackDto.NumCrossAisles,
                NumShelves = rackDto.NumShelves,
                IsVertical = rackDto.IsVertical,
                Bidirectional =  rackDto.Bidirectional,
                MaxPickers = rackDto.MaxPickers,
                NarrowAisle =  rackDto.NarrowAisle,
                ZoneId = rackDto.ZoneId,
                Zone = existingRack.Zone,
                ViewPort = JsonSerializer.Serialize(rackDto)
            };
        }

        private RackDto MapperRackDto(Rack rack)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(rack.ZoneId);

            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {rack.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            // Map the Rack entity to the DTO.
            return new RackDto
            {
                Id = rack.Id,
                CanvasObjectType = "Rack",
                ZoneId = rack.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                AreaId = areaDto.Id ?? Guid.Empty,
                ViewPort = rack.ViewPort,
                QuantityHeigth = rack.QuantityHeigth ?? 0,
                QuantityWidth = rack.QuantityWidth ?? 0,
                NumShelves = rack.NumShelves ?? 0,
                NumCrossAisles = rack.NumCrossAisles ?? 0,
                IsVertical = rack.IsVertical,
                X = (float)(zone.xInit??0),
                Y = (float)(zone.yInit??0),
                Height = (float)(zone.yEnd??0),
                Width = (float)(zone.xEnd??0),
                Bidirectional = rack.Bidirectional ?? false,
                MaxPickers = rack.MaxPickers ?? 0,
                NarrowAisle = rack.NarrowAisle ?? false,
            };
        }

        private ShelfDto MapperRackToShelfDto(Rack rack)
        {
            RackDto rackDto = JsonSerializer.Deserialize<RackDto>(rack.ViewPort);

            // Map the Rack entity to the DTO.
            return new ShelfDto
            {
                Id = rackDto.Id,
                CanvasObjectType = rackDto.CanvasObjectType,
                Name = rackDto.Name,
                ZoneId = rackDto.ZoneId,
                NumShelves = rackDto.NumShelves,
                NumCrossAisles = rackDto.NumCrossAisles,
                IsVertical = rackDto.IsVertical,
                X = (float)(rackDto.X),
                Y = (float)(rackDto.Y),
                Height = (float)(rackDto.Height),
                Width = (float)(rackDto.Width),
                LocationTypes = rackDto.LocationTypes,
                StatusObject = rackDto.StatusObject,
                AreaId = rack.Zone.AreaId
            };
        }
        private ShelfDto MapperChaoticDtoToShelfDto(ChaoticStorage chaotic)
        {
            ChaoticStorageDto chaoticStorageDto = JsonSerializer.Deserialize<ChaoticStorageDto>(chaotic.ViewPort);

            // Map the Rack entity to the DTO.
            return new ShelfDto
            {
                Id = chaotic.Id,
                CanvasObjectType = "ChaoticStorage",
                Name = chaotic.Zone.Name,
                ZoneId = chaotic.ZoneId,
                X = (float)(chaotic.Zone.xInit ?? 0),
                Y = (float)(chaotic.Zone.yInit ?? 0),
                Height = (float)(chaotic.Zone.yEnd ?? 0),
                Width = (float)(chaotic.Zone.xEnd ?? 0),
                LocationTypes = LocationTypes.CaoticStorage,
                NumShelves = 0,
                NumCrossAisles = 0,
                StatusObject = chaoticStorageDto.StatusObject,
                AreaId = chaotic.Zone.AreaId
            };
        }

        private ShelfDto MapperRackDtoToShelfDto(RackDto rack)
        {
            RackDto rackDto = JsonSerializer.Deserialize<RackDto>(rack.ViewPort);
            // Map the Rack entity to the DTO.
            return new ShelfDto
            {
                Id = rack.Id,
                CanvasObjectType = "Rack",
                Name = rack.Zone.Name,
                ZoneId = rack.ZoneId,
                NumShelves = rack.NumShelves,
                NumCrossAisles = rack.NumCrossAisles,
                IsVertical = rack.IsVertical,
                X = (float)(rack.Zone.X),
                Y = (float)(rack.Zone.Y),
                Height = (float)(rack.Zone.Height),
                Width = (float)(rack.Zone.Width),
                LocationTypes = LocationTypes.Rack,
                StatusObject = rackDto.StatusObject,
                AreaId = rack.Zone.AreaId
            };
        }

        private ShelfDto MapperDriveInDtoToShelfDto(DriveInDto driveIn)
        {
            DriveInDto driveInDto = JsonSerializer.Deserialize<DriveInDto>(driveIn.ViewPort);
            // Map the DriveIn entity to the DTO.
            return new ShelfDto
            {
                Id = driveIn.Id,
                CanvasObjectType = "DriveIn",
                Name = driveIn.Zone.Name,
                ZoneId = driveIn.ZoneId,
                NumShelves = driveInDto?.NumShelves,
                NumCrossAisles = driveInDto?.NumCrossAisles,
                IsVertical = driveIn.IsVertical,
                X = (float)(driveIn.Zone.X),
                Y = (float)(driveIn.Zone.Y),
                Height = (float)(driveIn.Zone.Height),
                Width = (float)(driveIn.Zone.Width),
                LocationTypes = LocationTypes.DriveIn,
                StatusObject = driveInDto.StatusObject,
                AreaId = driveIn.Zone.AreaId
            };
        }

        private ShelfDto MapperChaoticStorageDtoToShelfDto(ChaoticStorageDto chaoticStorage)
        {
            ChaoticStorageDto chaoticStorageDto = JsonSerializer.Deserialize<ChaoticStorageDto>(chaoticStorage.ViewPort);
            // Map the ChaoticStorage entity to the DTO.
            return new ShelfDto
            {
                Id = chaoticStorage.Id,
                CanvasObjectType = "ChaoticStorage",
                Name = chaoticStorage.Zone.Name,
                ZoneId = chaoticStorage.ZoneId,
                X = (float)(chaoticStorage.Zone.X),
                Y = (float)(chaoticStorage.Zone.Y),
                Height = (float)(chaoticStorage.Zone.Height),
                Width = (float)(chaoticStorage.Zone.Width),
                LocationTypes = LocationTypes.CaoticStorage,
                StatusObject = chaoticStorageDto.StatusObject,
                AreaId = chaoticStorage.Zone.AreaId
            };
        }

        private ShelfDto MapperAutomaticStorageDtoToShelfDto(AutomaticStorageDto automaticStorage)
        {
            AutomaticStorageDto automaticStorageDto = JsonSerializer.Deserialize<AutomaticStorageDto>(automaticStorage.ViewPort);
            // Map the AutomaticStorage entity to the DTO.
            return new ShelfDto
            {
                Id = automaticStorage.Id,
                CanvasObjectType = "AutomaticStorage",
                Name = automaticStorage.Zone.Name,
                ZoneId = automaticStorage.ZoneId,
                NumShelves = automaticStorage.NumShelves,
                NumCrossAisles = automaticStorage.NumCrossAisles,
                IsVertical = automaticStorage.IsVertical,
                X = (float)(automaticStorage.Zone.X),
                Y = (float)(automaticStorage.Zone.Y),
                Height = (float)(automaticStorage.Zone.Height),
                Width = (float)(automaticStorage.Zone.Width),
                LocationTypes = LocationTypes.AutomaticStorage,
                StatusObject = automaticStorageDto.StatusObject,
                AreaId = automaticStorage.Zone.AreaId
            };
        }

        #endregion

        #region DriveIn Implementation

        public async Task<Guid> AddDriveInList(List<DriveInDto> driveInDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB();
                Guid newArea = Guid.Empty;

                // Validate that the input list is not null or empty
                if (driveInDtoList == null || driveInDtoList.Count == 0)
                    throw new ArgumentException("The driveIn list is empty.", nameof(driveInDtoList));

                if (isCreateNewArea)
                {
                    AreaDto areaDto = driveInDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found."); // Extract the common AreaDto from the first DockDto's Zone
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }

                CalculateName(driveInDtoList, dto => dto.Zone!);

                foreach (var driveInDto in driveInDtoList)
                {
                    Zone zone = MapperAddZone(driveInDto.Zone ?? new(), isCreateNewArea); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                    if (zone == null)
                        throw new InvalidOperationException("The zone could not be mapped correctly.");

                    var driveIn = MapperDriveIn(driveInDto, zone);
                    if (driveIn == null)
                        throw new InvalidOperationException("The DriveIn could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;
                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("DriveIns", driveIn);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("DriveIns added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding DriveIn list.");
                throw;
            }
        }

        public async Task AddDriveIn(DriveInDto driveInDto)
        {
            try
            {
                operationDB = new OperationDB();

                // Validate that the input list is not null or empty
                if (driveInDto == null)
                    throw new ArgumentException("The driveIn list is empty.", nameof(driveInDto));

                Zone zone = MapperAddZone(driveInDto.Zone ?? new(), false); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                if (zone == null)
                    throw new InvalidOperationException("The zone could not be mapped correctly.");

                var driveIn = MapperDriveIn(driveInDto, zone);
                if (driveIn == null)
                    throw new InvalidOperationException("The DriveIn could not be mapped correctly.");

                operationDB.AddNew("Zones", zone);
                operationDB.AddNew("DriveIns", driveIn);
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("DriveIns added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding DriveIn list.");
                throw;
            }
        }

        public async Task UpdateDriveIn(DriveInDto driveInDto)
        {
            try
            {
                operationDB = new OperationDB();

                Zone zone = MapperUpdateZone(driveInDto.Zone);

                //var zoneToStore = _dataAccess.GetZoneByIdNoTracking(driveInDto.ZoneId);
                if (zone == null)
                    throw new InvalidOperationException("The DriveIn could not be mapped correctly.");

                DriveIn driveIn = MapperUpdateDriveIn(driveInDto);
                if (driveIn == null)
                    throw new InvalidOperationException("The DriveIn could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("DriveIns", driveIn);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("DriveIn updated successfully: {DriveInId}", driveIn.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DriveIn. DTO: {@driveInDto}", driveInDto);
                throw;
            }
        }

        public async Task DeleteDriveIn(DriveIn driveIn)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", driveIn.Zone);
                operationDB.AddDelete("DriveIns", driveIn);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("DriveIn deleted successfully: {DriveInId}", driveIn.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting DriveIn. DTO: {@driveInDto}", driveIn);
                throw;
            }
        }

        public async Task DeleteListDriveIn(IEnumerable<DriveIn> driveInList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in driveInList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);

                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("DriveIns", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of DriveIns deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of DriveIns. DTO List: {@driveInDtoList}", driveInList);
                throw;
            }
        }

        public IEnumerable<DriveInDto> GetDriveInsDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var driveIns = _dataAccess.GetDriveInsWithZoneByAreaIdAsNoTracking(areaId);
                return driveIns.Select(MapperDriveInDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DriveIns for AreaId: {areaId}", areaId);
                return Enumerable.Empty<DriveInDto>();
            }
        }

        public DriveInDto? GetDriveInDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var driveIn = _dataAccess.GetDriveInWithZoneByDriveInIdAsNoTracking(itemId);
                if (driveIn == null)
                {
                    _logger.LogWarning("No DriveIn found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperDriveInDto(driveIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DriveIn with Id: {itemId}", itemId);
                return null;
            }
        }

        public DriveInDto GetDriveInDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var driveIn = _dataAccess.GetDriveInWithZoneByZoneIdAsNoTracking(zoneId);
                if (driveIn == null)
                {
                    _logger.LogWarning("No driveIn found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperDriveInDto(driveIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driveIn for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public IEnumerable<DriveIn?> GetDriveInsByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetDriveInsByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<DriveIn>(); // Returns null in case of error
            }
        }

        public DriveIn? GetDriveInByIdAsNoTracking(Guid driveInId)
        {
            try
            {
                // Get the DriveIn from the database
                DriveIn? route = _dataAccess.GetDriveInWithZoneByDriveInIdAsNoTracking(driveInId);
                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the DriveIn from the database.");
                return null; // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's DriveIn

        private DriveIn MapperDriveIn(DriveInDto dto, Zone zone)
        {
            // Map the DTO to the DriveIn entity.
            return new DriveIn
            {
                Id = dto.Id ?? Guid.Empty,
                ZoneId = dto.ZoneId,
                Zone = zone,
                IsVertical = dto.IsVertical,
                NumShelves = dto.NumShelves,
                NumCrossAisles = dto.NumCrossAisles,
                Bidirectional = dto.Bidirectional,
                NarrowAisle = dto.NarrowAisle,
                MaxPickers = dto.MaxPickers,
                ViewPort = JsonSerializer.Serialize(dto)
            };
        }

        private ShelfDto MapperDriveInToShelfDto(DriveIn driveIn)
        {
            DriveInDto driveInDto = JsonSerializer.Deserialize<DriveInDto>(driveIn.ViewPort);

            // Map the DriveIn entity to the DTO.
            return new ShelfDto
            {
                Id = driveIn.Id,
                CanvasObjectType = "DriveIn",
                Name = driveIn.Zone.Name,
                ZoneId = driveIn.ZoneId,
                NumShelves = driveIn.NumShelves ?? 0,
                NumCrossAisles = driveIn.NumCrossAisles ?? 0,
                IsVertical = driveIn.IsVertical,
                X = (float)(driveIn.Zone.xInit ?? 0),
                Y = (float)(driveIn.Zone.yInit ?? 0),
                Height = (float)(driveIn.Zone.yEnd ?? 0),
                Width = (float)(driveIn.Zone.xEnd ?? 0),
                LocationTypes = LocationTypes.DriveIn,
                StatusObject = driveInDto.StatusObject,
                AreaId = driveIn.Zone.AreaId
            };
        }

        private DriveIn MapperUpdateDriveIn(DriveInDto driveInDto)
        {
            // 1. Validation: an ID is required to update
            if (driveInDto.Id == Guid.Empty)
                throw new InvalidOperationException("The driveIn cannot be updated without a valid ID.");

            // 2. Get existing DriveIn from data access layer
            DriveIn? existingDriveIn = _dataAccess.GetDriveInByIdNoTracking(driveInDto.Id ?? Guid.Empty);
            if (existingDriveIn == null)
                throw new InvalidOperationException($"The driveIn with the ID {driveInDto.Id} was not found.");

            // 3. Assign properties from the DTO
            driveInDto.StatusObject = "StoredInBase";
            driveInDto.AreaId = driveInDto.Zone.AreaId;

            return new DriveIn
            {
                Id = driveInDto.Id ?? Guid.Empty,
                NumCrossAisles = driveInDto.NumCrossAisles,
                NumShelves = driveInDto.NumShelves,
                IsVertical = driveInDto.IsVertical,
                Bidirectional = driveInDto.Bidirectional,
                MaxPickers = driveInDto.MaxPickers,
                NarrowAisle = driveInDto.NarrowAisle,
                ZoneId = driveInDto.ZoneId,
                Zone = existingDriveIn.Zone,
                ViewPort = JsonSerializer.Serialize(driveInDto)
            };
        }

        private DriveInDto MapperDriveInDto(DriveIn driveIn)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(driveIn.ZoneId);

            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {driveIn.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            // Map the DriveIn entity to the DTO.
            return new DriveInDto
            {
                Id = driveIn.Id,
                CanvasObjectType = "DriveIn",
                ZoneId = driveIn.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                AreaId = areaDto.Id ?? Guid.Empty,
                ViewPort = driveIn.ViewPort,
                NumShelves = driveIn.NumShelves ?? 0,
                NumCrossAisles = driveIn.NumCrossAisles ?? 0,
                IsVertical = driveIn.IsVertical,
                X = (float)(zone.xInit ?? 0),
                Y = (float)(zone.yInit ?? 0),
                Height = (float)(zone.yEnd ?? 0),
                Width = (float)(zone.xEnd ?? 0),
                Bidirectional = driveIn.Bidirectional ?? false,
                MaxPickers = driveIn.MaxPickers ?? 0,
                NarrowAisle = driveIn.NarrowAisle ?? false,
            };
        }

        #endregion

        #region AutomaticStorage Implementation

        public async Task<Guid> AddAutomaticStorageList(List<AutomaticStorageDto> automaticStorageDtoList, bool isCreateNewArea)
        {
            try
            {
                operationDB = new OperationDB();
                Guid newArea = Guid.Empty;

                // Validate that the input list is not null or empty
                if (automaticStorageDtoList == null || automaticStorageDtoList.Count == 0)
                    throw new ArgumentException("The automaticStorage list is empty.", nameof(automaticStorageDtoList));

                if (isCreateNewArea)
                {
                    AreaDto areaDto = automaticStorageDtoList[0].Zone?.Area ?? throw new InvalidOperationException("The area could not be found."); // Extract the common AreaDto from the first DockDto's Zone
                    Area area = MapperArea(areaDto); // Map the DTO to the domain Area object
                    newArea = area.Id;
                    operationDB.AddNew("Areas", area); // Add the area only once to the operation batch
                }

                CalculateName(automaticStorageDtoList, dto => dto.Zone!);

                foreach (var automaticStorageDto in automaticStorageDtoList)
                {
                    Zone zone = MapperAddZone(automaticStorageDto.Zone ?? new(), isCreateNewArea); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                    if (zone == null)
                        throw new InvalidOperationException("The zone could not be mapped correctly.");

                    var automaticStorage = MapperAutomaticStorage(automaticStorageDto, zone);
                    if (automaticStorage == null)
                        throw new InvalidOperationException("The AutomaticStorage could not be mapped correctly.");

                    if (newArea == Guid.Empty)
                        newArea = zone.AreaId;
                    operationDB.AddNew("Zones", zone);
                    operationDB.AddNew("AutomaticStorages", automaticStorage);
                }
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("AutomaticStorages added successfully");
                return newArea;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding AutomaticStorage list.");
                throw;
            }
        }

        public async Task AddAutomaticStorage(AutomaticStorageDto automaticStorageDto)
        {
            try
            {
                operationDB = new OperationDB();

                // Validate that the input list is not null or empty
                if (automaticStorageDto == null)
                    throw new ArgumentException("The automaticStorage list is empty.", nameof(automaticStorageDto));

                Zone zone = MapperAddZone(automaticStorageDto.Zone ?? new(), false); // Map the Zone from DTO; use in-memory counter when haveAreaData is true;;
                if (zone == null)
                    throw new InvalidOperationException("The zone could not be mapped correctly.");

                var automaticStorage = MapperAutomaticStorage(automaticStorageDto, zone);
                if (automaticStorage == null)
                    throw new InvalidOperationException("The AutomaticStorage could not be mapped correctly.");

                operationDB.AddNew("Zones", zone);
                operationDB.AddNew("AutomaticStorages", automaticStorage);
                _zoneCounters.Clear();
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("AutomaticStorages added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding AutomaticStorage list.");
                throw;
            }
        }

        public async Task UpdateAutomaticStorage(AutomaticStorageDto automaticStorageDto)
        {
            try
            {
                operationDB = new OperationDB();

                Zone zone = MapperUpdateZone(automaticStorageDto.Zone);
                if (zone == null)
                    throw new InvalidOperationException("The AutomaticStorage could not be mapped correctly.");

                AutomaticStorage automaticStorage = MapperUpdateAutomaticStorage(automaticStorageDto);
                if (automaticStorage == null)
                    throw new InvalidOperationException("The AutomaticStorage could not be mapped correctly.");

                operationDB.AddUpdate("Zones", zone);
                operationDB.AddUpdate("AutomaticStorages", automaticStorage);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("AutomaticStorage updated successfully: {AutomaticStorageId}", automaticStorage.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AutomaticStorage. DTO: {@automaticStorageDto}", automaticStorageDto);
                throw;
            }
        }

        public async Task DeleteAutomaticStorage(AutomaticStorage automaticStorage)
        {
            try
            {
                operationDB = new OperationDB();

                operationDB.AddDelete("Zones", automaticStorage.Zone);
                operationDB.AddDelete("AutomaticStorages", automaticStorage);

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("AutomaticStorage deleted successfully: {AutomaticStorageId}", automaticStorage.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AutomaticStorage. DTO: {@automaticStorageDto}", automaticStorage);
                throw;
            }
        }

        public async Task DeleteListAutomaticStorage(IEnumerable<AutomaticStorage> automaticStorageList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in automaticStorageList)
                {
                    var zone = _dataAccess.GetZoneByIdNoTracking(item.ZoneId);

                    if (null == zone)
                        _logger.LogWarning($"No zone found with the Id: {item}");

                    Area? area = _dataAccess.GetAreaByAreaIdWithAlternativeAreaAsNoTracking(zone.AreaId);
                    if (null == area)
                        _logger.LogWarning($"No area found with the Id: {item}");

                    item.Zone = zone;
                    item.Zone.Area = area;

                    operationDB.AddDelete("Zones", item.Zone);
                    operationDB.AddDelete("AutomaticStorages", item);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of AutomaticStorages deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of AutomaticStorages. DTO List: {@automaticStorageDtoList}", automaticStorageList);
                throw;
            }
        }

        public IEnumerable<AutomaticStorageDto> GetAutomaticStoragesDtoByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                var automaticStorages = _dataAccess.GetAutomaticStoragesWithZoneByAreaIdAsNoTracking(areaId);
                return automaticStorages.Select(MapperAutomaticStorageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AutomaticStorages for AreaId: {areaId}", areaId);
                return Enumerable.Empty<AutomaticStorageDto>();
            }
        }

        public AutomaticStorageDto? GetAutomaticStorageDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                var automaticStorage = _dataAccess.GetAutomaticStorageWithZoneByAutomaticStorageIdAsNoTracking(itemId);
                if (automaticStorage == null)
                {
                    _logger.LogWarning("No AutomaticStorage found with Id: {itemId}", itemId);
                    return null;
                }
                return MapperAutomaticStorageDto(automaticStorage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AutomaticStorage with Id: {itemId}", itemId);
                return null;
            }
        }

        public AutomaticStorageDto GetAutomaticStorageDtoByIdZoneAsNoTracking(Guid zoneId)
        {
            try
            {
                var automaticStorage = _dataAccess.GetAutomaticStoragesWithZoneByZoneIdAsNoTracking(zoneId);
                if (automaticStorage == null)
                {
                    _logger.LogWarning("No automaticStorage found for ZoneId: {zoneId}", zoneId);
                    return null;
                }
                return MapperAutomaticStorageDto(automaticStorage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving automaticStorage for ZoneId: {zoneId}", zoneId);
                return null;
            }
        }

        public IEnumerable<AutomaticStorage?> GetAutomaticStoragesByIdAreaNoTracking(Guid areaId)
        {
            try
            {
                return _dataAccess.GetAutomaticStoragesByAreaNoTracking(areaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the aisle from the database.");
                return Enumerable.Empty<AutomaticStorage>(); // Returns null in case of error
            }
        }

        public AutomaticStorage? GetAutomaticStorageByIdAsNoTracking(Guid automaticStorageId)
        {
            try
            {
                // Get the AutomaticStorage from the database
                AutomaticStorage? route = _dataAccess.GetAutomaticStorageWithZoneByAutomaticStorageIdAsNoTracking(automaticStorageId);
                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the AutomaticStorage from the database.");
                return null; // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's AutomaticStorage

        private AutomaticStorage MapperAutomaticStorage(AutomaticStorageDto dto, Zone zone)
        {
            // Map the DTO to the AutomaticStorage entity.
            return new AutomaticStorage
            {
                Id = dto.Id ?? Guid.Empty,
                ZoneId = dto.ZoneId,
                Zone = zone,
                IsVertical = dto.IsVertical,
                NumShelves = dto.NumShelves,
                NumCrossAisles = dto.NumCrossAisles,
                ViewPort = JsonSerializer.Serialize(dto)
            };
        }

        private ShelfDto MapperAutomaticStorageToShelfDto(AutomaticStorage automaticStorage)
        {
            AutomaticStorageDto automaticStorageDto = JsonSerializer.Deserialize<AutomaticStorageDto>(automaticStorage.ViewPort);

            // Map the AutomaticStorage entity to the DTO.
            return new ShelfDto
            {
                Id = automaticStorage.Id,
                CanvasObjectType = "AutomaticStorage",
                Name = automaticStorage.Zone.Name,
                ZoneId = automaticStorage.ZoneId,
                NumShelves = automaticStorage.NumShelves ?? 0,
                NumCrossAisles = automaticStorage.NumCrossAisles ?? 0,
                IsVertical = automaticStorage.IsVertical,
                X = (float)(automaticStorage.Zone.xInit ?? 0),
                Y = (float)(automaticStorage.Zone.yInit ?? 0),
                Height = (float)(automaticStorage.Zone.yEnd ?? 0),
                Width = (float)(automaticStorage.Zone.xEnd ?? 0),
                LocationTypes = LocationTypes.AutomaticStorage,
                StatusObject = automaticStorageDto.StatusObject,
                AreaId = automaticStorage.Zone.AreaId
            };
        }

        private AutomaticStorage MapperUpdateAutomaticStorage(AutomaticStorageDto automaticStorageDto)
        {
            // 1. Validation: an ID is required to update
            if (automaticStorageDto.Id == Guid.Empty)
                throw new InvalidOperationException("The automaticStorage cannot be updated without a valid ID.");

            // 2. Get existing AutomaticStorage from data access layer
            AutomaticStorage? existingAutomaticStorage = _dataAccess.GetAutomaticStorageByIdNoTracking(automaticStorageDto.Id ?? Guid.Empty);
            if (existingAutomaticStorage == null)
                throw new InvalidOperationException($"The automaticStorage with the ID {automaticStorageDto.Id} was not found.");

            // 3. Assign properties from the DTO
            automaticStorageDto.StatusObject = "StoredInBase";
            automaticStorageDto.AreaId = automaticStorageDto.Zone.AreaId;

            return new AutomaticStorage
            {
                Id = automaticStorageDto.Id ?? Guid.Empty,
                NumCrossAisles = automaticStorageDto.NumCrossAisles,
                NumShelves = automaticStorageDto.NumShelves,
                IsVertical = automaticStorageDto.IsVertical,
                ZoneId = automaticStorageDto.ZoneId,
                Zone = existingAutomaticStorage.Zone,
                ViewPort = JsonSerializer.Serialize(automaticStorageDto)
            };
        }

        private AutomaticStorageDto MapperAutomaticStorageDto(AutomaticStorage automaticStorage)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(automaticStorage.ZoneId);

            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {automaticStorage.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            // Map the AutomaticStorage entity to the DTO.
            return new AutomaticStorageDto
            {
                Id = automaticStorage.Id,
                CanvasObjectType = "AutomaticStorage",
                ZoneId = automaticStorage.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                ViewPort = automaticStorage.ViewPort,
                NumShelves = automaticStorage.NumShelves ?? 0,
                NumCrossAisles = automaticStorage.NumCrossAisles ?? 0,
                IsVertical = automaticStorage.IsVertical,
                X = (float)(zone.xInit ?? 0),
                Y = (float)(zone.yInit ?? 0),
                Height = (float)(zone.yEnd ?? 0),
                Width = (float)(zone.xEnd ?? 0),
            };
        }

        private ShelfDto MapperAutomaticStorageDtoToShelfDto(AutomaticStorage automaticStorage)
        {
            AutomaticStorageDto automaticStorageDto = JsonSerializer.Deserialize<AutomaticStorageDto>(automaticStorage.ViewPort);
            // Map the AutomaticStorage entity to the DTO.
            return new ShelfDto
            {
                Id = automaticStorage.Id,
                CanvasObjectType = "AutomaticStorage",
                Name = automaticStorage.Zone.Name,
                ZoneId = automaticStorage.ZoneId,
                X = (float)(automaticStorage.Zone.xInit ?? 0),
                Y = (float)(automaticStorage.Zone.yInit ?? 0),
                Height = (float)(automaticStorage.Zone.yEnd ?? 0),
                Width = (float)(automaticStorage.Zone.xEnd ?? 0),
                NumShelves = automaticStorage.NumShelves ?? 0,
                NumCrossAisles = automaticStorage.NumCrossAisles ?? 0,
                IsVertical = automaticStorage.IsVertical,
                LocationTypes = LocationTypes.AutomaticStorage,
                StatusObject = automaticStorageDto.StatusObject,
                AreaId = automaticStorage.Zone.AreaId
            };
        }

        #endregion
        #region Mapper's Chaotic
        private ChaoticStorage MapperChaotic(ChaoticStorageDto dto, Zone zone)
        {
            return new ChaoticStorage
            {
                Id = dto.Id ?? Guid.Empty,
                ZoneId = dto.ZoneId,
                Zone = zone,
                ViewPort = JsonSerializer.Serialize(dto)
            };
        }

        private ChaoticStorage MapperUpdateChaotic(ChaoticStorageDto chaoticStorageDto)
        {
            // 1. Validation: an ID is required to update
            if (chaoticStorageDto.Id == Guid.Empty)
                throw new InvalidOperationException("The rack cannot be updated without a valid ID.");

            // 2. Get existing Rack from data access layer
            ChaoticStorage? existingChaotic = _dataAccess.GetChaoticByIdNoTracking(chaoticStorageDto.Id ?? Guid.Empty);
            if (existingChaotic == null)
                throw new InvalidOperationException($"The rack with the ID {chaoticStorageDto.Id} was not found.");

            // 3. Assign properties from the DTO
            chaoticStorageDto.StatusObject = "StoredInBase";
            chaoticStorageDto.AreaId = chaoticStorageDto.Zone.AreaId;

            return new ChaoticStorage
            {
                Id = chaoticStorageDto.Id ?? Guid.Empty,
                ZoneId = chaoticStorageDto.ZoneId,
                Zone = existingChaotic.Zone,
                ViewPort = JsonSerializer.Serialize(chaoticStorageDto)
            };
        }
        #endregion
        #region Mapper's ChaoticStorage
        private ChaoticStorageDto MapperChaoticStationDto(ChaoticStorage chaoticStation)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(chaoticStation.ZoneId);

            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {chaoticStation.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            // Map the ChaoticStation entity to the DTO.
            return new ChaoticStorageDto
            {
                Id = chaoticStation.Id,
                CanvasObjectType = "ChaoticStation",
                ZoneId = chaoticStation.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                ViewPort = chaoticStation.ViewPort,
                X = (float)(zone.xInit ?? 0),
                Y = (float)(zone.yInit ?? 0),
                Height = (float)(zone.yEnd ?? 0),
                Width = (float)(zone.xEnd ?? 0),
            };
        }

        #endregion

        #region Mapper's AutomaticStorage

        private AutomaticStorageDto MapperAutomaticDto(AutomaticStorage automatic)
        {
            Zone? zone = _dataAccess.GetZoneWithAreaByIdAsNoTracking(automatic.ZoneId);

            if (zone == null)
                throw new InvalidOperationException($"No zone with ID was found {automatic.ZoneId}.");

            if (zone.Area == null)
                throw new InvalidOperationException($"No area with ID was found {zone.AreaId}.");

            AreaDto? areaDto = MapperAreaDto(zone.Area);

            // Map the Automatic entity to the DTO.
            return new AutomaticStorageDto
            {
                Id = automatic.Id,
                CanvasObjectType = "Automatic",
                ZoneId = automatic.ZoneId,
                Zone = Models.DTO.Designer.ZoneDto.GetZoneDTO(zone, areaDto),
                ViewPort = automatic.ViewPort,
                NumShelves = automatic.NumShelves ?? 0,
                NumCrossAisles = automatic.NumCrossAisles ?? 0,
                IsVertical = automatic.IsVertical,
                X = (float)(zone.xInit ?? 0),
                Y = (float)(zone.yInit ?? 0),
                Height = (float)(zone.yEnd ?? 0),
                Width = (float)(zone.xEnd ?? 0),
            };
        }

        #endregion

        #region ProcessDirectionProperty
        public async Task AddProcessDirectionProperty(ProcessDirectionPropertyDto itemDto)
        {
            try
            {
                // Inicializar operación en la base de datos
                operationDB = new OperationDB();

                // Mapear el DTO al modelo de datos (usando tu mapper con validación)
                ProcessDirectionProperty? processDirection = MapperProcessDirectionProperty(itemDto);

                if (processDirection == null)
                    throw new InvalidOperationException("The ProcessDirectionProperty could not be mapped correctly.");

                // Agregar el nuevo ProcessDirectionProperty a la operación
                operationDB.AddNew("ProcessDirectionProperties", processDirection);

                // Guardar los cambios en base de datos
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("ProcessDirectionProperty added successfully: {Name}", itemDto.Name);
            }
            catch (Exception ex)
            {
                // Log del error con el contenido del DTO
                _logger.LogError(ex, "Error adding the ProcessDirectionProperty. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateProcessDirectionProperty(ProcessDirectionPropertyDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();

                ProcessDirectionProperty? processDirection = MapperProcessDirectionProperty(itemDto);

                if (processDirection == null)
                    throw new InvalidOperationException("The ProcessDirectionPropertyDto could not be mapped correctly.");

                operationDB.AddUpdate("ProcessDirectionProperties", processDirection);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("ProcessDirectionProperty updated successfully: {Name}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating the ProcessDirectionProperty. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateListProcessDirectionProperties(IEnumerable<ProcessDirectionPropertyDto> itemListDto)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (var itemDto in itemListDto)
                {
                    ProcessDirectionProperty? processDirection = MapperProcessDirectionProperty(itemDto);
                    if (processDirection == null)
                        throw new InvalidOperationException("The ProcessDirectionPropertyDto could not be mapped correctly.");

                    operationDB.AddUpdate("ProcessDirectionProperties", processDirection);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("List of ProcessDirectionProperties updated successfully. Count: {Count}", itemListDto.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating list of ProcessDirectionProperties. Count: {Count}", itemListDto.Count());
                throw;
            }
        }

        public async Task DeleteListProcessDirectionProperties(IEnumerable<ProcessDirectionProperty> itemList)
        {
            try
            {
                operationDB = new OperationDB();

                foreach (var itemDto in itemList)
                {
                    ProcessDirectionProperty? processDirection = _dataAccess.GetProcessDirectionPropertyByIdNoTracking(itemDto.Id);
                    if (processDirection == null)
                        throw new InvalidOperationException("The Process Direction could not be mapped correctly for deletion.");

                    operationDB.AddDelete("ProcessDirectionProperties", processDirection);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("List of Process Direction deleted successfully. Count: {Count}", itemList.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of Process Direction. Count: {Count}", itemList.Count());
                throw;
            }
        }

        public ProcessDirectionProperty? GetProcessDirectionPropertyByIdNoTracking(Guid itemId)
        {
            try
            {
                ProcessDirectionProperty? processDirection = _dataAccess.GetProcessDirectionPropertyByIdNoTracking(itemId);
                if (processDirection == null)
                {
                    _logger.LogWarning("No ProcessDirectionProperty found with the Id: {itemId}", itemId);
                    return null;
                }
                return processDirection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProcessDirectionProperty by Id: {itemId}", itemId);
                return null;
            }
        }

        public ProcessDirectionPropertyDto? GetProcessDirectionPropertyDtoByIdNoTracking(Guid itemId)
        {
            try
            {
                ProcessDirectionProperty? processDirection = _dataAccess.GetProcessDirectionPropertyByIdWithProcessAsNoTracking(itemId);
                if (processDirection == null)
                {
                    _logger.LogWarning("No ProcessDirectionProperty found with the Id: {itemId}", itemId);
                    return null;
                }

                return MapperProcessDirectionPropertyDto(processDirection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProcessDirectionProperty DTO by Id: {itemId}", itemId);
                return null;
            }
        }

        public IEnumerable<ProcessDirectionPropertyDto>? GetProcessDirectionPropertyDtoByIdLayout(Guid itemId)
        {
            try
            {
                List<ProcessDirectionProperty>? processDirections = _dataAccess.GetAllProcessDirectionPropertyByLayoutId(itemId).ToList();
                List<ProcessDirectionPropertyDto>? processDirectionsDto = new();
                foreach (var processDirection in processDirections)
                {
                    processDirectionsDto.Add(MapperProcessDirectionPropertyDto(processDirection));
                }

                if (processDirectionsDto == null)
                {
                    _logger.LogWarning("No ProcessDirectionProperty found with the Id: {itemId}", itemId);
                    return null;
                }

                return processDirectionsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProcessDirectionProperty DTO by Id: {itemId}", itemId);
                return null;
            }
        }

        public IEnumerable<ProcessDirectionProperty>? GetProcessDirectionPropertyListByProcessDirectionPropertyDtoList(IEnumerable<ProcessDirectionPropertyDto> processDirectionPropertyDtos)
        {
            try
            {
                List<ProcessDirectionProperty> processDirectionPropertyList = new();
                foreach (var processDirectionPropertyDto in processDirectionPropertyDtos)
                {
                    // Get the ProcessDirectionProperty from the database
                    ProcessDirectionProperty? processDirectionProperty = _dataAccess.GetProcessDirectionPropertyByIdNoTracking(processDirectionPropertyDto.Id);

                    // If ProcessDirectionProperty is null, return null
                    if (processDirectionProperty == null)
                    {
                        _logger.LogWarning($"No Process Direction found with the Id: {processDirectionPropertyDto.Id}");
                        return null;
                    }

                    processDirectionPropertyList.Add(processDirectionProperty);
                }
                return processDirectionPropertyList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Process Direction from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<ProcessDirectionProperty?> GetProcessDirectionsByIdProcess(Guid processId)
        {
            try
            {
                return _dataAccess.GetProcessDirectionsByIdProcessAsNoTracking(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Process Direction from the database.");
                return Enumerable.Empty<ProcessDirectionProperty>(); // Returns null in case of error
            }
        }

        public IEnumerable<ProcessDirectionPropertyDto> GetProcessDirectionsByListProcess(List<ProcessDto> processDtos)
        {
            try
            {
                List<ProcessDirectionPropertyDto> processDirectionPropertyDtoList = new();
                foreach (var processDto in processDtos)
                {
                    List<ProcessDirectionProperty>? processDirectionsPropertys = (List<ProcessDirectionProperty>)_dataAccess.GetProcessDirectionsByIdProcessAsNoTracking((Guid)processDto.Id);
                    if (processDirectionsPropertys == null)
                    {
                        _logger.LogWarning($"No Process Direcction data was found with the process id: {processDto.Id}");
                        return null;
                    }
                    foreach (var processDirectionProperty in processDirectionsPropertys)
                    {
                        processDirectionPropertyDtoList.Add(MapperProcessDirectionPropertyDto(processDirectionProperty));
                    }
                }

                return processDirectionPropertyDtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Process Direction from the database.");
                return null;
            }

        }

        public object GetProcessAndDirectionsByFlow(Guid layoutId, List<Guid> flows)
        {
            return _dataAccess.GetProcessAndDirectionsByFlow(layoutId, flows);
        }

        #endregion

        #region Mapper's ProcessDirectionProperty
        private ProcessDirectionProperty MapperProcessDirectionProperty(ProcessDirectionPropertyDto itemDto)
        {
            if (itemDto.InitProcessId == Guid.Empty || itemDto.EndProcessId == Guid.Empty)
                throw new InvalidOperationException("Both InitProcessId and EndProcessId must be provided.");


            //Process initProcess = _dataAccess.GetProcessByIdNoTracking(itemDto.InitProcessId);
            //Process endProcess = _dataAccess.GetProcessByIdNoTracking(itemDto.EndProcessId);

            //if (initProcess == null)
            //    throw new InvalidOperationException($"No InitProcess found with ID {itemDto.InitProcessId}.");

            //if (endProcess == null)
            //    throw new InvalidOperationException($"No EndProcess found with ID {itemDto.EndProcessId}.");

            return new ProcessDirectionProperty
            {
                Id = itemDto.Id,
                Name = itemDto.Name ?? string.Empty,
                Percentage = itemDto.Percentage,
                InitProcessId = itemDto.InitProcessId,
                InitProcess = null,
                EndProcessId = itemDto.EndProcessId,
                EndProcess = null,
                IsEnd = itemDto.IsEnd,
                ViewPort = itemDto.ViewPort
            };
        }

        private ProcessDirectionPropertyDto MapperProcessDirectionPropertyDto(ProcessDirectionProperty item)
        {
            if (item.InitProcess == null || item.EndProcess == null)
                throw new InvalidOperationException("Both InitProcess and EndProcess must be loaded.");

            ProcessDto initProcessDto = MapperProcessDto(item.InitProcess);
            ProcessDto endProcessDto = MapperProcessDto(item.EndProcess);

            return new ProcessDirectionPropertyDto
            {
                Id = item.Id,
                Name = item.Name,
                Percentage = item.Percentage,
                InitProcessId = item.InitProcessId,
                InitProcessIsIn = initProcessDto.IsIn,
                InitProcessIsOut = initProcessDto.IsOut,
                InitProcess = initProcessDto,
                EndProcessId = item.EndProcessId,
                EndProcess = endProcessDto,
                IsEnd = item.IsEnd,
                ViewPort = item.ViewPort // The Viewport stores the coordinates of the process direction points and obtains data after making movements at the points
            };
        }

        #endregion

        #region Custom Process Implementation

        public async Task AddCustomProcess(CustomProcessDto itemDto)
        {
            try
            {
                operationDB = new();
                CustomProcess customProcess = MapperCustomeProcess(itemDto);

                if (customProcess == null)
                    throw new InvalidOperationException("The custom process could not be mapped correctly.");

                Process process = GetProcessByIdNoTracking(customProcess.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", customProcess.Process);
                else
                    operationDB.AddUpdate("Processes", customProcess.Process);

                operationDB.AddNew("CustomProcesses", customProcess);

                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("CustomProcesses added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                // Log the error with additional details
                _logger.LogError(ex, "Error adding the CustomProcesses. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateCustomProcess(CustomProcessDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                CustomProcess customProcess = MapperCustomeProcess(itemDto);

                if (customProcess == null)
                    throw new InvalidOperationException("The custom processes could not be mapped correctly.");

                operationDB.AddUpdate("CustomProcesses", customProcess);
                operationDB.AddUpdate("Processes", customProcess.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly upgraded process: {customProcess.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the process. DTO:  {itemDto.Name}");
                throw;
            }
        }

        public async Task DeleteCustomProcess(CustomProcessDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                CustomProcess customProcess = MapperCustomeProcess(itemDto);

                if (customProcess == null)
                    throw new InvalidOperationException("The custom process could not be mapped correctly.");

                operationDB.AddDelete("CustomProcesses", customProcess);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly upgraded process: {customProcess.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the process. DTO:  {itemDto.Name}");
                throw;
            }
        }

        public CustomProcessDto? GetCustomProcessById(Guid Id)
        {
            try
            {
                CustomProcess customProcess = _dataAccess.GetCustomProcessDesignerById(Id);

                if (customProcess == null)
                {
                    _logger.LogWarning($"No CustomProcess found with the Id: {Id}");
                    return null;
                }

                return new CustomProcessDto
                {
                    Id = customProcess.Id,
                    InitHour = customProcess.InitHour,
                    EndHour = customProcess.EndHour,
                    Percentage = customProcess.Percentage,
                    NumPossibleTimes = customProcess.NumPossibleTimes,
                    ProcessId = customProcess.ProcessId,
                    Process = MapperProcessDto(customProcess.Process),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the database Layout.");
                return null;
            }
        }

        public CustomProcessDto GetCustomProcessDtoByProcessId(Guid processId)
        {
            try
            {
                CustomProcess? customProcess = _dataAccess.GetCustomProcessDesignerByProcessId(processId);

                return new CustomProcessDto
                {
                    Id = customProcess.Id,
                    InitHour = customProcess.InitHour,
                    EndHour = customProcess.EndHour,
                    Percentage = customProcess.Percentage,
                    NumPossibleTimes = customProcess.NumPossibleTimes,
                    ProcessId = customProcess.ProcessId,
                    Process = MapperProcessDto(customProcess.Process),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<CustomProcess?> GetCustomProcessDtoByProcess(Guid processId)
        {
            try
            {
                return _dataAccess.GetCustomProcessDesignerByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<CustomProcess>(); // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's Custom Process
        private CustomProcess MapperCustomeProcess(CustomProcessDto customeProcesDto)
        {
            Process process = MapperProcess(customeProcesDto.Process);
            return new CustomProcess
            {
                Id = customeProcesDto.Id,
                InitHour = customeProcesDto.InitHour,
                EndHour = customeProcesDto.EndHour,
                Percentage = customeProcesDto.Percentage,
                NumPossibleTimes = customeProcesDto.NumPossibleTimes,
                ProcessId = process.Id,
                Process = process,
                ViewPort = customeProcesDto.ViewPort,
            };
        }

        #endregion

        #region Inbounds Process Implementation
        public async Task AddInbounds(InboundDto itemDto)
        {
            try
            {
                operationDB = new();
                Inbound inbound = MapperInbound(itemDto);

                if (inbound == null)
                    throw new InvalidOperationException("The inbound could not be mapped correctly.");

                Process process = GetProcessByIdNoTracking(inbound.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", inbound.Process);
                else
                    operationDB.AddUpdate("Processes", inbound.Process);

                operationDB.AddNew("Inbounds", inbound);

                // Save changes to the database
                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Inbound added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                // Log the error with additional details
                _logger.LogError(ex, "Error adding the Inbound. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public InboundDto? GetInboundsById(Guid Id)
        {
            try
            {
                Inbound inbound = _dataAccess.GetInboundDesignerById(Id);

                if (inbound == null)
                {
                    _logger.LogWarning($"No inbound found with the Id: {Id}");
                    return null;
                }

                return new InboundDto
                {
                    Id = inbound.Id,
                    Quantity = inbound.Quantity,
                    UnitOfMeasure = inbound.UnitOfMeasure,
                    VehiclePerHour = inbound.VehiclePerHour,
                    TruckPerDay = inbound.TruckPerDay,
                    MinTimeInBuffer = inbound.MinTimeInBuffer,
                    LoadTime = inbound.LoadTime,
                    AdditionalTimeInBuffer = inbound.AdditionalTimeInBuffer,
                    ProcessId = inbound.ProcessId,
                    Process = MapperProcessDto(inbound.Process),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the database Layout.");
                return null;
            }
        }

        public InboundDto GetInboundDtoByProcessId(Guid processId)
        {
            try
            {
                Inbound inbound = _dataAccess.GetInboundByProcessId(processId);
                return new InboundDto
                {
                    Id = inbound.Id,
                    Quantity = inbound.Quantity,
                    UnitOfMeasure = inbound.UnitOfMeasure,
                    VehiclePerHour = inbound.VehiclePerHour,
                    TruckPerDay = inbound.TruckPerDay,
                    MinTimeInBuffer = inbound.MinTimeInBuffer,
                    LoadTime = inbound.LoadTime,
                    AdditionalTimeInBuffer = inbound.AdditionalTimeInBuffer,
                    ProcessId = inbound.ProcessId,
                    Process = MapperProcessDto(inbound.Process),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<Inbound?> GetInboundListByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetInboundDesignerByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<Inbound>(); // Returns null in case of error
            }
        }

        public async Task UpdateInbound(InboundDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Inbound inbound = MapperInbound(itemDto);

                if (inbound == null)
                    throw new InvalidOperationException("The inbound could not be mapped correctly.");

                operationDB.AddUpdate("Inbounds", inbound);
                operationDB.AddUpdate("Processes", inbound.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated inbound for process: {inbound.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the inbound. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeleteInbound(InboundDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Inbound inbound = MapperInbound(itemDto);

                if (inbound == null)
                    throw new InvalidOperationException("The inbound could not be mapped correctly.");

                operationDB.AddDelete("Inbounds", inbound);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted inbound for process: {inbound.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the inbound. DTO: {itemDto.Name}");
                throw;
            }
        }

        #endregion

        #region Mapper's Inbounds
        private Inbound MapperInbound(InboundDto inboundDto)
        {
            Process process = MapperProcess(inboundDto.Process);
            return new Inbound
            {
                Id = (Guid)inboundDto.Id,
                Quantity = inboundDto.Quantity,
                UnitOfMeasure = inboundDto.UnitOfMeasure,
                VehiclePerHour = inboundDto.VehiclePerHour,
                TruckPerDay = inboundDto.TruckPerDay,
                MinTimeInBuffer = inboundDto.MinTimeInBuffer,
                LoadTime = inboundDto.LoadTime,
                AdditionalTimeInBuffer = inboundDto.AdditionalTimeInBuffer,
                ProcessId = inboundDto.ProcessId,
                Process = process,
            };
        }

        private InboundDto MapperInboundDto(Inbound inbound)
        {
            var processDto = MapperProcessDto(inbound.Process);
            return new InboundDto
            {
                Id = inbound.Id,
                Quantity = inbound.Quantity,
                UnitOfMeasure = inbound.UnitOfMeasure,
                VehiclePerHour = inbound.VehiclePerHour,
                TruckPerDay = inbound.TruckPerDay,
                MinTimeInBuffer = inbound.MinTimeInBuffer,
                LoadTime = inbound.LoadTime,
                AdditionalTimeInBuffer = inbound.AdditionalTimeInBuffer,
                ProcessId = inbound.ProcessId,
                Process = processDto
            };
        }

        #endregion

        #region Loading Process Implementation
        public async Task AddLoading(LoadingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Loading loading = MapperLoading(itemDto);

                if (loading == null)
                    throw new InvalidOperationException("The loading could not be mapped correctly.");

                var process = GetProcessByIdNoTracking(loading.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", loading.Process);
                else
                    operationDB.AddUpdate("Processes", loading.Process);

                operationDB.AddNew("Loadings", loading);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Loading added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the loading. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateLoading(LoadingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Loading loading = MapperLoading(itemDto);

                if (loading == null)
                    throw new InvalidOperationException("The loading could not be mapped correctly.");

                operationDB.AddUpdate("Loadings", loading);
                operationDB.AddUpdate("Processes", loading.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated loading for process: {loading.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the loading. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeleteLoading(LoadingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Loading loading = MapperLoading(itemDto);

                if (loading == null)
                    throw new InvalidOperationException("The loading could not be mapped correctly.");

                operationDB.AddDelete("Loadings", loading);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted loading for process: {loading.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the loading. DTO: {itemDto.Name}");
                throw;
            }
        }

        public LoadingDto? GetLoadingDtoByProcessId(Guid id)
        {
            try
            {
                Loading loading = _dataAccess.GetLoadingByProcessId(id);

                if (loading == null)
                {
                    _logger.LogWarning($"No loading found with the Id: {id}");
                    return null;
                }

                return new LoadingDto
                {
                    Id = loading.Id,
                    Dock = loading.Dock,
                    AutomaticLoadingTime = loading.AutomaticLoadingTime,
                    AdditionalTimeInBuffer = loading.AdditionalTimeInBuffer,
                    ProcessId = loading.ProcessId,
                    Process = MapperProcessDto(loading.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the loading.");
                return null;
            }
        }

        public LoadingDto? GetLoadingDtoById(Guid id)
        {
            try
            {
                Loading loading = _dataAccess.GetLoadingByIdWithProcess(id);

                if (loading == null)
                {
                    _logger.LogWarning($"No loading found with the Id: {id}");
                    return null;
                }

                return new LoadingDto
                {
                    Id = loading.Id,
                    Dock = loading.Dock,
                    AutomaticLoadingTime = loading.AutomaticLoadingTime,
                    AdditionalTimeInBuffer = loading.AdditionalTimeInBuffer,
                    ProcessId = loading.ProcessId,
                    Process = MapperProcessDto(loading.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the loading.");
                return null;
            }
        }

        public IEnumerable<Loading?> GetLoadingDtoListByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetLoadingByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<Loading>(); // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's Loading 
        private Loading MapperLoading(LoadingDto dto)
        {
            var process = MapperProcess(dto.Process);
            return new Loading
            {
                Id = (Guid)dto.Id,
                Dock = dto.Dock,
                AutomaticLoadingTime = dto.AutomaticLoadingTime,
                AdditionalTimeInBuffer = dto.AdditionalTimeInBuffer,
                ProcessId = dto.ProcessId,
                Process = process
            };
        }

        private LoadingDto MapperLoadingDto(Loading loading)
        {
            var processDto = MapperProcessDto(loading.Process);
            return new LoadingDto
            {
                Id = loading.Id,
                Dock = loading.Dock,
                AutomaticLoadingTime = loading.AutomaticLoadingTime,
                AdditionalTimeInBuffer = loading.AdditionalTimeInBuffer,
                ProcessId = loading.ProcessId,
                Process = processDto
            };
        }
        #endregion

        #region Picking Process Implementation
        public async Task AddPicking(PickingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Picking picking = MapperPicking(itemDto);

                if (picking == null)
                    throw new InvalidOperationException("The picking could not be mapped correctly.");

                var process = GetProcessByIdNoTracking(picking.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", picking.Process);
                else
                    operationDB.AddUpdate("Processes", picking.Process);

                operationDB.AddNew("Pickings", picking);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Picking added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the picking. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdatePicking(PickingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Picking picking = MapperPicking(itemDto);

                if (picking == null)
                    throw new InvalidOperationException("The picking could not be mapped correctly.");

                operationDB.AddUpdate("Pickings", picking);
                operationDB.AddUpdate("Processes", picking.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated picking for process: {picking.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the picking. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeletePicking(PickingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Picking picking = MapperPicking(itemDto);

                if (picking == null)
                    throw new InvalidOperationException("The picking could not be mapped correctly.");

                operationDB.AddDelete("Pickings", picking);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted picking for process: {picking.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the picking. DTO: {itemDto.Name}");
                throw;
            }
        }

        public PickingDto? GetPickingDtoById(Guid id)
        {
            try
            {
                Picking picking = _dataAccess.GetPickingByIdWithProcess(id);

                if (picking == null)
                {
                    _logger.LogWarning($"No picking found with the Id: {id}");
                    return null;
                }

                return new PickingDto
                {
                    Id = picking.Id,
                    PickingRoadTime = picking.PickingRoadTime,
                    ProcessId = picking.ProcessId,
                    Process = MapperProcessDto(picking.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the picking.");
                return null;
            }
        }

        public PickingDto? GetPickingDtoByProcessId(Guid id)
        {
            try
            {
                Picking picking = _dataAccess.GetPickingByProcessId(id);

                if (picking == null)
                {
                    _logger.LogWarning($"No picking found with the Id: {id}");
                    return null;
                }

                return new PickingDto
                {
                    Id = picking.Id,
                    PickingRoadTime = picking.PickingRoadTime,
                    ProcessId = picking.ProcessId,
                    Process = MapperProcessDto(picking.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the picking.");
                return null;
            }
        }

        public IEnumerable<Picking?> GetPickingListByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetPickingByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<Picking>(); // Returns null in case of error
            }
        }
        #endregion

        #region Mapper's Picking
        private Picking MapperPicking(PickingDto dto)
        {
            var process = MapperProcess(dto.Process);
            return new Picking
            {
                Id = (Guid)dto.Id,
                PickingRoadTime = dto.PickingRoadTime,
                ProcessId = dto.ProcessId,
                Process = process
            };
        }

        private PickingDto MapperPickingDto(Picking picking)
        {
            var processDto = MapperProcessDto(picking.Process);
            return new PickingDto
            {
                Id = picking.Id,
                PickingRoadTime = picking.PickingRoadTime,
                ProcessId = picking.ProcessId,
                Process = processDto
            };
        }
        #endregion

        #region Putaway  Process Implementation
        public async Task AddPutaway(PutawayDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Putaway putaway = MapperPutaway(itemDto);

                if (putaway == null)
                    throw new InvalidOperationException("The putaway could not be mapped correctly.");

                var process = GetProcessByIdNoTracking(putaway.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", putaway.Process);
                else
                    operationDB.AddUpdate("Processes", putaway.Process);

                operationDB.AddNew("Putaways", putaway);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Putaway added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the putaway. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdatePutaway(PutawayDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Putaway putaway = MapperPutaway(itemDto);

                if (putaway == null)
                    throw new InvalidOperationException("The putaway could not be mapped correctly.");

                operationDB.AddUpdate("Putaways", putaway);
                operationDB.AddUpdate("Processes", putaway.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated putaway for process: {putaway.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the putaway. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeletePutaway(PutawayDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Putaway putaway = MapperPutaway(itemDto);

                if (putaway == null)
                    throw new InvalidOperationException("The putaway could not be mapped correctly.");

                operationDB.AddDelete("Putaways", putaway);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted putaway for process: {putaway.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the putaway. DTO: {itemDto.Name}");
                throw;
            }
        }

        public PutawayDto? GetPutawayDtoById(Guid id)
        {
            try
            {
                Putaway putaway = _dataAccess.GetPutawayByIdWithProcess(id);

                if (putaway == null)
                {
                    _logger.LogWarning($"No putaway found with the Id: {id}");
                    return null;
                }

                return new PutawayDto
                {
                    Id = putaway.Id,
                    AdditionTmeToPutaway = putaway.AdditionTmeToPutaway,
                    MinHour = putaway.MinHour,
                    ProcessId = putaway.ProcessId,
                    Process = MapperProcessDto(putaway.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the putaway.");
                return null;
            }
        }

        public PutawayDto? GetPutawayDtoByProcessId(Guid id)
        {
            try
            {
                Putaway putaway = _dataAccess.GetPutawayByProcessId(id);

                if (putaway == null)
                {
                    _logger.LogWarning($"No putaway found with the Id: {id}");
                    return null;
                }

                return new PutawayDto
                {
                    Id = putaway.Id,
                    AdditionTmeToPutaway = putaway.AdditionTmeToPutaway,
                    MinHour = putaway.MinHour,
                    ProcessId = putaway.ProcessId,
                    Process = MapperProcessDto(putaway.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the putaway.");
                return null;
            }
        }

        public IEnumerable<Putaway?> GetPutawayListByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetPutawayByIdProcessAsNoTracking(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<Putaway>(); // Returns null in case of error
            }
        }
        #endregion

        #region Mapper's Putaway 
        private Putaway MapperPutaway(PutawayDto dto)
        {
            var process = MapperProcess(dto.Process);
            return new Putaway
            {
                Id = (Guid)dto.Id,
                AdditionTmeToPutaway = dto.AdditionTmeToPutaway,
                MinHour = dto.MinHour,
                ProcessId = dto.ProcessId,
                Process = process
            };
        }

        private PutawayDto MapperPutawayDto(Putaway putaway)
        {
            var processDto = MapperProcessDto(putaway.Process);
            return new PutawayDto
            {
                Id = putaway.Id,
                AdditionTmeToPutaway = putaway.AdditionTmeToPutaway,
                MinHour = putaway.MinHour,
                ProcessId = putaway.ProcessId,
                Process = processDto
            };
        }
        #endregion

        #region Shipping  Process Implementation
        public async Task AddShipping(ShippingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Shipping shipping = MapperShipping(itemDto);

                if (shipping == null)
                    throw new InvalidOperationException("The shipping could not be mapped correctly.");

                var process = GetProcessByIdNoTracking(shipping.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", shipping.Process);
                else
                    operationDB.AddUpdate("Processes", shipping.Process);

                operationDB.AddNew("Shippings", shipping);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Shipping added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the shipping. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateShipping(ShippingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Shipping shipping = MapperShipping(itemDto);

                if (shipping == null)
                    throw new InvalidOperationException("The shipping could not be mapped correctly.");

                operationDB.AddUpdate("Shippings", shipping);
                operationDB.AddUpdate("Processes", shipping.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated shipping for process: {shipping.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the shipping. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeleteShipping(ShippingDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Shipping shipping = MapperShipping(itemDto);

                if (shipping == null)
                    throw new InvalidOperationException("The shipping could not be mapped correctly.");

                operationDB.AddDelete("Shippings", shipping);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted shipping for process: {shipping.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the shipping. DTO: {itemDto.Name}");
                throw;
            }
        }

        public ShippingDto? GetShippingDtoById(Guid id)
        {
            try
            {
                Shipping shipping = _dataAccess.GetShippingByIdWithProcess(id);

                if (shipping == null)
                {
                    _logger.LogWarning($"No shipping found with the Id: {id}");
                    return null;
                }

                return new ShippingDto
                {
                    Id = shipping.Id,
                    Quantity = shipping.Quantity,
                    ProcessId = shipping.ProcessId,
                    Process = MapperProcessDto(shipping.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the shipping.");
                return null;
            }
        }

        public ShippingDto? GetShippingDtoByProcessId(Guid processId)
        {
            try
            {
                Shipping shipping = _dataAccess.GetShippingByProcessId(processId);

                if (shipping == null)
                {
                    _logger.LogWarning($"No shipping found with the Id: {processId}");
                    return null;
                }

                return new ShippingDto
                {
                    Id = shipping.Id,
                    Quantity = shipping.Quantity,
                    ProcessId = shipping.ProcessId,
                    Process = MapperProcessDto(shipping.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the shipping.");
                return null;
            }
        }

        public IEnumerable<Shipping?> GetShippingListByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetShippingByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<Shipping>(); // Returns null in case of error
            }
        }

        #endregion

        #region Mapper's Shipping 
        private Shipping MapperShipping(ShippingDto dto)
        {
            var process = MapperProcess(dto.Process);
            return new Shipping
            {
                Id = dto.Id,
                Quantity = dto.Quantity,
                ProcessId = dto.ProcessId,
                Process = process
            };
        }

        private ShippingDto MapperShippingDto(Shipping shipping)
        {
            var processDto = MapperProcessDto(shipping.Process);
            return new ShippingDto
            {
                Id = shipping.Id,
                Quantity = shipping.Quantity,
                ProcessId = shipping.ProcessId,
                Process = processDto
            };
        }
        #endregion

        #region Reception Process Implementation
        public async Task AddReception(ReceptionDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Reception reception = MapperReception(itemDto);

                if (reception == null)
                    throw new InvalidOperationException("The reception could not be mapped correctly.");

                var process = GetProcessByIdNoTracking(reception.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", reception.Process);
                else
                    operationDB.AddUpdate("Processes", reception.Process);

                operationDB.AddNew("Receptions", reception);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Reception added successfully: {ProcessName}", itemDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the reception. DTO: {@ItemDto}", itemDto);
                throw;
            }
        }

        public async Task UpdateReception(ReceptionDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Reception reception = MapperReception(itemDto);

                if (reception == null)
                    throw new InvalidOperationException("The reception could not be mapped correctly.");

                operationDB.AddUpdate("Receptions", reception);
                operationDB.AddUpdate("Processes", reception.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated reception for process: {reception.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the reception. DTO: {itemDto.Name}");
                throw;
            }
        }

        public async Task DeleteReception(ReceptionDto itemDto)
        {
            try
            {
                operationDB = new OperationDB();
                Reception reception = MapperReception(itemDto);

                if (reception == null)
                    throw new InvalidOperationException("The reception could not be mapped correctly.");

                operationDB.AddDelete("Receptions", reception);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted reception for process: {reception.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the reception. DTO: {itemDto.Name}");
                throw;
            }
        }

        public ReceptionDto? GetReceptionDtoById(Guid id)
        {
            try
            {
                Reception reception = _dataAccess.GetReceptionByIdWithProcessAsNoTracking(id);

                if (reception == null)
                {
                    _logger.LogWarning($"No reception found with the Id: {id}");
                    return null;
                }

                return new ReceptionDto
                {
                    Id = reception.Id,
                    BreakageFactor = reception.BreakageFactor,
                    ProcessId = reception.ProcessId,
                    Process = MapperProcessDto(reception.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the reception.");
                return null;
            }
        }

        public ReceptionDto? GetReceptionDtoByProcessId(Guid id)
        {
            try
            {
                Reception reception = _dataAccess.GetReceptionByProcessIdAsNoTracking(id);

                if (reception == null)
                {
                    _logger.LogWarning($"No reception found with the Id: {id}");
                    return null;
                }

                return new ReceptionDto
                {
                    Id = reception.Id,
                    BreakageFactor = reception.BreakageFactor,
                    ProcessId = reception.ProcessId,
                    Process = MapperProcessDto(reception.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the reception.");
                return null;
            }
        }

        public IEnumerable<Reception?> GetReceptionByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetReceptionByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the process from the database.");
                return Enumerable.Empty<Reception>(); // Returns null in case of error
            }
        }
        #endregion

        #region Mapper's Reception 
        private Reception MapperReception(ReceptionDto dto)
        {
            var process = MapperProcess(dto.Process);
            return new Reception
            {
                Id = (Guid)dto.Id,
                BreakageFactor = dto.BreakageFactor,
                ProcessId = dto.ProcessId,
                Process = process
            };
        }
        private ReceptionDto MapperReceptionDto(Reception reception)
        {
            var processDto = MapperProcessDto(reception.Process);
            return new ReceptionDto
            {
                Id = reception.Id,
                BreakageFactor = reception.BreakageFactor,
                ProcessId = reception.ProcessId,
                Process = processDto
            };
        }
        #endregion

        #region Replenishment Process Implementation

        public async Task AddReplenishment(ReplenishmentDto replenishmentDto)
        {
            try
            {
                operationDB = new OperationDB();
                Replenishment replenishment = MapperReplenishment(replenishmentDto);

                if (replenishment == null)
                    throw new InvalidOperationException("The replenishment could not be mapped correctly.");

                var process = GetProcessByIdNoTracking(replenishment.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", replenishment.Process);
                else
                    operationDB.AddUpdate("Processes", replenishment.Process);

                operationDB.AddNew("Replenishments", replenishment);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Replenishment added successfully: {ProcessName}", replenishmentDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the replenishment. DTO: {@ItemDto}", replenishmentDto);
                throw;
            }
        }

        public async Task UpdateReplenishment(ReplenishmentDto replenishmentDto)
        {
            try
            {
                operationDB = new OperationDB();
                Replenishment replenishment = MapperReplenishment(replenishmentDto);

                if (replenishment == null)
                    throw new InvalidOperationException("The replenishment could not be mapped correctly.");

                operationDB.AddUpdate("Replenishments", replenishment);
                operationDB.AddUpdate("Processes", replenishment.Process);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated replenishment for process: {replenishment.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the replenishment. DTO: {replenishmentDto.Name}");
                throw;
            }
        }

        public async Task DeleteReplenishment(ReplenishmentDto replenishmentDto)
        {
            try
            {
                operationDB = new OperationDB();
                Replenishment replenishment = MapperReplenishment(replenishmentDto);

                if (replenishment == null)
                    throw new InvalidOperationException("The replenishment could not be mapped correctly.");

                operationDB.AddDelete("Replenishments", replenishment);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted replenishment for process: {replenishment.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the replenishment. DTO: {replenishmentDto.Name}");
                throw;
            }
        }

        public ReplenishmentDto? GetReplenishmentDtoById(Guid id)
        {
            try
            {
                Replenishment replenishment = _dataAccess.GetReplenishmentByIdWithProcessAsNoTracking(id);

                if (replenishment == null)
                {
                    _logger.LogWarning($"No replenishment found with the Id: {id}");
                    return null;
                }

                return new ReplenishmentDto
                {
                    Id = replenishment.Id,
                    Percentage = replenishment.Percentage ?? 0,
                    ProcessId = replenishment.ProcessId,
                    Process = MapperProcessDto(replenishment.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the replenishment.");
                return null;
            }
        }

        public ReplenishmentDto? GetReplenishmentDtoByProcessId(Guid processId)
        {
            try
            {
                Replenishment replenishment = _dataAccess.GetReplenishmentByProcessIdAsNoTracking(processId);

                if (replenishment == null)
                {
                    _logger.LogWarning($"No replenishment found with the Id: {processId}");
                    return null;
                }

                return new ReplenishmentDto
                {
                    Id = replenishment.Id,
                    Percentage = replenishment.Percentage ?? 0,
                    ProcessId = replenishment.ProcessId,
                    Process = MapperProcessDto(replenishment.Process)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the replenishment.");
                return null;
            }
        }

        public IEnumerable<Replenishment?> GetReplenishmentByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetReplenishmentByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the replenishment from the database.");
                return Enumerable.Empty<Replenishment>(); // Returns null in case of error
            }
        }

        #endregion

        #region Mapper´s Replenishment
        private Replenishment MapperReplenishment(ReplenishmentDto dto)
        {
            var process = MapperProcess(dto.Process);
            return new Replenishment
            {
                Id = dto.Id ?? Guid.Empty,
                Percentage = dto.Percentage,
                ProcessId = dto.ProcessId,
                Process = process
            };
        }
        #endregion


        #region Packing Process Implementation

        public async Task AddPacking(PackingDto packingDto)
        {
            try
            {
                operationDB = new OperationDB();
                Packing packing = MapperPacking(packingDto);

                List<PackingMode> MasterPackModes = _dataAccess.GetPackModes().ToList();

                List<PackingPacksMode> PackModes = MapperPackModes(packingDto, MasterPackModes);

                if (packing == null)
                    throw new InvalidOperationException("The packing could not be mapped correctly.");

                var process = GetProcessByIdNoTracking(packing.Process.Id);
                if (process == null)
                    operationDB.AddNew("Processes", packing.Process);
                else
                    operationDB.AddUpdate("Processes", packing.Process);

                operationDB.AddNew("Packing", packing);

                foreach (var mode in PackModes)
                {
                    operationDB.AddNew("PackingPacksMode", mode);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation("Packing added successfully: {ProcessName}", packingDto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the packing. DTO: {@ItemDto}", packingDto);
                throw;
            }
        }

        public async Task UpdatePacking(PackingDto packingDto)
        {
            try
            {
                operationDB = new OperationDB();
                Packing packing = MapperPacking(packingDto);

                List<PackingMode> MasterPackModes = _dataAccess.GetPackModes().ToList();

                List<PackingPacksMode> PackModes = MapperPackModes(packingDto, MasterPackModes);

                if (packing == null)
                    throw new InvalidOperationException("The packing could not be mapped correctly.");

                operationDB.AddUpdate("Packing", packing);
                operationDB.AddUpdate("Processes", packing.Process);

                foreach (var mode in PackModes)
                {
                    operationDB.AddUpdate("PackingPacksMode", mode);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly updated packing for process: {packing.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating the packing. DTO: {packingDto.Name}");
                throw;
            }
        }

        public async Task DeletePacking(PackingDto packingDto)
        {
            try
            {
                operationDB = new OperationDB();
                Packing packing = MapperPacking(packingDto);

                if (packing == null)
                    throw new InvalidOperationException("The packing could not be mapped correctly.");

                operationDB.AddDelete("Packing", packing);

                await _simulateService.SaveChangesInDataBase(operationDB);

                _logger.LogInformation($"Correctly deleted packing for process: {packing.Process.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting the packing. DTO: {packingDto.Name}");
                throw;
            }
        }

        public PackingDto? GetPackingDtoById(Guid id)
        {
            try
            {
                Packing packing = _dataAccess.GetPackingByIdWithProcessAsNoTracking(id);

                List<PackingPacksMode> PackModes = _dataAccess.GetPackModesByPackId(id).ToList();

                if (packing == null)
                {
                    _logger.LogWarning($"No packing found with the Id: {id}");
                    return null;
                }

                return new PackingDto
                {
                    Id = packing.Id,
                    ProcessId = packing.ProcessId,
                    Process = MapperProcessDto(packing.Process),
                    PackModes = MapperPackModes(PackModes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the packing.");
                return null;
            }
        }

        public PackingDto? GetPackingDtoByProcessId(Guid processId)
        {
            try
            {
                Packing packing = _dataAccess.GetPackingByProcessIdAsNoTracking(processId);

                List<PackingPacksMode> PackModes = _dataAccess.GetPackModesByPackId(packing.Id).ToList();

                if (packing == null)
                {
                    _logger.LogWarning($"No packing found with the Id: {processId}");
                    return null;
                }

                return new PackingDto
                {
                    Id = packing.Id,
                    ProcessId = packing.ProcessId,
                    Process = MapperProcessDto(packing.Process),
                    PackModes = MapperPackModes(PackModes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining the packing.");
                return null;
            }
        }

        public IEnumerable<Packing?> GetPackingByProcessId(Guid processId)
        {
            try
            {
                return _dataAccess.GetPackingByIdProcess(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Packing from the database.");
                return Enumerable.Empty<Packing>(); // Returns null in case of error
            }
        }

        #endregion

        #region Mapper´s Packing
        private Packing MapperPacking(PackingDto dto)
        {
            var process = MapperProcess(dto.Process);
            return new Packing
            {
                Id = dto.Id ?? Guid.Empty,
                ProcessId = dto.ProcessId,
                Process = process
            };
        }

        private List<PackModeDto> MapperPackModes(List<PackingPacksMode> packModes)
        {
            return packModes.Select(x =>
            {
                if (!Enum.TryParse<PackModes>(x.PackingMode.PackingType, true, out var mode))
                    throw new Exception($"PackingType inválido: {x.PackingMode.PackingType}");

                return new PackModeDto
                {
                    Id = x.Id,
                    PackName = mode,
                    PackQty = x.NumPackages ?? 1,
                };
            }).ToList();
        }

        private List<PackingPacksMode> MapperPackModes(PackingDto packingDto, List<PackingMode> packingModes)
        {
            return packingDto.PackModes
                .Select(x =>
                {
                    var dbMode = packingModes
                        .First(m => m.PackingType == x.PackName.ToString());

                    return new PackingPacksMode
                    {
                        Id = x.Id,
                        PackingId = packingDto.Id ?? Guid.NewGuid(),
                        PackingModeId = dbMode.Id,
                        NumPackages = x.PackQty
                    };
                })
                .ToList();
        }




        #endregion

        #region FlowGraphs Implementation

        public async Task DeleteFlowGraphAsync(Guid guid, FlowType flowType)
        {
            try
            {
                operationDB = new OperationDB();
                switch (flowType)
                {
                    case FlowType.Inbound:
                        var inbound = _dataAccess.GetInboundFlowGraphByFlowId(guid);
                        if (inbound != null)
                            operationDB.AddDelete("InboundFlowGraphs", new { Id = inbound.Id });
                        break;
                    case FlowType.Outbound:
                        var outbound = _dataAccess.GetOutboundFlowGraphByFlowId(guid);
                        if (outbound != null)
                            operationDB.AddDelete("OutboundFlowGraphs", new { Id = outbound.Id });
                        break;
                    case FlowType.Custom:
                        var custom = _dataAccess.GetCustomFlowGraphByFlowId(guid);
                        if (custom != null)
                            operationDB.AddDelete("CustomFlowGraphs", new { Id = custom.Id });
                        break;
                }

                operationDB.AddDelete("Flow", new { Id = guid });
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("Flow deleted successfully: {FlowId}", guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flow graph: {@Guid}", guid);
                throw;
            }
        }

        public (Guid? FlowId, string FlowName) GetFlowInfoFromType(Guid childId, FlowType type)
        {
            switch (type)
            {
                case FlowType.Inbound:
                    var inbound = _dataAccess.GetInboundFlowGraphById(childId);
                    return (inbound?.FlowId, inbound?.Name ?? "Inbound");

                case FlowType.Outbound:
                    var outbound = _dataAccess.GetOutboundFlowGraphById(childId);
                    return (outbound?.FlowId, outbound?.Name ?? "Outbound");

                case FlowType.Custom:
                    var custom = _dataAccess.GetCustomFlowGraphDtoById(childId);
                    return (custom?.FlowId, custom?.Name ?? "Custom");

                default:

                    var flow = _dataAccess.GetFlowById(childId);
                    return (flow?.Id, flow?.Name ?? "Flow");
            }
        }

        public FlowDto? GetFlowDtoById(Guid flowId)
        {
            try
            {
                var flow = _dataAccess.GetFlowById(flowId);

                if (flow == null)
                    return null;

                return new FlowDto
                {
                    Id = flow.Id,
                    Name = flow.Name,
                    Type = Enum.Parse<FlowType>(flow.Type),
                    WarehouseId = flow.WarehouseId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Flow by Id: {FlowId}", flowId);
                return null;
            }
        }

        #endregion

        #region InboundFlowGraphs Implementation
        public async Task AddInboundFlowGraph(InboundFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                InboundFlowGraph entity = MapperInboundFlowGraph(dto);
                var flowIdNew = Guid.NewGuid();
                operationDB.AddNew("Flow", new Flow
                {
                    Id = flowIdNew,
                    Type = "Inbound",
                    Name = dto.Name,
                    WarehouseId = dto.WarehouseId
                });
                entity.FlowId = flowIdNew;
                operationDB.AddNew("InboundFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("InboundFlowGraph added successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding InboundFlowGraph: {@Dto}", dto);
                throw;
            }
        }

        public async Task UpdateInboundFlowGraph(InboundFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                InboundFlowGraph entity = MapperInboundFlowGraph(dto);
                operationDB.AddUpdate("InboundFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("InboundFlowGraph updated successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating InboundFlowGraph: {@Dto}", dto);
                throw;
            }
        }

        public async Task DeleteInboundFlowGraph(InboundFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                InboundFlowGraph entity = MapperInboundFlowGraph(dto);
                operationDB.AddDelete("InboundFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("InboundFlowGraph deleted successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting InboundFlowGraph: {@Dto}", dto);
                throw;
            }
        }

        public async Task DeleteInboundFlowGraphs(IEnumerable<InboundFlowGraphDto> dtos)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in dtos)
                {
                    InboundFlowGraph entity = MapperInboundFlowGraph(item);
                    operationDB.AddDelete("InboundFlowGraphs", entity);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("InboundFlowGraphs deleted successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting InboundFlowGraph");
                throw;
            }
        }

        public InboundFlowGraphDto? GetInboundFlowGraphById(Guid id)
        {
            try
            {
                var entity = _dataAccess.GetInboundFlowGraphById(id);
                return entity == null ? null : MapperInboundFlowGraphDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving InboundFlowGraph by id: {Id}", id);
                return null;
            }
        }

        public InboundFlowGraphDto? GetInboundFlowGraphByWarehouseId(Guid id)
        {
            try
            {
                var entity = _dataAccess.GetInboundFlowGraphByWarehouseid(id);
                return entity == null ? null : MapperInboundFlowGraphDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving InboundFlowGraph by id: {Id}", id);
                return null;
            }
        }

        public IEnumerable<InboundFlowGraphDto> GetInboundFlowGraphsByWarehouseId(Guid warehouseId)
        {
            try
            {
                var entities = _dataAccess.GetInboundFlowGraphsByWarehouseId(warehouseId);
                return entities.Select(MapperInboundFlowGraphDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving InboundFlowGraphs for warehouse: {Id}", warehouseId);
                return Enumerable.Empty<InboundFlowGraphDto>();
            }
        }
        #endregion

        #region Mapper InboundFlowGraph

        private InboundFlowGraph MapperInboundFlowGraph(InboundFlowGraphDto dto)
        {
            var warehouse = _dataAccess.GetWarehouses().FirstOrDefault(x => x.Id == dto.WarehouseId);

            if (warehouse == null)
            {
                throw new InvalidOperationException($"No warehouse with ID was found {dto.WarehouseId}.");
            }


            return new InboundFlowGraph
            {
                Id = dto.Id,
                Name = dto.Name,
                DockSelectionStrategy = dto.DockSelection != null ? MapperDockSelectionStrategy(dto.DockSelection) : null,
                DockSelectionStrategyId = dto.DockSelectionStrategyId,
                AverageItemsPerOrder = dto.AverageItemsPerOrder,
                AverageLinesPerOrder = dto.AverageLinesPerOrder,
                Group = dto.Group,
                WarehouseId = dto.WarehouseId,
                Warehouse = warehouse,
                ViewPort = dto.ViewPort,
                MaxVehicleTime = dto.MaxVehicleTime
            };
        }

        private InboundFlowGraphDto MapperInboundFlowGraphDto(InboundFlowGraph entity)
        {
            return new InboundFlowGraphDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DockSelection = entity.DockSelectionStrategy != null ? MapperDockSelectionStrategyDto(entity.DockSelectionStrategy) : null,
                DockSelectionStrategyId = entity.DockSelectionStrategyId,
                AverageItemsPerOrder = entity.AverageItemsPerOrder,
                AverageLinesPerOrder = entity.AverageLinesPerOrder,
                Group = entity.Group ?? false,
                WarehouseId = entity.WarehouseId,
                ViewPort = entity.ViewPort,
                MaxVehicleTime = (double)entity.MaxVehicleTime
            };
        }

        #endregion

        #region CustomFlowGraph Implementation      
        public async Task AddCustomFlowGraph(CustomFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var flowIdNew = Guid.NewGuid();
                operationDB.AddNew("Flow", new Flow
                {
                    Id = flowIdNew,
                    Type = "Custom",
                    Name = dto.Name,
                    WarehouseId = dto.WarehouseId ?? Guid.Empty
                });
                var entity = new CustomFlowGraph
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    FlowId = flowIdNew
                };
                operationDB.AddNew("CustomFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("CustomFlowGraphs added successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding CustomFlowGraphs: {@Dto}", dto);
                throw;
            }
        }

        public async Task UpdateCustomFlow(CustomFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var flowIdNew = dto.FlowId ?? Guid.NewGuid();
                if (dto.FlowId == null)
                    operationDB.AddNew("Flow", new Flow
                    {
                        Id = flowIdNew,
                        Type = "Custom",
                        Name = dto.Name,
                        WarehouseId = dto.WarehouseId ?? Guid.Empty
                    });
                var entity = new CustomFlowGraph
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    FlowId = flowIdNew
                };
                operationDB.AddUpdate("CustomFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("CustomFlowGraphs updated successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding CustomFlowGraphs: {@Dto}", dto);
                throw;
            }
        }

        public async Task DeleteCustomFlowGraph(CustomFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var entity = MapperCustomFlowGrap(dto);
                operationDB.AddDelete("CustomFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("CustomFlowGraphs deleted successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CustomFlowGraphs: {@Dto}", dto);
                throw;
            }
        }

        #endregion

        #region Mapper CustomFlowGrap
        private CustomFlowGraphDto MapperCustomFlowGrapDto(CustomFlowGraph entity)
        {
            return new CustomFlowGraphDto
            {
                Id = entity.Id,
                Name = entity.Name,
                FlowId = entity.FlowId,
            };
        }

        private CustomFlowGraph MapperCustomFlowGrap(CustomFlowGraphDto dto)
        {
            return new CustomFlowGraph
            {
                Id = dto.Id,
                Name = dto.Name,
                FlowId = dto.FlowId,
            };
        }
        #endregion

        #region OutboundFlowGraph Implementation      
        public async Task AddOutboundFlowGraph(OutboundFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var entity = MapperOutboundFlowGraph(dto);
                var flowIdNew = Guid.NewGuid();
                operationDB.AddNew("Flow", new Flow
                {
                    Id = flowIdNew,
                    Type = "Outbound",
                    Name = dto.Name,
                    WarehouseId = dto.WarehouseId
                });
                entity.FlowId = flowIdNew;
                operationDB.AddNew("OutboundFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("OutboundFlowGraph added successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding OutboundFlowGraph: {@Dto}", dto);
                throw;
            }
        }

        public async Task UpdateOutboundFlowGraph(OutboundFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var entity = MapperOutboundFlowGraph(dto);
                operationDB.AddUpdate("OutboundFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("OutboundFlowGraph updated successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OutboundFlowGraph: {@Dto}", dto);
                throw;
            }
        }

        public async Task DeleteOutboundFlowGraph(OutboundFlowGraphDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var entity = MapperOutboundFlowGraph(dto);
                operationDB.AddDelete("OutboundFlowGraphs", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("OutboundFlowGraph deleted successfully: {Name}", dto.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting OutboundFlowGraph: {@Dto}", dto);
                throw;
            }
        }

        public async Task DeleteOutboundFlowGraphs(IEnumerable<OutboundFlowGraphDto> dtos)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in dtos)
                {
                    var entity = MapperOutboundFlowGraph(item);
                    operationDB.AddDelete("OutboundFlowGraphs", entity);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("OutboundFlowGraph deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting OutboundFlowGraph");
                throw;
            }
        }

        public OutboundFlowGraphDto? GetOutboundFlowGraphById(Guid id)
        {
            try
            {
                var entity = _dataAccess.GetOutboundFlowGraphById(id);
                return entity == null ? null : MapperOutboundFlowGraphDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OutboundFlowGraph by id: {Id}", id);
                return null;
            }
        }

        public CustomFlowGraphDto? GetCustomFlowGraphById(Guid id)
        {
            try
            {
                return _dataAccess.GetCustomFlowGraphDtoById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OutboundFlowGraph by id: {Id}", id);
                return null;
            }
        }


        public OutboundFlowGraphDto? GetOutboundFlowGraphByWarehouseId(Guid warehouseId)
        {
            try
            {
                var entity = _dataAccess.GetOutboundFlowGraphByWarehouseId(warehouseId);
                return entity == null ? null : MapperOutboundFlowGraphDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OutboundFlowGraph by id: {Id}", warehouseId);
                return null;
            }
        }

        public CustomFlowGraphDto? GetCustomFlowGraphByWarehouseId(Guid warehouseId)
        {

            try
            {
                var entity = _dataAccess.GetCustomFlowGraphByWarehouseId(warehouseId);
                return entity == null ? null : MapperCustomFlowGrapDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OutboundFlowGraph by id: {Id}", warehouseId);
                return null;
            }
        }

        public IEnumerable<OutboundFlowGraphDto> GetOutboundFlowGraphsByWarehouseId(Guid warehouseId)
        {
            try
            {
                var entities = _dataAccess.GetOutboundFlowGraphsByWarehouseId(warehouseId);
                return entities.Select(MapperOutboundFlowGraphDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OutboundFlowGraphs for warehouse: {Id}", warehouseId);
                return Enumerable.Empty<OutboundFlowGraphDto>();
            }
        }
        #endregion

        #region Mapper OutboundFlowGraph
        private OutboundFlowGraph MapperOutboundFlowGraph(OutboundFlowGraphDto dto)
        {
            var warehouse = _dataAccess.GetWarehouses().FirstOrDefault(x => x.Id == dto.WarehouseId);

            if (warehouse == null)
            {
                throw new InvalidOperationException($"No warehouse with ID was found {dto.WarehouseId}.");
            }
            return new OutboundFlowGraph
            {
                Id = dto.Id,
                Name = dto.Name,
                DockSelectionStrategy = dto.DockSelection != null ? MapperDockSelectionStrategy(dto.DockSelection) : null,
                DockSelectionStrategyId = dto.DockSelectionStrategyId,
                AverageItemsPerOrder = dto.AverageItemsPerOrder,
                AverageLinesPerOrder = dto.AverageLinesPerOrder,
                Group = dto.Group,
                PartialClosed = dto.PartialClosed,
                WarehouseId = dto.WarehouseId,
                Warehouse = warehouse,
                SecurityLoadTime = dto.SecurityLoadTime,
                ViewPort = dto.ViewPort,
                MaxVehicleTime = dto.MaxVehicleTime
                

            };
        }

        private OutboundFlowGraphDto MapperOutboundFlowGraphDto(OutboundFlowGraph entity)
        {
            return new OutboundFlowGraphDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DockSelection = entity.DockSelectionStrategy != null ? MapperDockSelectionStrategyDto(entity.DockSelectionStrategy) : null,
                DockSelectionStrategyId = entity.DockSelectionStrategyId,
                AverageItemsPerOrder = entity.AverageItemsPerOrder,
                AverageLinesPerOrder = entity.AverageLinesPerOrder,
                Group = (bool)entity.Group,
                PartialClosed = (bool)entity.PartialClosed,
                WarehouseId = entity.WarehouseId,
                SecurityLoadTime = entity.SecurityLoadTime,
                ViewPort = entity.ViewPort,
                MaxVehicleTime = (double)entity.MaxVehicleTime

            };
        }
        #endregion

        #region Mapper Flow

        private FlowDto MapperFlowDto(dynamic dto, FlowType flowType)
        {
            return new FlowDto()
            {
                Id = dto.Id,
                Name = dto.Name ?? string.Empty,
                Type = flowType,
                WarehouseId = dto.WarehouseId
            };
        }

        #endregion

        #region DockSelectionStrategy Implementation
        public async Task AddDockSelectionStrategy(DockSelectionStrategyDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var entity = MapperDockSelectionStrategy(dto);
                operationDB.AddNew("DockSelectionStrategies", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("DockSelectionStrategy added successfully: {Code}", dto.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding DockSelectionStrategy: {@Dto}", dto);
                throw;
            }
        }
        public async Task UpdateDockSelectionStrategy(DockSelectionStrategyDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var entity = MapperDockSelectionStrategy(dto);
                operationDB.AddUpdate("DockSelectionStrategies", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("DockSelectionStrategy updated successfully: {Code}", dto.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DockSelectionStrategy: {@Dto}", dto);
                throw;
            }
        }

        public async Task DeleteDockSelectionStrategy(DockSelectionStrategyDto dto)
        {
            try
            {
                operationDB = new OperationDB();
                var entity = MapperDockSelectionStrategy(dto);
                operationDB.AddDelete("DockSelectionStrategies", entity);
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("DockSelectionStrategy deleted successfully: {Code}", dto.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting DockSelectionStrategy: {@Dto}", dto);
                throw;
            }
        }

        public DockSelectionStrategyDto? GetDockSelectionStrategyById(Guid id)
        {
            try
            {
                var entity = _dataAccess.GetDockSelectionStrategyById(id);
                return entity == null ? null : MapperDockSelectionStrategyDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DockSelectionStrategy by id: {Id}", id);
                return null;
            }
        }

        public IEnumerable<DockSelectionStrategyDto> GetDockSelectionStrategiesByWarehouseId(Guid warehouseId)
        {
            try
            {
                var entities = _dataAccess.GetDockSelectionStrategiesByWarehouseId(warehouseId);
                return entities.Select(MapperDockSelectionStrategyDto).OrderBy(x=> x.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving DockSelectionStrategies for warehouse: {Id}", warehouseId);
                return Enumerable.Empty<DockSelectionStrategyDto>();
            }
        }

        #endregion

        #region Mapper DockSelectionStrategy
        private DockSelectionStrategy MapperDockSelectionStrategy(DockSelectionStrategyDto dto)
        {


            return new DockSelectionStrategy
            {
                Id = dto.Id,
                Code = dto.Code,
                OrganizationId = dto.OrganizationId,
                Organization = dto.Organization
            };
        }

        private DockSelectionStrategyDto MapperDockSelectionStrategyDto(DockSelectionStrategy entity)
        {
            return new DockSelectionStrategyDto
            {
                Id = entity.Id,
                Code = entity.Code,
                OrganizationId = entity.OrganizationId,
                Organization = entity.Organization
            };
        }





        #endregion

        #region Steps Implementation

        public async Task AddStepDtoList(IEnumerable<StepDto> stepDtoList)
        {
            try
            {
                operationDB = new(); // Initialize database operation

                foreach (StepDto stepDto in stepDtoList)
                {
                    Step? step = MapperStep(stepDto); // Mapping the DTO to the data model

                    if (null == step)
                        throw new InvalidOperationException("The Step could not be mapped correctly.");

                    operationDB.AddNew("Steps", step); // Add the new layout to the database
                }

                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes to the database

                _logger.LogInformation($"List of successfully adding step.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding step. List:  {stepDtoList.ToList()}"); // Log the error with additional details
                throw;
            }
        }

        public async Task UpdateStepDto(IEnumerable<StepDto> stepDtoList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (StepDto? stepDto in stepDtoList)
                {
                    Step? step = MapperStep(stepDto);

                    if (step == null)
                        throw new InvalidOperationException("The step could not be mapped correctly.");

                    operationDB.AddUpdate("Steps", step);
                }

                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes in DataBase

                _logger.LogInformation($"List of successfully updated step.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating step. List:  {stepDtoList.ToList()}");
                throw;
            }
        }

        public async Task DeleteStepsDtoList(IEnumerable<StepDto> stepList)
        {
            try
            {
                operationDB = new();
                foreach (StepDto? stepDto in stepList)
                {
                    Step step = MapperStep(stepDto);
                    if (stepDto == null)
                        throw new InvalidOperationException("The step could not be mapped correctly.");

                    operationDB.AddDelete("Steps", step);
                }

                await _simulateService.SaveChangesInDataBase(operationDB); // Save changes to the database
                _logger.LogInformation($"List of successfully deleted steps.");
            }
            catch (Exception ex)
            {
                //Log the error with additional details
                _logger.LogError(ex, $"Error deleted steps. List:  {stepList}");
                throw;
            }
        }

        public StepDto GetStepsById(Guid stepId)
        {
            try
            {
                Step step = _dataAccess.GetStepByStepIdAsNoTracking(stepId); // Get the step from the database

                if (step == null) // If step is null, return null
                {
                    _logger.LogWarning($"No Step found with the Id: {stepId}");
                    return null;
                }

                return MapperStepDto(step); // Map Step to StepDto
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Step from the database.");
                return null; // Returns null in case of error
            }
        }

        public StepDto? GetStepsDtoByProcessId(Guid processId)
        {
            try
            {
                Step? step = _dataAccess.GetStepsWithProcessByProcessIdNoTracking(processId); // Get the step from the database

                if (step == null) // If step is null, return null
                {
                    _logger.LogWarning($"No Step found with the Id: {processId}");
                    return null;
                }

                return MapperStepDto(step); // Map Step to StepDto
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the Step from the database.");
                return null; // Returns null in case of error
            }
        }

        public IEnumerable<StepDto> GetStepsDtoByProcessIdNoTracking(Guid processId)
        {
            try
            {
                List<StepDto?> stepDtosList = new List<StepDto?>();
                IEnumerable<Step> stepList = _dataAccess.GetStepsListWithProcessByProcessIdNoTracking(processId);
                foreach (var step in stepList)
                {
                    StepDto stepDto = MapperStepDto(step);
                    stepDtosList.Add(stepDto);
                }
                return stepDtosList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting the list of steps from the database.");
                return Enumerable.Empty<StepDto>(); // Returns null in case of error
            }
        }

        #endregion

        #region MapperSteps

        public Step MapperStep(StepDto stepDto)
        {
            Process? process = _dataAccess.GetProcessByIdNoTracking(stepDto.ProcessId);

            if (process == null)
                throw new InvalidOperationException($"No process with ID was found {stepDto.ProcessId}.");

            return new Step
            {
                Id = stepDto.Id,
                Name = stepDto.Name,
                TimeMin = stepDto.TimeMin,
                Order = stepDto.Order, 
                InitProcess = stepDto.InitProcess,
                EndProcess = stepDto.EndProcess,
                ProcessId = stepDto.ProcessId,
                Process = process,
                ViewPort = string.Empty
            };
        }

        public StepDto MapperStepDto(Step stepDto)
        {
            return new StepDto
            {
                Id = stepDto.Id,
                Name = stepDto.Name,
                TimeMin = stepDto.TimeMin ?? 0,
                Order = stepDto.Order,
                InitProcess = stepDto.InitProcess,
                EndProcess = stepDto.EndProcess,
                ProcessId = stepDto.ProcessId,
                ViewPort = string.Empty
            };
        }

        #endregion

        #region AvailableDocksPerStages Implementation
        public async Task DeleteListAvailableDocksPerStages(IEnumerable<AvailableDocksPerStageDto> availableDocksList)
        {
            try
            {
                operationDB = new OperationDB();
                foreach (var item in availableDocksList)
                {
                    AvailableDocksPerStage? available = MapperAvailableDocksPerStage(item);

                    if (null == available)
                        throw new InvalidOperationException("The AvailableDocksPerStage could not be mapped correctly.");

                    operationDB.AddDelete("AvailableDocksPerStages", available);
                }
                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation("List of AvailableDocksPerStages deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list of AvailableDocksPerStages. DTO List: {@availableDocksList}", availableDocksList);
                throw;
            }
        }

        public async Task AddAvailableDocksPerStages(IEnumerable<AvailableDocksPerStageDto> availableDtoList)
        {
            try
            {
                operationDB = new();
                foreach (AvailableDocksPerStageDto availableDto in availableDtoList)
                {
                    AvailableDocksPerStage? available = MapperAvailableDocksPerStage(availableDto);

                    if (null == available)
                        throw new InvalidOperationException("The AvailableDocksPerStage could not be mapped correctly.");

                    operationDB.AddNew("AvailableDocksPerStages", available);
                }

                await _simulateService.SaveChangesInDataBase(operationDB);
                _logger.LogInformation($"List of successfully adding AvailableDocksPerStage.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding AvailableDocksPerStage. List:  {availableDtoList.ToList()}");
                throw;
            }
        }

        public IEnumerable<AvailableDocksPerStageDto> GetAvailableDocksPerStageDtoByStageId(Guid stageId)
        {
            try
            {
                var availableDocks = _dataAccess.GetAvailableDocksPerStagesByStepsIdAsNoTracking(stageId);
                if (availableDocks == null || !availableDocks.Any())
                {
                    _logger.LogWarning("No AvailableDocksPerStage found for StageId: {stageId}", stageId);
                    return Enumerable.Empty<AvailableDocksPerStageDto>();
                }
                return availableDocks
                        .Select(x => new AvailableDocksPerStageDto
                        {
                            Id = (Guid)x.GetType().GetProperty("Id").GetValue(x, null),
                            StageId = (Guid)x.GetType().GetProperty("StageId").GetValue(x, null),
                            DockId = (Guid)x.GetType().GetProperty("DockId").GetValue(x, null),
                            Name = (string)x.GetType().GetProperty("ZoneName").GetValue(x, null)
                        })
                        .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AvailableDocksPerStage for StageId: {stageId}", stageId);
                throw;
            }

        }
        public IEnumerable<AvailableDocksPerStageDto> GetAvailableDocksPerStageDtoByZoneId(Guid zoneId)
        {
            try
            {
                var availableDocks = _dataAccess.GetAvailableDocksPerStagesByZoneIdAsNoTracking(zoneId);
                if (availableDocks == null || !availableDocks.Any())
                {
                    _logger.LogWarning("No AvailableDocksPerStage found for StageId: {stageId}", zoneId);
                    return Enumerable.Empty<AvailableDocksPerStageDto>();
                }
                return availableDocks
                        .Select(x => new AvailableDocksPerStageDto
                        {
                            Id = (Guid)x.GetType().GetProperty("Id").GetValue(x, null),
                            StageId = (Guid)x.GetType().GetProperty("StageId").GetValue(x, null),
                            DockId = (Guid)x.GetType().GetProperty("DockId").GetValue(x, null),
                            Name = (string)x.GetType().GetProperty("ZoneName").GetValue(x, null)
                        })
                        .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AvailableDocksPerStage for StageId: {stageId}", zoneId);
                throw;
            }

        }

        #endregion

        #region AvailableDocksPerStage
        public AvailableDocksPerStage MapperAvailableDocksPerStage(AvailableDocksPerStageDto availableDto)
        {
            var stage = _dataAccess.GetStageByIdNoTracking(availableDto.StageId);
            var dock = _dataAccess.GetDockByIdNoTracking(availableDto.DockId);

            return new AvailableDocksPerStage
            {
                Id = availableDto.Id,
                StageId = availableDto.StageId,
                Stage = stage,
                DockId = availableDto.DockId,
                Dock = dock,
            };
        }

        public AvailableDocksPerStageDto MapperAvailableDocksPerStageDto(AvailableDocksPerStage available)
        {
            return new AvailableDocksPerStageDto
            {
                Id = available.Id,
                StageId = available.StageId,
                DockId = available.DockId,
            };
        }
        #endregion

        #region Check configuration
        public Task<Dictionary<string, List<ResourceMessage>>> CheckConfiguration(Guid warehouseId) =>
             _simulateService.CheckConfiguration(warehouseId);
        #endregion

        #region Calculate Name for zone
        /// <summary>
        /// Ajusta Zone.Name en cualquier lista de DTOs que contenga una propiedad Zone.
        /// - Si el nombre no existía en BD, se deja tal cual.
        /// - Si existía, se añade sufijo "_n" rellenando huecos (1,2,3…).
        /// </summary>
        /// <typeparam name="TDto">Cualquier DTO que tenga una propiedad de tipo ZoneDto.</typeparam>
        /// <param name="dtoList">Lista de DTOs a procesar.</param>
        /// <param name="getZone">Función que, dado un DTO, devuelve su objeto Zone (no nulo).</param>
        private void CalculateName<TDto>(List<TDto> dtoList, Func<TDto, ZoneDto> getZone)
        {
            if (dtoList == null || dtoList.Count == 0)
                return;

            var firstZone = getZone(dtoList[0])
                ?? throw new InvalidOperationException("The Zone is not initialized in the DTO.");
            var areaId = firstZone.AreaId;

            // 1) Recolecta los IDs de las zonas que se están editando
            var dtoZoneIds = new HashSet<Guid>(
                dtoList
                    .Select(getZone)
                    .Where(z => z.Id.HasValue && z.Id != Guid.Empty)
                    .Select(z => z.Id!.Value)
            );

            // 2) Obtén los nombres de zonas del área, EXCLUYENDO las que ya estamos editando
            var existingNames = new HashSet<string>(
                _dataAccess
                    .GetZonesWithAreaByAreaIdNoTracking(areaId)
                    .Where(z => !dtoZoneIds.Contains(z.Id)) // ← aquí se excluyen las zonas que se están actualizando
                    .Select(z => z.Name),
                StringComparer.OrdinalIgnoreCase
            );

            // 3) Para cada DTO, si el nombre ya existe, busca el primer "_n" libre
            foreach (var dto in dtoList)
            {
                var zone = getZone(dto);
                var candidate = zone.Name!;

                if (!existingNames.Contains(candidate))
                {
                    existingNames.Add(candidate);
                    continue;
                }

                int suffix = 1;
                string nextName;
                do
                {
                    nextName = $"{candidate}_{suffix}";
                    suffix++;
                }
                while (existingNames.Contains(nextName));

                zone.Name = nextName;
                existingNames.Add(nextName);
            }
        }

        public (bool itsUnique, IEnumerable<string> suggestions) CalculateNameToValidations(string nameToCompare, Guid areaId, Guid? currentId = null)
        {
            List<string> suggestions = new List<string>();
            var existingNames = new HashSet<string>(_dataAccess.GetZonesWithAreaByAreaIdNoTracking(areaId).Where(z => z.Id != currentId).Select(z => z.Name), StringComparer.OrdinalIgnoreCase); // Get existing names in DB (excluding the current one)

            if (!existingNames.Contains(nameToCompare))
                return (true, suggestions); // If the name does not exist

            // Generate alternative names
            int suffix = 1;
            while (suggestions.Count < 2)
            {
                var candidate = $"{nameToCompare}_{suffix}";
                if (!existingNames.Contains(candidate))
                    suggestions.Add(candidate);
                suffix++;
            }

            return (false, suggestions);
        }
        #endregion

        #region Dependencies Implementation

        public bool HasProcessDependenciesToFlow(Guid flowId)
        {
            return _dataAccess.HasProcessDependenciesToFlow(flowId);
        }

        #endregion
    }
}

