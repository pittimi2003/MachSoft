using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class ShiftDto
    {

        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }
        [DisplayAttributes(0, "Role name", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Name { get; set; }
        [DisplayAttributes(3, "Warehouse", true, ComponentType.DropDown, "", GroupTypes.None, false, isVisibleDefault: true)]
        public SelectionWarehouseDto? SelectionWarehouseDto { get; set; }
        [DisplayAttributes(1, "Init hour", true, ComponentType.NumericSpin, "", GroupTypes.None, false, isVisibleDefault: true)]
        public double InitHour { get; set; }
        [DisplayAttributes(2, "End hour", true, ComponentType.NumericSpin, "", GroupTypes.None, false, isVisibleDefault: true)]
        public double EndHour { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static ShiftDto? NewDto()
        {
            return new ShiftDto
            {
                SelectionWarehouseDto = new SelectionWarehouseDto(),
                Id = Guid.NewGuid(),
                Name = string.Empty,
                InitHour = 0,
                EndHour = 0,
            };
        }
    }
}
