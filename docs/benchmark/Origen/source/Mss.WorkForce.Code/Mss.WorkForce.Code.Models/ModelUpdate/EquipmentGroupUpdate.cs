using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.ModelUpdate
{
    public class EquipmentGroupUpdate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? Equipments { get; set; }
        public Guid? TypeEquipmentId { get; set; }
        public Guid? AreaId { get; set; }
    }
}
