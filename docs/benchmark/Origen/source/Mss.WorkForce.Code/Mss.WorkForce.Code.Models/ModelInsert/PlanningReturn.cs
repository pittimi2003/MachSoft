using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models.ModelInsert
{
    public class PlanningReturn : Planning
    {
        public List<WorkOrderPlanningReturn> WorkOrderPlanning { get; set; }
        public List<WarehouseProcessPlanningReturn> WarehouseProcessPlanning { get; set; }
    }
}
