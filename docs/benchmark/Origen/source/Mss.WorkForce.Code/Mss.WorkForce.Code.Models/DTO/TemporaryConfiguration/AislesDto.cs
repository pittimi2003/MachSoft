using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class AislesDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Max MHE", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int MaxMHE { get; set; } = 0;

        [DisplayAttributes(2, "Additional time per unit entry", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int AdditionalTimePerUnitEntry { get; set; } = 0;

        [DisplayAttributes(3, "Additional time per unit exit", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int AdditionalTimePerUnitExit { get; set; } = 0;

        [DisplayAttributes(4, "Max tasks", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int MaxTasks { get; set; } = 0;

        [DisplayAttributes(5, "Aisle change time", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int AisleChangeTime { get; set; } = 0;

        [DisplayAttributes(6, "Narrow aisle", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool NarrowAisle { get; set; }

        [DisplayAttributes(7, "Max pickers", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int MaxPickers { get; set; } = 0;

        [DisplayAttributes(8, "End picking", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool EndPicking { get; set; }

        [DisplayAttributes(9, "Bidirectional", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool Bidirectional { get; set; }

        [DisplayAttributes(10, "Width per direction", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int WidthPerDirection { get; set; } = 0;

        [DisplayAttributes(11, "Max MHE pick or putaway", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int MaxMHEPickOrPutaway { get; set; } = 0;

        [DisplayAttributes(9, "Replenishment control", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool ReplenishmentControl { get; set; }

        [DisplayAttributes(4, "Max movement ", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int MaxMovement { get; set; } = 0;

        [DisplayAttributes(2, "Zone", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required StationCatalogueDto Station { get; set; }


        public static AislesDto NewDto()
        {
            return new AislesDto
            {
                Id = Guid.NewGuid(),
                Station = StationCatalogueDto.NewDto(),
            };
        }
    }
}
