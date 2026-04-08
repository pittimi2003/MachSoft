using Mss.WorkForce.Code.Models.Models;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.ModelInsert
{
	public class ItemPlanningReturn
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public Process Process { get; set; }
        public bool IsOutbound { get; set; }
        public DateTime LimitDate { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public double WorkTime { get; set; }
        public bool IsStored { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsStarted { get; set; }
        public Guid WorkOrderPlanningId { get; set; }
        [JsonIgnore]
        public WorkOrderPlanningReturn WorkOrderPlanning { get; set; }
        public Guid? WorkerId { get; set; }
        public Guid? EquipmentGroupId { get; set; }
        public double Progress { get; set; } = 0;
        public Guid? ShiftId { get; set; }
        public Shift? Shift { get; set; }
        public Guid? StageId { get; set; }
        public Stage? Stage { get; set; }
        public string? WorkerName { get; set; }
        public string? RolName { get; set; }
    }
}
