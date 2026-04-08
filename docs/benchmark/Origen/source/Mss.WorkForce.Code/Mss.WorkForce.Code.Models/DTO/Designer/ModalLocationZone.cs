using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class ModalLocationZone
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string? Name { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public LocationTypes? ZoneType { get; set; }

        public int NumCrossAisles { get; set; }

        public int NumShelves { get; set; }

        public bool IsVertical { get; set; }

        public int? Capacity { get; set; }

        public Guid? AreaId { get; set; }

        public AreaDto? Area { get; set; }

        public int Quantity { get; set; }

        public OrientationType? Orientation { get; set; } = OrientationType.None;
    }
}
