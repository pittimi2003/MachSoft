using System.ComponentModel.DataAnnotations;
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class LayoutDto: IProject
    {
        [DisplayAttributes(required: true, isVisible: false, isVisibleDefault: false)]
        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(index: 1, caption: "Name", isVisible: true, required: true, isVisibleDefault: true, fieldType:ComponentType.TextBox)]
        public string Name { get; set; }

        [DisplayAttributes(index: 3, caption: "Height", isVisible: true, required: true, isVisibleDefault: false, fieldType: ComponentType.NumericSpin, minValue: 1, maxValue:16000, textAlignment: true)]
		public double HeightConvert { get; set; }

        [DisplayAttributes(index: 4, caption: "Width", isVisible: true, required: true, isVisibleDefault: false, fieldType: ComponentType.NumericSpin, minValue: 1, maxValue: 16000, textAlignment: true)]
		public double WidthConvert { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        [DisplayAttributes(index: 2, caption: "Warehouse", isVisible: true, required: true, isVisibleDefault: true, fieldType: ComponentType.DropDown)]
        public LayoutWarehouseDto? LayoutWarehouseDto { get; set; }

        public required string? Viewport { get; set; }

        [DisplayAttributes(index: 5, caption: "Creation date", isVisible: false, required: true, isVisibleDefault: true)]
        public DateTime CreationDate { get; set; }

        public static LayoutDto NewDto()
        {
            return new LayoutDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Height = 0,
                Width = 0,
                LayoutWarehouseDto = new LayoutWarehouseDto(),
                Viewport = string.Empty,
                CreationDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
            };
        }
    }


    public interface IProject { }
}
