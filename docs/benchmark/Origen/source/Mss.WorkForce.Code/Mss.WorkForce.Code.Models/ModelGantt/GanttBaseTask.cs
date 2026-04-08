using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Interface;
using Mss.WorkForce.Code.Models.ModelGantt.DataGanttPlanning;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public abstract class GanttBaseTask 
    {
        public Guid id { get; set; }
        public Guid? parentId { get; set; }
        public string? title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public int progress { get; set; }
        public string color { get; set; }
        public int ActivityType { get; set; }
        public GanttTooltipType TooltipType { get; set; }
        public string ToolTip { get; set; }
        public List<SegmentDto>? segments { get; set; }
        public string BackgroundColorRow { get; set; }
		public bool FillProgress { get; set; } = false;
        public bool IsChildTask { get; set; }

        public bool isOnTime { get; set; } = true;
        public bool isOffOverlay { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndtDate { get; set; }
        public DateTime? CommintedDate { get; set; }
    }
}
