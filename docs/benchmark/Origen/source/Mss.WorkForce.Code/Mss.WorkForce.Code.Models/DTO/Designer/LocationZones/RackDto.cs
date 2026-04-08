using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer.LocationZones
{
    public class RackDto: BaseDesignerDto
    {
        public Guid ZoneId { get; set; }

        [JsonIgnore]
        public ZoneDto Zone { get; set; }

        [JsonIgnore]
        public string? ViewPort { get; set; }

        public int NumCrossAisles { get; set; }

        public int NumShelves { get; set; }

        public bool IsVertical
        {
            get => Orientation == OrientationType.Vertical;
            set => Orientation = value ? OrientationType.Vertical : OrientationType.Horizontal;
        }

        [JsonIgnore]
        public bool Bidirectional { get; set; }

        [JsonIgnore]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int MaxPickers { get; set; }

        [JsonIgnore]
        public bool NarrowAisle { get; set; }

        public string CanvasObjectType { get; set; }

        public Guid AreaId { get; set; }

        public int QuantityHeigth { get; set; }

        public int QuantityWidth { get; set; }

        public LocationTypes LocationTypes { get; set; }

        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public OrientationType Orientation { get; set; } = OrientationType.None;

        /// <summary>
        /// Inicializa una nueva instancia de RackDto con valores por defecto, incluyendo una Zone preinicializada.
        /// </summary>
        /// <param name="area">Instancia de AreaDto necesaria para crear la Zone asociada.</param>
        public static RackDto NewDto(ZoneDto zoneDto)
        {
            return new RackDto
            {
                Id = Guid.NewGuid(),
                CanvasObjectType = "Rack",
                ZoneId = zoneDto.Id ?? Guid.Empty,
                Zone = zoneDto,
                AreaId = zoneDto.AreaId,
                ViewPort = string.Empty,
                QuantityHeigth = 0,
                QuantityWidth = 0,
                IsVertical = false,
                NumShelves = 0,
                NumCrossAisles = 0,
                LocationTypes = LocationTypes.Rack,
                StatusObject = "StoredInBase",
                Name = zoneDto.Name,
                Orientation = OrientationType.Horizontal
            };
        }

        public static List<RackDto> NewDtoList(string Name, int Quantity, ObjectTypes objectTypes, AreaDto areaDto, int height, int width, bool isVertical, int NumShelves, int NumCrossAisles, int Capacity, LocationTypes locationTypes, OrientationType? orientation)
        {
            List<RackDto> rackDtos = new();

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = ZoneDto.NewDto(areaDto, objectTypes, Name, orientation);
                var aisle = new RackDto
                {
                    Id = Guid.NewGuid(),
                    CanvasObjectType = "Rack",
                    ZoneId = zone.Id ?? Guid.Empty,
                    Zone = zone,
                    AreaId = areaDto.Id ?? Guid.Empty,
                    ViewPort = string.Empty,
                    QuantityHeigth = height,
                    QuantityWidth = width,
                    IsVertical = isVertical,
                    NumShelves = NumShelves,
                    NumCrossAisles = NumCrossAisles,
                    LocationTypes = locationTypes,
                    StatusObject = "Update",
                    Name = zone.Name,
                    Orientation = orientation ?? OrientationType.None
                };

                rackDtos.Add(aisle);
            }
            return rackDtos;
        }
        public static RackDto NewViewportDto()
        {
            return new RackDto
            {
                CanvasObjectType = "Rack",
                QuantityHeigth = 0,
                QuantityWidth = 0,
                IsVertical = false,
                NumShelves = 1,
                NumCrossAisles = 0,
                LocationTypes = LocationTypes.Rack,
                StatusObject = "Update",
                Orientation = OrientationType.Horizontal,
                X = 0,
                Y =0,
                Height = 0,
                Width = 0
            };
        }
    }
}
