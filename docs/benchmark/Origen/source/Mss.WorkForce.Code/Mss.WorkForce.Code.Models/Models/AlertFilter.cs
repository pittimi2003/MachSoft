using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class AlertFilter : IFillable
    {
        public Guid Id { get; set; }
        public Guid AlertId { get; set; }
        public Alert Alert { get; set; }
        public AlertOperator Operator { get; set; }
        public string FilterField { get; set; }
        public string? FilterReference { get; set; }
        public string? FilterFixedValue { get; set; }
        public bool IsFixed { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Alert = context.Alerts.FirstOrDefault(x => x.Id == AlertId)!;
        }
    }
}
