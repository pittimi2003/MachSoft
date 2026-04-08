using Mss.WorkForce.Code.Models.Models;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.ModelInsert
{
	public class WorkOrderPlanningReturn
    {
        public Guid Id { get; set; }
        public bool IsOutbound { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public double WorkTime { get; set; }
        public bool IsEstimated { get; set; }
        public bool IsStored { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsStarted { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public double Progress { get; set; }
        public Guid PlanningId { get; set; }
        [JsonIgnore]
        public PlanningReturn Planning { get; set; }
        public Guid? InputOrderId { get; set; }
        public InputOrder? InputOrder { get; set; }
        public Guid? AssignedDockId { get; set; }
        public Dock? AssignedDock { get; set; }
        public List<ItemPlanningReturn> ItemPlanning { get; set; }
        public bool IsOnTime { get; set; }
        public bool IsInVehicleTime { get; set; }
        public double? OrderDelay { get; set; }
        public DateTime SLATarget { get; set; }
        public bool SLAMet { get; set; }
        public DateTime? AppointmentEndDate { get; set; }
    }
}
