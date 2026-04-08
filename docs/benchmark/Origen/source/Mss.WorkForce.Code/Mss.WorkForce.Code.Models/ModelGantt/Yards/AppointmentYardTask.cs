
namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class AppointmentYardTask : GanttBaseTask
    {
        public DateTime AppointmentDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Customer { get; set; }
        public string YardCode { get; set; }
        public string VehicleType { get; set; }
        public string License { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
    }
}
