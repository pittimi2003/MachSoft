using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class YardDockUsagePerHour : IFillable
    {
        public Guid Id { get; set; }
        public DateTime InitHour { get; set; }
        public DateTime EndHour { get; set; }
        public Guid DockId { get; set; }
        public Dock Dock { get; set; }
        public bool AllowInbound { get; set; }
        public bool AllowOutbound { get; set; }
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
            Dock = context.Docks.FirstOrDefault(x => x.Id == DockId)!;
            Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
            Warehouse = context.Warehouses.FirstOrDefault(x => x.Id == WarehouseId)!;
        }
    }
}
