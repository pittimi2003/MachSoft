using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class CommonShelfProperties
    {
        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public LocationTypes LocationTypes { get; set; }

        public Guid AreaId { get; set; }

        public AreaDto Area { get; set; }

        public int MaxContainers { get; set; }

        public int MaxEquipments { get; set; }
    }
}
