using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class AlertResponse : IFillable
    {
        public Guid Id { get; set; }
        public Guid AlertId { get; set; }
        public Alert Alert { get; set; }
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid EntityId { get; set; }
        public DateTime TriggerDate { get; set; }
        public AlertSeverity Severity { get; set; }
        public AlertType Type { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Alert = context.Alerts.FirstOrDefault(x => x.Id == AlertId)!;
            this.Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
        }
    }
}
