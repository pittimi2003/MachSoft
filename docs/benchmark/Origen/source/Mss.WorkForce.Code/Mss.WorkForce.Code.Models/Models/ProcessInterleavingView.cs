namespace Mss.WorkForce.Code.Models.Models
{
    public class ProcessInterleavingView
    {
        private DateTime when;
        public DateTime When
        {
            get => when;
            set => when = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public double InboundSec { get; set; }
        public double OutboundSec { get; set; }
        public double InterleavingSec { get; set; }

    }
}
