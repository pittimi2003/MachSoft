using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Models.Attributtes;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class BreakDto : PenelEditorOperations, ICustomValidation
    {
        [DisplayAttributes(index: 0, caption: "Break profile", isVisible: true, required: true, fieldType: ComponentType.DropDown)]
        public CatalogEntity BreakProfile { get; set; } = new CatalogEntity(EntityNamesConst.BreakProfile);

        [DisplayAttributes(index: 2, caption: "Start", isVisible: true, required: false, fieldType: ComponentType.Time)]
        public TimeSpan InitBreak { get; set; }

        [DisplayAttributes(index: 3, caption: "End", isVisible: true, required: false, fieldType: ComponentType.Time)]
        public TimeSpan EndBreak { get; set; }

        [DisplayAttributes(index: 4, caption: "Paid", isVisible: true, required: true, fieldType: ComponentType.DropDownYesNo)]
        public bool? IsPaid { get; set; }

        [DisplayAttributes(index: 5, caption: "Required", isVisible: true, required: true, fieldType: ComponentType.DropDownYesNo)]
        public bool? IsRequiered { get; set; }

        public Guid BreakProfileId { get; set; }

        public ValidationResult CustomValidation()
        {
            if (InitBreak != default && EndBreak != default)
            {
                if (InitBreak > EndBreak)
                {
                    return new ValidationResult(
                        "* Start cannot be greater than End",
                        new[] { nameof(InitBreak), nameof(EndBreak) }
                    );
                }
            }

            return ValidationResult.Success;
        }
    }
}
