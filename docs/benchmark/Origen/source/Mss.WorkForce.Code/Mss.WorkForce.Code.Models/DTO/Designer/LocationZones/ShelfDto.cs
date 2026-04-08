using System.Text.Json.Serialization;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer.LocationZones
{
    public class ShelfDto : BaseDesignerDto
    {
        public Guid? ZoneId { get; set; }

        public Guid AreaId { get; set; }

        public int? NumCrossAisles { get; set; }

        public int? NumShelves { get; set; }

        public bool IsVertical { get; set; }

        public LocationTypes LocationTypes { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Ignore in Serialize and Not ignore in Deserialize
        public Guid? IdOriginal { get; set; }
    }
}
