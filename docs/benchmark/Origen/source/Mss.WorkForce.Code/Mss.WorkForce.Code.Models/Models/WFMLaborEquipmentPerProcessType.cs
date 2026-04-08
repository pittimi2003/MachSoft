using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class WFMLaborEquipmentPerProcessType : IFillable
    {
        public Guid Id { get; set; }
        public Guid EquipmentGroupId { get; set; }
        public EquipmentGroup EquipmentGroup { get; set; }
        public Guid TypeEquipmentId { get; set; }
        public TypeEquipment TypeEquipment { get; set; }
        public int Equipments { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProcessType { get; set; }
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid WFMLaborEquipmentPerFlowId { get; set; }
        public WFMLaborEquipmentPerFlow WFMLaborEquipmentPerFlow { get; set; }
        public double WorkTime { get; set; }
        public double Efficiency { get; set; }
        public double Productivity { get; set; }
        public double Utility { get; set; }
        public double TotalProductivity { get; set; }
        public double TotalUtility { get; set; }
        public int TotalOrders { get; set; }
        public int ClosedOrders { get; set; }
        public int TotalProcesses { get; set; }
        public int ClosedProcesses { get; set; }
        public double Progress { get; set; }

        public void Fill(ApplicationDbContext context)
        {
            EquipmentGroup = context.EquipmentGroups.FirstOrDefault(x => x.Id == EquipmentGroupId)!;
            TypeEquipment = context.TypeEquipment.FirstOrDefault(x => x.Id == TypeEquipmentId)!;
            Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
        }
    }
}
