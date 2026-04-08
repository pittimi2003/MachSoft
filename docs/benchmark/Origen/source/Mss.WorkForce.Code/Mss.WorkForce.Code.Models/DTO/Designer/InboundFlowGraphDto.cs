using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class InboundFlowGraphDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessageResourceName = "THENAMEISREQUIRED", ErrorMessageResourceType = typeof(ValidationResources))]
        [StringLength(200, ErrorMessageResourceName = "THENAMECANNOTEXCEED200CHARACTERS", ErrorMessageResourceType = typeof(ValidationResources))]
        public string Name { get; set; }

        public DockSelectionStrategyDto? DockSelection { get; set; }

        [Required(ErrorMessageResourceName = "AVERAGEITEMSPERORDERISREQUIRED", ErrorMessageResourceType = typeof(ValidationResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHANOREQUALTO1", ErrorMessageResourceType = typeof(ValidationResources))]
        public int AverageItemsPerOrder { get; set; }

        [Required(ErrorMessageResourceName = "AVERAGENUMBEROFLINESPERORDERISREQUIRED", ErrorMessageResourceType = typeof(ValidationResources))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "MUSTBEGREATERTHANOREQUALTO1", ErrorMessageResourceType = typeof(ValidationResources))]
        public int AverageLinesPerOrder { get; set; }


        public bool? Group { get; set; }

        public Guid WarehouseId { get; set; }

        public WarehouseDto Warehouse { get; set; }

        public string ViewPort { get; set; }

        [Range(0, double.MaxValue, ErrorMessageResourceName = "VEHICLEMAXTIMEINDOCKMINMUSTBEGREATERTHANOREQUALTO0", ErrorMessageResourceType = typeof(ValidationResources))]
        public double MaxVehicleTime { get; set; }
        public Guid? DockSelectionStrategyId { get; set; }

        public FlowDto? FlowDto { get; set; }
    }
}
