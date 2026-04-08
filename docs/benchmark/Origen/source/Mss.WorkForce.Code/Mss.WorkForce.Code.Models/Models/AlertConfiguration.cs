using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class AlertConfiguration : IFillable
    {
        public Guid Id { get; set; }
        public Guid AlertId { get; set; }
        public Alert Alert { get; set; }
        public AlertSeverity Severity { get; set; }
        public AlertType Type { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            Alert = context.Alerts.FirstOrDefault(x => x.Id == AlertId)!;
        }
    }
}