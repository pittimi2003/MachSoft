using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class AreaDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }
        [DisplayAttributes(0, "Area name", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Name { get; set; }
        [DisplayAttributes(1, "Type", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Type { get; set; }
        [DisplayAttributes(2, "X init", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: false)]
        public double xInit { get; set; }
        [DisplayAttributes(3, "Y init", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: false)]
        public double yInit { get; set; }
        [DisplayAttributes(4, "X end", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: false)]
        public double xEnd { get; set; }
        [DisplayAttributes(5, "Y end", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: false)]
        public double yEnd { get; set; }
        [DisplayAttributes(6, "Alternative area", false, ComponentType.DropDown, "", GroupTypes.None, true, isVisibleDefault: false)]
        public SelectionAreaDto? SelectionAreaDto { get; set; }
        [DisplayAttributes(7, "Delayed time per unit", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: false)]
        public int DelayedTimePerUnit { get; set; }
        [DisplayAttributes(8, "Narrow aisle", true, ComponentType.CheckBox, "", GroupTypes.None, false, isVisibleDefault: false)]
        public bool NarrowAisle { get; set; }
        [DisplayAttributes(9, "Is automatic", true, ComponentType.CheckBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public bool IsAutomatic { get; set; }
        [DisplayAttributes(10, "Layout", true, ComponentType.DropDown, "", GroupTypes.None, false, isVisibleDefault: true)]
        public SelectionLayoutDto SelectionLayoutDto { get; set; }

        [DisplayAttributes(11, "View port", false, ComponentType.None, "", GroupTypes.None, false, isVisibleDefault: false)]
        public string ViewPort { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public static AreaDto? NewDto()
        {
            return new AreaDto
            {
                SelectionLayoutDto = new SelectionLayoutDto(),
                xInit = 0,
                yInit = 0,
                yEnd = 0,
                xEnd = 0,
                Type = string.Empty,
                SelectionAreaDto = new SelectionAreaDto(),
                DelayedTimePerUnit = 0,
                Id = Guid.NewGuid(),
                IsAutomatic = false,
                Name = string.Empty,
                NarrowAisle = false,
                ViewPort = string.Empty,
            };
        }
    }
}
