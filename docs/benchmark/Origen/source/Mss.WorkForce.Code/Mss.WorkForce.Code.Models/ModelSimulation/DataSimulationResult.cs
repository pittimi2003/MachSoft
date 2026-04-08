using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models.ModelSimulation
{
    public class BasicRequest()
    {
        public string? Json { get; set; }
    }

    public class DataResponseSimulation()
    {
        public PlanningReturn Planning { get; set; }
    }

    public class DBModelSimulationResult()
    {
        public Planning Planning { get; set; }
        public List<WorkOrderPlanning> WorkOrderPlanning { get; set; }
        public List<WarehouseProcessPlanning> WarehouseProcessPlanning { get; set; }
        public List<ItemPlanning> ItemPlanning { get; set; }
    }
}
