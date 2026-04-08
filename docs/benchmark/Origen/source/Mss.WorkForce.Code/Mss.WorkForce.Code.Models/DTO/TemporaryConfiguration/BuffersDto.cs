using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class BuffersDto
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Entry capacity", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int EntryCapacity { get; set; }

        [DisplayAttributes(2, "Exit capacity", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int ExitCapacity { get; set; }

        [DisplayAttributes(3, "Capacity by material", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool CapacityByMaterial { get; set; }

        [DisplayAttributes(4, "Excess", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool Excess { get; set; }

        [DisplayAttributes(5, "Max. equipment", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int MaxEquipments { get; set; }

        [DisplayAttributes(6, "Zone", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required StationCatalogueDto StationCatalogueDto { get; set; }


        public static BuffersDto NewDto()
        {
            return new BuffersDto
            {
                Id = Guid.NewGuid(),
                StationCatalogueDto = StationCatalogueDto.NewDto(),
            };
        }
    }
}
