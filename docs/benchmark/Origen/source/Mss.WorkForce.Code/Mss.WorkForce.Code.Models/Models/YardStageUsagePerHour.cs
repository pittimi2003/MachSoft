using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class YardStageUsagePerHour : IFillable
    {
        public Guid Id { get; set; }
        public DateTime InitHour { get; set; }
        public DateTime EndHour { get; set; }
        public Guid StageId { get; set; }
        public Stage Stage { get; set; }
        public double TotalCapacity { get; set; }
        public double RealUsage { get; set; }
        public double PlannedUsage { get; set; }
        public double Saturation { get; set; }
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            this.Stage = context.Stages.FirstOrDefault(x => x.Id == StageId)!;
            this.Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
            this.Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
    }
}
