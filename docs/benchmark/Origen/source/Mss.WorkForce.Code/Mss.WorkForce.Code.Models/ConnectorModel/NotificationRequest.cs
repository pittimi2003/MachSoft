using Mss.WorkForce.Code.Models.ConnectorModel.Enums;
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models.ConnectorModel
{
    public class NotificationRequest
    {
        [Required]
        public Guid NotificationId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationStatus Status { get; set; }

        [Required]
        public NotificationData Data { get; set; }
    }
}
