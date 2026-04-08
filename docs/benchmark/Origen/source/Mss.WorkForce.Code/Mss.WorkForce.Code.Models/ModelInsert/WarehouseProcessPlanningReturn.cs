using Mss.WorkForce.Code.Models.Models;
using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.ModelInsert
{
	public class WarehouseProcessPlanningReturn
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid ProcessId { get; set; }
        public Process Process { get; set; }
        public DateTime? LimitDate { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public double WorkTime { get; set; }
        public bool IsStored { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsStarted { get; set; }
        public Guid PlanningId { get; set; }

        [JsonIgnore]
        public PlanningReturn Planning { get; set; }
        public Guid? WorkerId { get; set; }
        public Guid? EquipmentGroupId { get; set; }
    }
}
