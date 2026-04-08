using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer.LocationZones
{
    public class ChaoticStorageDto : BaseDesignerDto
    {
        public Guid ZoneId { get; set; }

        [JsonIgnore]
        public ZoneDto Zone { get; set; }

        [JsonIgnore]
        public string? ViewPort { get; set; }

        public LocationTypes LocationTypes { get; set; }

        [JsonIgnore]
        public OrientationType? Orientation { get; set; } = OrientationType.None;

        public Guid AreaId { get; set; }

        public static ChaoticStorageDto NewDto(ZoneDto zoneDto)
        {
            return new ChaoticStorageDto
            {
                Id = Guid.NewGuid(),
                CanvasObjectType = "ChaoticStorage",
                ZoneId = zoneDto.Id ?? Guid.Empty,
                Zone = zoneDto,
                StatusObject = "StoredInBase",
                ViewPort = string.Empty,
                LocationTypes = LocationTypes.CaoticStorage,
                AreaId = zoneDto.AreaId,
                Name = zoneDto.Name
            };
        }

        public static List<ChaoticStorageDto> NewDtoList(string Name, int Quantity, ObjectTypes objectTypes, AreaDto areaDto, bool isVertical, int NumShelves, int NumCrossAisles, int Capacity)
        {
            List<ChaoticStorageDto> chaoticStorageDtos = new();

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = ZoneDto.NewDto(areaDto, objectTypes, Name);
                var chaotic = new ChaoticStorageDto
                {
                    Id = Guid.NewGuid(),
                    CanvasObjectType = "ChaoticStorage",
                    ZoneId = zone.Id ?? Guid.Empty,
                    Zone = zone,
                    StatusObject= "Update",
                    ViewPort = string.Empty,
                    LocationTypes = LocationTypes.CaoticStorage,
                    AreaId = areaDto.Id ?? Guid.Empty,
                    Name = zone.Name
                };

                chaoticStorageDtos.Add(chaotic);
            }
            return chaoticStorageDtos;
        }
        public static ChaoticStorageDto NewViewPortDto()
        {
            return new ChaoticStorageDto
            {
                CanvasObjectType = "ChaoticStorage",
                StatusObject = "Update",
                LocationTypes = LocationTypes.CaoticStorage,
                X = 0,
                Y = 0,
                Height = 0,
                Width = 0
            };
        }
    }
}
