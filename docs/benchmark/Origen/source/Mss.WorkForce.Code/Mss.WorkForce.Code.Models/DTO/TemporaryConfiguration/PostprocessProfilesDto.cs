using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    public class PostprocessProfilesDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string Name { get; set; }

        [DisplayAttributes(2, "Type", true, ComponentType.TextBox, isVisibleDefault: true)]
        public string Type { get; set; }

        [DisplayAttributes(3, "In inbound flow", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool AllowInInboundFlow { get; set; }

        [DisplayAttributes(4, "Out inbound flow", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool AllowInOutboundFlow { get; set; }

        [DisplayAttributes(5, "Warehouse", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required SelectionWarehouseDto Warehouse { get; set; }


        public static PostprocessProfilesDto NewDto()
        {
            return new PostprocessProfilesDto
            {
                Id = Guid.NewGuid(),
                Warehouse = SelectionWarehouseDto.NewDto(),
            };
        }
    }
}
