using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class ZoneDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        [ValidationAttributes(ValidationType.AllExceptWhiteSpace)]
        public required string Name { get; set; }

        [DisplayAttributes(2, "Type", true, ComponentType.TextBox, isVisibleDefault: true)]
        public required string Type { get; set; }

        [DisplayAttributes(3, "X init", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double xInit { get; set; }

        [DisplayAttributes(4, "Y init", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double yInit { get; set; }

        [DisplayAttributes(5, "X end", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double xEnd { get; set; }

        [DisplayAttributes(6, "Y end", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double yEnd { get; set; }

        [DisplayAttributes(7, "Max stock book", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double MaxStockToBook { get; set; }

        [DisplayAttributes(8, "Is limitStock", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool IsLimitStock { get; set; }

        [DisplayAttributes(9, "Max stock", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double MaxStock { get; set; }

        [DisplayAttributes(10, "Max containers", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int MaxContainers { get; set; }

        [DisplayAttributes(11, "Init stock  book", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double InitStockToBook { get; set; }
        public Guid AreaId { get; set; }

        [DisplayAttributes(12, "Area", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required AreasCatalogueDto Area { get; set; }

        public static ZoneDto NewDto()
        {
            return new ZoneDto
            {
                Id = Guid.NewGuid(),
                Name= string.Empty,
                Type= string.Empty,
                Area = AreasCatalogueDto.NewDto(),
            };
        }
    }
}
