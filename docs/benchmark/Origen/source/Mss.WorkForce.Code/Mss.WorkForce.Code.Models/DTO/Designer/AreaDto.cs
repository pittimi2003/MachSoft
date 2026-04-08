using Mss.WorkForce.Code.Models.DTO.Designer;
using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class AreaDto : BaseDesignerDto
    {
        [JsonIgnore]
        public bool Inbound { get; set; }
        [JsonIgnore]
        public bool Outbound { get; set; }
        public AreaType? AreaType { get; set; }
        [JsonIgnore]
        public string AreaTypeName { get; set; }
        [JsonIgnore]
        public bool NarrowAisle { get; set; }
        [JsonIgnore]
        public bool IsAutomatic { get; set; }
        public Guid? AlternativeAreaId { get; set; }
        [JsonIgnore]
        public AreaDto? AlternativeArea { get; set; }
        [JsonIgnore]
        public TimeSpan DelayedTimePerUnit { get; set; }

        public static AreaDto NewDto()
        {
            return new AreaDto
            {
                Id = Guid.NewGuid(),
                CanvasObjectType = string.Empty,
                Name = string.Empty,
                X = 0,
                Y = 0,
                Inbound = false,
                Outbound = false,          
                AreaType = new(),
                AreaTypeName = string.Empty,
                NarrowAisle = false,
                IsAutomatic = false,
                AlternativeAreaId = Guid.Empty,
                AlternativeArea = new(),
                DelayedTimePerUnit = new(),
                LayoutId = Guid.Empty
            };
        }      
        public static AreaDto NewDto(AreaDto areaDto, LayoutDto layoutDto)
        {
            return new AreaDto
            {
                Id = Guid.NewGuid(),
                AreaTypeName = GetAreaTypeString((AreaType)areaDto.AreaType),
                CanvasObjectType = "Area",
                Name = areaDto.Name,
                Width = (float)(layoutDto.Width * 0.1),
                Height = (float)(layoutDto.Height * 0.1),
                X = areaDto.X, // If the position is larger than the size of the layout, it is moved within the layout.
                Y = areaDto.Y, // If the position is larger than the size of the layout, it is moved within the layout.
                StatusObject = "StoredInBase",
                Inbound = false,
                Outbound = false,
                AreaType = areaDto.AreaType,
                NarrowAisle = false,
                IsAutomatic = false,
                DelayedTimePerUnit = new(),
                LayoutId = layoutDto.Id,
                AlternativeAreaId = Guid.Empty,
                AlternativeArea = null,
            };
        }
        public static AreaDto NewViewPortAreaGrid()
        {
            return new AreaDto
            {
                CanvasObjectType = "Area",
                Name = string.Empty,
                X = 0,
                Y = 0,
                Height = 100,
                Width = 100,
                Inbound = false,
                Outbound = false,
                NarrowAisle = false,
                IsAutomatic = false,
                AlternativeAreaId = Guid.Empty,
                AlternativeArea = new(),
                DelayedTimePerUnit = new(),
                LayoutId = Guid.Empty,
                StatusObject = "Update",
            };
        }
    }
}
