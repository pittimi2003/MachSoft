using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class InboundFlowGraphDto
    {

        [Key]
        public Guid Id { get; set; }

        [DisplayAttributes(1, "Name", true, ComponentType.TextBox, isVisibleDefault: true)]
        [ValidationAttributes(ValidationType.AllExceptWhiteSpace)]
        public required string Name { get; set; }

        [DisplayAttributes(1, "Dock selection", required: false, ComponentType.TextBox, isVisibleDefault: false, isVisible: false)]
        public string? DockSelectionCode { get; set; }

        [DisplayAttributes(2, "Average items per order", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int AverageItemsPerOrder { get; set; }

        [DisplayAttributes(3, "Average lines per order", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public int AverageLinesPerOrder { get; set; }

        [DisplayAttributes(4, "Group", true, ComponentType.CheckBox, isVisibleDefault: true)]
        public bool Group { get; set; }
        public Guid WarehouseId { get; set; }

        [DisplayAttributes(5, "Warehouse", true, ComponentType.DropDown, isVisibleDefault: true)]
        public required SelectionWarehouseDto Warehouse { get; set; }

        [DisplayAttributes(6, "MaxVehicleTime", true, ComponentType.NumericSpin, isVisibleDefault: true)]
        public double MaxVehicleTime { get; set; }

        public static InboundFlowGraphDto NewDto()
        {
            return new InboundFlowGraphDto
            {
                Id = Guid.NewGuid(),
                Name= string.Empty,
                Warehouse = SelectionWarehouseDto.NewDto(),
            };
        }
    }
}
