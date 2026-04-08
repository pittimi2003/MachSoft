
namespace Mss.WorkForce.Code.Models
{
    public class AppointmentMetrics
    {
        public int AppointmentType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ResourceId { get; set; }
        public bool Accepted { get; set; }
        public double TotalSaturation { get; set; }
        public double ActualUtilization { get; set; }
        public double PlannedUtilization { get; set; }
        public double TotalCapacity { get; set; }
   
    }
}
