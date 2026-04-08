namespace Mss.WorkForce.Code.Models.Models
{
    public class WFMLaborEquipmentPerFlow
    {
        public Guid Id { get; set; }
        public Guid EquipmentGroupId { get; set; }
        public EquipmentGroup EquipmentGroup { get; set; }
        public Guid TypeEquipmentId { get; set; }
        public TypeEquipment TypeEquipment { get; set; }
        public bool IsOutbound { get; set; }
        public int Equipments { get; set; }
        public DateTime InitDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid WFMLaborEquipmentId { get; set; }
        public WFMLaborEquipment WFMLaborEquipment { get; set; }
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
    }
}
