using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class WFMLaborWorkerPerProcessType : IFillable
    {
        public Guid Id { get; set; }
        public Guid WorkerId { get; set; }
        public Worker Worker { get; set; }
        public Guid ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public string ProcessType { get; set; }
        public Guid PlanningId { get; set; }
        public Planning Planning { get; set; }
        public Guid WFMLaborPerFlowId { get; set; }
        public WFMLaborWorkerPerFlow WFMLaborPerFlow { get; set; }
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

        public void Fill(ApplicationDbContext context)
        {
            Worker = context.Workers.FirstOrDefault(x => x.Id == WorkerId)!;
            Schedule = context.Schedules.FirstOrDefault(x => x.Id == ScheduleId)!;
            Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
        }
    }
}