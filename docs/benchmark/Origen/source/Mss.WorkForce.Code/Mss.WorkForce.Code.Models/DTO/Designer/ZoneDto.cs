using Mss.WorkForce.Code.Models.Models;
using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class ZoneDto : BaseDesignerDto
    {
        public int MaxEquipments { get; set; }

        public ZoneType ZoneType { get; set; }

        [JsonIgnore]
        public int MaxStockToBook { get; set; }

        [JsonIgnore]
        public bool IsLimitStock { get; set; }

        [JsonIgnore]
        public int? MaxStock { get; set; }

        [JsonIgnore]
        public int MaxContainers { get; set; }

        [JsonIgnore]
        public int InitStockToBook { get; set; }

        public Guid AreaId { get; set; }

        [JsonIgnore]
        public AreaDto? Area { get; set; }

        [JsonIgnore]
        public string? ViewPort { get; set; }


        public OrientationType? Orientation { get; set; }

        public LocationTypes LocationTypes { get; set; }

        public int NumCrossAisles { get; set; }

        public int NumShelves { get; set; }

        public bool IsVertical { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Ignore in Serialize and Not ignore in Deserialize
        public Guid? IdOriginal { get; set; }
        /// <summary>
        /// Inicializa una nueva instancia de ZoneDto con valores por defecto.
        /// </summary>
        public static ZoneDto NewDto(AreaDto itemArea, ObjectTypes objectsTypes, string Name, OrientationType? orientation = OrientationType.None)
        {
            return new ZoneDto
            {
                // Propiedades heredadas de BaseDesignerDto
                Id = Guid.NewGuid(),
                CanvasObjectType = "Station",
                Name = Name ?? string.Empty,
                X = 0,
                Y = 0,
                Height = 0,
                Width = 0,
                MaxEquipments = 0,
                StatusObject = string.Empty,
                LayoutId = Guid.Empty,
                Layout = null,

                // Propiedades específicas de ZoneDto
                ZoneType = GetZoneTypeToObjectTypes(objectsTypes), // Asigna el valor predeterminado del enum ZoneType
                MaxStockToBook = 0,
                IsLimitStock = false,
                MaxStock = 0,
                MaxContainers = 0,
                InitStockToBook = 0,
                Orientation = orientation,
                AreaId = itemArea.Id ?? Guid.Empty,
                Area = itemArea,
                ViewPort = string.Empty,
                IdOriginal = Guid.Empty
            };
        }

        public static ZoneDto NewDto(AreaDto itemArea, ObjectTypes objectsTypes, string Name)
        {
            return new ZoneDto
            {
                // Propiedades heredadas de BaseDesignerDto
                Id = Guid.NewGuid(),
                CanvasObjectType = "Station",
                Name = Name,
                X = 0,
                Y = 0,
                Height = 0,
                Width = 0,
                MaxEquipments = 0,
                StatusObject = string.Empty,
                LayoutId = Guid.Empty,
                Layout = null,

                // Propiedades específicas de ZoneDto
                ZoneType = GetZoneTypeToObjectTypes(objectsTypes), // Asigna el valor predeterminado del enum ZoneType
                MaxStockToBook = 0,
                IsLimitStock = false,
                MaxStock = 0,
                MaxContainers = 0,
                InitStockToBook = 0,
                AreaId = itemArea.Id ?? Guid.Empty,
                Area = itemArea,
                ViewPort = string.Empty
            };
        }

        /// <summary>
        /// Crea un ZoneDto a partir de una entidad Zone existente y un AreaDto.
        /// Este método está separado de NewDto para mapear una entidad existente.
        /// </summary>
        public static ZoneDto GetZoneDTO(Zone zone, AreaDto itemArea)
        {
            if (zone == null)
                throw new ArgumentNullException(nameof(zone));

            // Convertir el string de la entidad en el enum ZoneType
            ZoneType zoneType;
            if (!Enum.TryParse(zone.Type, out zoneType))
                zoneType = default;

            return new ZoneDto
            {
                // Propiedades heredadas de BaseDesignerDto
                Id = zone.Id,
                CanvasObjectType = "Station",
                Name = zone.Name,
                X = (float)(zone.xInit ?? 0),
                Y = (float)(zone.yInit ?? 0),
                Height = (float)(zone.yEnd ?? 0),
                Width = (float)(zone.xEnd ?? 0),
                MaxEquipments = zone.MaxEquipments ?? 0,
                StatusObject = string.Empty,
                LayoutId = itemArea.Layout?.Id ?? Guid.Empty,
                Layout = null,

                // Propiedades específicas de ZoneDto
                ZoneType = zoneType,
                MaxStockToBook = zone.MaxStockToBook,
                IsLimitStock = zone.IsLimitStock,
                MaxStock = zone.MaxStock,
                MaxContainers = zone.MaxContainers ?? 0,
                InitStockToBook = zone.InitStockToBook,
                AreaId = zone.AreaId,
                Area = itemArea,
                ViewPort = zone.ViewPort
            };
        }
        public static ZoneDto NewViewPortZonaGrid()
        {
            return new ZoneDto
            {
                CanvasObjectType = "Station",
                X = 0,
                Y = 0,
                Height = 0,
                Width = 0,
                StatusObject = "Update",
            };
        }


    }
}
