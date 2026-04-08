namespace Mss.WorkForce.Code.Models.Models
{
    public class WFMLaborWorkerPerFlow
    {
        public Guid Id { get; set; }
        public Guid WorkerId { get; set; }
        public Worker Worker { get; set; }
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public bool IsOutbound { get; set; }
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid WFMLaborId { get; set; }
        public WFMLaborWorker WFMLabor { get; set; }
        public double WorkTime { get; set; }
        public double Efficiency { get; set; }
        public double Productivity { get; set; }
        public double Utility { get; set; }
        public double TotalProductivity { get; set; }
        public double TotalUtility { get; set; }
        public int Breaks { get; set; }
        public double Ranking { get; set; }
        public int TotalOrders { get; set; }
        public int ClosedOrders { get; set; }
        public int TotalProcesses { get; set; }
        public int ClosedProcesses { get; set; }
        public double Progress { get; set; }
    }
}