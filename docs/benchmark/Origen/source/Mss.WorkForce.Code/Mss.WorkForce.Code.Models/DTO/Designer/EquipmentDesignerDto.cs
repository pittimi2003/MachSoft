using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class EquipmentDesignerDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string Name { get; set; }

        [Range(1, 99, ErrorMessageResourceName = "THENEQUIPMENTMUSTBEBETWEEN0AND99", ErrorMessageResourceType = typeof(ValidationResources))]
        public int NEquipment { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public Guid? TypeEquipmentId { get; set; }

        [JsonPropertyName("TypeName")]
        public string? NameTypeEquipment { get; set; }

        [JsonIgnore]
        public TypeEquipment? TypeEquipment { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public Guid? AreaId { get; set; }

        [JsonIgnore]
        public AreaDto? Area { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Ignore in Serialize and Not ignore in Deserialize
        public Guid? IdOriginal { get; set; }

        public string? StatusObject { get; set; }
    }

    public class EquipmentDesignerModalDto
    {
        public Guid? Id { get; set; }
        public int NEquipment { get; set; }

        public Guid? TypeEquipmentId { get; set; }

        [JsonIgnore]
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public TypeEquipment? TypeEquipment { get; set; }

        public Guid? AreaId { get; set; }

        [JsonIgnore]
        public AreaDto? Area { get; set; }
    }
}
