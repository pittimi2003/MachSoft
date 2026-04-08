
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models
{
    public class AlertMessageDto : ICloneable
    {
        public Guid Id { get; set; }    
        public Guid AlertId { get; set; }
        public string EntityCode { get; set; }
        public string Message { get; set; }
        public DateTime CreationDate { get; set; }
        public AlertType AlertType { get; set; }
        public AlertSeverity AlertSeverity { get; set; }
        public bool IsRead { get; set; }
        public Guid EntityId { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
