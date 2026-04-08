namespace Mss.WorkForce.Code.Models.ModelGantt.DataGanttPlanning
{
    public class SegmentDto
    {
        public string start { get; set; }
        public string end { get; set; }
        public Guid id { get; set; }
        public int progress { get; set; }
        public string worktime { get; set; }

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndtDate { get; set; }
        public bool isOnTime { get; set; } = true;
        public bool isOffOverlay { get; set; } = true;

    }

}
