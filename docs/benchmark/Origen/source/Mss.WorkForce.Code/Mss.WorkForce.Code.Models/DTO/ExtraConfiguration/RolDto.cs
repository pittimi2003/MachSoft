using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class RolDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }
        [DisplayAttributes(0, "Role name", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Name { get; set; }
        [DisplayAttributes(index: 1, caption: "Warehouse", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionWarehouseDto? SelectionWarehouseDto { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static RolDto? NewDto()
        {
            return new RolDto
            {
                SelectionWarehouseDto = new SelectionWarehouseDto(),
                Id = Guid.NewGuid(),
                Name = string.Empty,
            };
        }
    }
}
