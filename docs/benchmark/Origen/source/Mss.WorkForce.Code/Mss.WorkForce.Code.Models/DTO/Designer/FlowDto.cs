using Mss.WorkForce.Code.Models.Resources;
using System.ComponentModel.DataAnnotations;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class FlowDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "_MANDATORYFIELD", ErrorMessageResourceType = typeof(ValidationResources))]
        public FlowType? Type { get; set; }

        public DockSelectionStrategyDto SelectionTypeDock { get; set; }

        public Guid WarehouseId { get; set; }

        public WarehouseDto Warehouse { get; set; }
    }
}
