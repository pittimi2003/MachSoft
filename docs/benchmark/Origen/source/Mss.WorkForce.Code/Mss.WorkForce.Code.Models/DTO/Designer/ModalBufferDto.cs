using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class ModalBufferDto
    {
        public Guid Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int Quantity { get; set; }

        public int QuantityWidth { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string Name { get; set; }

        public int QuantityHeigth { get; set; }

        public Guid? AreaId { get; set; }

        public AreaDto? Area { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public BufferType? BufferType { get; set; }

        //[Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int NumShelves { get; set; }

        //[Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHAN0", ErrorMessageResourceType = typeof(ValidationResources))]
        public int NumCrossAisles { get; set; }

        //[Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public OrientationType? Orientation { get; set; }

        public bool IsVertical
        {
            get => Orientation == OrientationType.Vertical;
            set => Orientation = value ? OrientationType.Vertical : OrientationType.Horizontal;
        }
    }
}
