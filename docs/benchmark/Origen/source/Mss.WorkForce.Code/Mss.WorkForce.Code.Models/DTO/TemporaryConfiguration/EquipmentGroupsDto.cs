using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class EquipmentGroupsDto : PenelEditorOperations
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(2, "Equipments", true, ComponentType.NumericSpinInt, isVisibleDefault: true, textAlignment: true)]
        public int Equipments { get; set; }

        [DisplayAttributes(3, "Equipment type", true, ComponentType.DropDown, isVisibleDefault: true)]
        public CatalogEntity TypeEquipmentCatalogueNoFilteredDto { get; set; } = new CatalogEntity(EntityNamesConst.TypeEquipment);

        [DisplayAttributes(4, "Area", true, ComponentType.DropDown, isVisibleDefault: true)]
        public CatalogEntity Area { get; set; } = new CatalogEntity(EntityNamesConst.Area);

        public string? ViewPort { get; set; }

        public static EquipmentGroupsDto NewDto()
        {
            return new EquipmentGroupsDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                TypeEquipmentCatalogueNoFilteredDto = new CatalogEntity(EntityNamesConst.TypeEquipment),
                Area = new CatalogEntity(EntityNamesConst.Area)
            };
        }
    }
}
