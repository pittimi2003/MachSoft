using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer.LocationZones
{
    public class AutomaticStorageDto : BaseDesignerDto
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

        public LocationTypes LocationTypes { get; set; }

        [JsonIgnore]
        public OrientationType Orientation { get; set; } = OrientationType.None;

        public Guid AreaId { get; set; }

        public static AutomaticStorageDto NewDto(ZoneDto zoneDto)
        {
            return new AutomaticStorageDto
            {
                Id = Guid.NewGuid(),
                CanvasObjectType = "AutomaticStorage",
                ZoneId = zoneDto.Id ?? Guid.Empty,
                Zone = zoneDto,
                ViewPort = string.Empty,
                NumCrossAisles = 0,
                NumShelves = 0,
                IsVertical = false,
                LocationTypes = LocationTypes.AutomaticStorage,
                AreaId = zoneDto.AreaId,
                StatusObject = "StoredInBase",
                Name = zoneDto.Name,
                Orientation = OrientationType.Horizontal
            };
        }

        public static List<AutomaticStorageDto> NewDtoList(string Name, int Quantity, ObjectTypes objectTypes, AreaDto areaDto, bool isVertical, int NumShelves, int NumCrossAisles, int Capacity, OrientationType? orientation)
        {
            List<AutomaticStorageDto> automaticStorageDtos = new();

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = ZoneDto.NewDto(areaDto, objectTypes, Name);
                var automaticStorage = new AutomaticStorageDto
                {
                    Id = Guid.NewGuid(),
                    CanvasObjectType = "AutomaticStorage",
                    ZoneId = zone.Id ?? Guid.Empty,
                    Zone = zone,
                    ViewPort = string.Empty,
                    NumCrossAisles = NumCrossAisles,
                    NumShelves = NumShelves,
                    IsVertical = isVertical,
                    LocationTypes = LocationTypes.AutomaticStorage,
                    AreaId = areaDto.Id ?? Guid.Empty,
                    StatusObject = "Update",
                    Name = zone.Name,
                    Orientation = orientation ?? OrientationType.None
                };
                automaticStorageDtos.Add(automaticStorage);
            }
            return automaticStorageDtos;
        }
        public static AutomaticStorageDto NewViewPortDto()
        {
            return new AutomaticStorageDto
            {
               
                CanvasObjectType = "AutomaticStorage",
                NumCrossAisles = 0,
                NumShelves = 0,
                IsVertical = false,
                LocationTypes = LocationTypes.AutomaticStorage,
                StatusObject = "Update",
                Orientation = OrientationType.Horizontal,
                X = 0,
                Y = 0,
                Height = 0,
                Width = 0
            };
        }
    }
}
