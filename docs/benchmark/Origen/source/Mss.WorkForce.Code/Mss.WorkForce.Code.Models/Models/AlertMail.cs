using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class AlertMail : IFillable
    {
        public Guid Id { get; set; }
        public Guid AlertId { get; set; }
        public Alert Alert { get; set; }
        public string? Subject { get; set; }
        public string Address { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Alert = context.Alerts.FirstOrDefault(x => x.Id == AlertId)!;
        }
    }
}
