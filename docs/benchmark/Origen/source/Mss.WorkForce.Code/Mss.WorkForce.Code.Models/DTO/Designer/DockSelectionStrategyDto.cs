using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class DockSelectionStrategyDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public bool IsActive { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
