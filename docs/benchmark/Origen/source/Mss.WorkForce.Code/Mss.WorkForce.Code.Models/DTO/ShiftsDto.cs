using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class ShiftsDto : PenelEditorOperations, ICustomValidation
    {
        [DisplayAttributes(index: 2, caption: "Start", isVisible: true, required: false, fieldType: ComponentType.Time)]
        public TimeSpan InitHour { get; set; }

        [DisplayAttributes(index: 3, caption: "End", isVisible: true, required: false, fieldType: ComponentType.Time)]
        public TimeSpan EndHour { get; set; }
        public Guid WarehouseId { get; set; }
       
        public ValidationResult CustomValidation()
        {
            if (InitHour != default && EndHour != default)
            {
                if (InitHour > EndHour)
                {
                    return new ValidationResult(
                        "* Start cannot be greater than End",
                        new[] { nameof(InitHour), nameof(EndHour) }
                    );
                }
            }
            return ValidationResult.Success;
        }
    }
}
