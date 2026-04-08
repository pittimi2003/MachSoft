using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.ExtraConfiguration
{
    public class ProcessDto
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }
        [DisplayAttributes(0, "Name", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Name { get; set; }
        [DisplayAttributes(1, "Type", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        public string Type { get; set; }
        [DisplayAttributes(2, "Minimal time", true, ComponentType.NumericSpin, "", GroupTypes.None, false, isVisibleDefault: true)]
        public int MinTime { get; set; }
        [DisplayAttributes(3, "Preprocess time", true, ComponentType.NumericSpin, "", GroupTypes.None, false, isVisibleDefault: true)]
        public int PreprocessTime { get; set; }
        [DisplayAttributes(4, "Postprocess time", true, ComponentType.NumericSpin, "", GroupTypes.None, false, isVisibleDefault: true)]
        public int PostprocessTime { get; set; }
        [DisplayAttributes(5, "Is warehouse process", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsWarehouseProcess { get; set; }
        [DisplayAttributes(6, "Is outbound", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsOut { get; set; }
        [DisplayAttributes(7, "Is inbound", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsIn { get; set; }
        [DisplayAttributes(8, "Is init process", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsInitProcess { get; set; }
        [DisplayAttributes(9, "Percentage init process", true, ComponentType.NumericSpin, "", GroupTypes.None, false, isVisibleDefault: true)]
        public double PercentageInitProcess { get; set; }
        [DisplayAttributes(index: 10, caption: "Area", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public SelectionAreaDto? SelectionAreaDto { get; set; }

        public static ProcessDto? NewDto()
        {
            return new ProcessDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                SelectionAreaDto = new SelectionAreaDto(),
                IsIn = false,
                IsInitProcess = false,
                IsOut = false,
                PercentageInitProcess = 0,
                IsWarehouseProcess = false,
                MinTime = 0,
                PostprocessTime = 0,
                PreprocessTime = 0,
                Type = string.Empty,
            };
        }
    }
}
