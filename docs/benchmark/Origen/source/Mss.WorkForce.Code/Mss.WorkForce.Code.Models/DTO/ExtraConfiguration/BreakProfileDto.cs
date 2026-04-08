using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class BreakProfileDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }
        [DisplayAttributes(0, "Name", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Name { get; set; }
        [DisplayAttributes(1, "Type", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Type { get; set; }
        [DisplayAttributes(2, "Allow inbound flow", true, ComponentType.CheckBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public bool AllowInInboundFlow { get; set; }
        [DisplayAttributes(3, "Allow outbound flow", true, ComponentType.CheckBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public bool AllowInOutboundFlow { get; set; }
        [DisplayAttributes(index: 4, caption: "Warehouse", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionWarehouseDto? SelectionWarehouseDto { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static BreakProfileDto? NewDto()
        {
            return new BreakProfileDto
            {
                SelectionWarehouseDto = new SelectionWarehouseDto(),
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Type = string.Empty,
                AllowInInboundFlow = false,
                AllowInOutboundFlow = false,
            };
        }
    }
}
