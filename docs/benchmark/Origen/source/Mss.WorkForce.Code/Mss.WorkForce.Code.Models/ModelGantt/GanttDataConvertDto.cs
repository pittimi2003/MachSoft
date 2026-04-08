using System.Text.Json.Serialization;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class GanttDataConvertDto<T> where T : GanttTaskBase
    {
        public Guid PlanningId { get; set; }

        public List<ItemPlanning> itemsPlanning { get; set; } = new ();
		public List<WarehouseProcessPlanning> warehouseProcessPlanning { get; set; } = new();
        public PlanningData PlanningData { get; set; } = new();

		[JsonPropertyName("TaskGantt")]
        public List<T> TaskGantt { get; set; } = new();

        [JsonPropertyName("DependenciesGantt")]
        public List<DependenciesGantt> DependenciesGantt { get; set; } = new();

        [JsonPropertyName("ResourcesGantt")]
        public List<ResourcesGantt> ResourcesGantt { get; set; } = new();

        [JsonPropertyName("ResourcesAssignmentsGantt")]
        public List<ResourcesAssignmentsGantt> ResourcesAssignmentsGantt { get; set; } = new();

    }
}
