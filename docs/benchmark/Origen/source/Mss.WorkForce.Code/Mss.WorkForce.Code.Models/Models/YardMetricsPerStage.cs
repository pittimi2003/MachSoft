using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.Models;

public class YardMetricsPerStage : IFillable
{
    public Guid Id { get; set; }
    public string ProcessType { get; set; }
    private DateTime startDate;
    public DateTime StartDate
    {
        get => startDate;
        set => startDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    private DateTime endDate;
    public DateTime EndDate
    {
        get => endDate;
        set => endDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    public Stage Stage { get; set; }
    public Guid StageId { get; set; }
    public int AttendedAppointments { get; set; }
    public int TotalAppointments { get; set; }
    public double Saturation { get; set; }
    public Planning Planning { get; set; }
    public Guid PlanningId { get; set; }

    public void Fill(ApplicationDbContext context)
    {
        this.Stage = context.Stages.FirstOrDefault(x => x.Id == StageId)!;
        this.Planning = context.Plannings.FirstOrDefault(x => x.Id == PlanningId)!;
    }
}