using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Mss.WorkForce.Code.Models.Resources;

namespace Mss.WorkForce.Code.Models.DTO.Designer.LocationZones
{
    public class DriveInDto : BaseDesignerDto
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

        public LocationTypes LocationTypes { get; set; }

        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public OrientationType Orientation { get; set; } = OrientationType.None;

        public Guid AreaId { get; set; }

        public static DriveInDto NewDto(ZoneDto zoneDto)
        {
            return new DriveInDto
            {
                Id = Guid.NewGuid(),
                CanvasObjectType = "DriveIn",
                ZoneId = zoneDto.Id ?? Guid.Empty,
                Zone = zoneDto,
                ViewPort = string.Empty,
                NumCrossAisles = 0,
                NumShelves = 0,
                IsVertical = false,
                Bidirectional = false,
                MaxPickers = 0,
                NarrowAisle = false,
                LocationTypes = LocationTypes.DriveIn,
                AreaId = zoneDto.AreaId,
                StatusObject = "StoredInBase",
                Name = zoneDto.Name,
                Orientation = OrientationType.Horizontal
            };
        }

        public static List<DriveInDto> NewDtoList(string Name, int Quantity, ObjectTypes objectTypes, AreaDto areaDto, bool isVertical, int NumShelves, int NumCrossAisles, int Capacity, OrientationType? orientation)
        {
            List<DriveInDto> driveInDtos = new();

            for (int i = 0; i < Quantity; i++)
            {
                Designer.ZoneDto zone = ZoneDto.NewDto(areaDto, objectTypes, Name, orientation);
                var driveIn = new DriveInDto
                {
                    Id = Guid.NewGuid(),
                    CanvasObjectType = "DriveIn",
                    ZoneId = zone.Id ?? Guid.Empty,
                    Zone = zone,
                    ViewPort = string.Empty,
                    NumCrossAisles = NumCrossAisles,
                    NumShelves = NumShelves,
                    IsVertical = isVertical,
                    Bidirectional = false,
                    MaxPickers = 0,
                    NarrowAisle = false,
                    LocationTypes = LocationTypes.DriveIn,
                    AreaId = areaDto.Id ?? Guid.Empty,
                    StatusObject = "Update",
                    Name = zone.Name,
                    Orientation = orientation ?? OrientationType.None
                };

                driveInDtos.Add(driveIn);
            }
            return driveInDtos;
        }

        public static DriveInDto NewViewportDriveInDto()
        {
            return new DriveInDto
            {
                Id = Guid.NewGuid(),
                CanvasObjectType = "DriveIn",
                NumCrossAisles = 0,
                NumShelves = 1,
                IsVertical = false,
                Bidirectional = false,
                MaxPickers = 0,
                NarrowAisle = false,
                LocationTypes = LocationTypes.DriveIn,
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
