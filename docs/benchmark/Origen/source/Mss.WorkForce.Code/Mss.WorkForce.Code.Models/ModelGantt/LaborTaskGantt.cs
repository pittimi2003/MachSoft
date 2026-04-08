using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class LaborTaskGantt : GanttTaskBase
    {
        public LaborTaskGantt() { }

        public LaborTaskGantt(UserFormatOptions userFormat) : base(userFormat) { }

        public Guid WorkerId { get; set; }

        [DisplayAttributes(1, "Worker name", true, ComponentType.None, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.FirstLevel)]
        public string WorkerName { get; set; }

        [DisplayAttributes(7, "Activity", true, ComponentType.None, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? ActivityTitle { get; set; }       // Reception, Putaway

        [DisplayAttributes(8, "Shift name", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string ShiftName { get; set; }

        [DisplayAttributes(9, "Role name", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string RolName { get; set; }

        [DisplayAttributes(10, "Team", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string? TeamName { get; set; }

        [DisplayAttributes(11, "Productivity (%)", true, ComponentType.None, "", GroupTypes.None, false, false, true)]
        public string ProductivityFormatted => Productivity.ToUserNumber(_userFormat);

        [DisplayAttributes(12, "Closed orders", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.SecondLevel)]
        public int? ClosedOrders { get; set; }

        [DisplayAttributes(13, "Total orders", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.SecondLevel)]
        public int? TotalOrders { get; set; }

        [DisplayAttributes(14, "Shift init hour", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string? ShiftInitHour { get; set; }

        [DisplayAttributes(15, "Shift end hour", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string? ShiftEndHour { get; set; }

        [DisplayAttributes(16, "Work time (HR)", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public string? WorkTime { get; set; }

        [DisplayAttributes(17, "Efficiency (%)", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.SecondLevel)]
        public string EfficiencyFormatted => Efficiency.ToUserNumber(_userFormat);

        [DisplayAttributes(18, "Utility (%)", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string UtilityFormatted => Utility.ToUserNumber(_userFormat);

        [DisplayAttributes(19, "Breaks", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.SecondLevel)]
         public string? Breaks { get; set; }

        [DisplayAttributes(22, "Committed time", true, ComponentType.DateTime, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.SecondLevel)]
        public override string CommittedHour { get => base.CommittedHour; }

        [DisplayAttributes(23, "Total activities", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public int? TotalActivities { get; set; }

        [DisplayAttributes(24, "Closed activities", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public int? ClosedActivities { get; set; }

        public double Utility { get; set; }
        public double Efficiency { get; set; }
        public double? Productivity { get; set; }
        public double? TotalProductivity { get; set; }

        [DisplayAttributes(25, "Total Productivity (%)", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string TotalProductivityFormatted => TotalProductivity.ToUserNumber(_userFormat);

        //No esta en el modelo
        //[DisplayAttributes(17, "Total Efficiency (%)", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        //public double TotalEfficiency { get; set; }

        public List<BreaksGantt> breaksGantts { get; set; }

		public string? StartTime { get; set; }
        

    }
}
