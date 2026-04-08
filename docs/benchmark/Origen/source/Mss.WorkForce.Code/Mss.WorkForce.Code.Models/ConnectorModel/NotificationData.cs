using Mss.WorkForce.Code.Models.ConnectorModel.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.ConnectorModel
{
    public class NotificationData
    {
        public string? ProcessType { get; set; }

        [Required]
        public string Worker { get; set; } = string.Empty;

        [Required]
        public string EquipmentGroup { get; set; } = string.Empty;

        [Required]
        public DateTime InitDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public Guid InputOrder { get; set; }

        [Required]
        public bool IsStarted { get; set; }

        [Required]
        public NotificationStatus Status { get; set; }

        [Required]
        public bool IsOutbound { get; set; }

        [Required]
        public bool AllowPartialClosed { get; set; }

        [Required]
        public bool AllowGroup { get; set; }

        public string? AssignedDock { get; set; }

        public string? PreferredDock { get; set; }

        [Required]
        public bool IsEstimated { get; set; }

        public string? Warehouse { get; set; }
    }
}
