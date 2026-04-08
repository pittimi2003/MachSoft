using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class LaborEquipmentGantt : GanttTaskBase
    {
        public LaborEquipmentGantt() { }    

        public LaborEquipmentGantt(UserFormatOptions userFormat) : base(userFormat) { }

        // Labor equipments columns
        [DisplayAttributes(1, "Equipment type", true, ComponentType.None, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.FirstLevel)]
        public string TypeEquipmentName { get; set; }

        [DisplayAttributes(2, "Equipment group", true, ComponentType.None, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.FirstLevel)]
        public string EquipmentGroupName { get; set; }

        [DisplayAttributes(3, "Equipments", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.SecondLevel)]
        public int Equipments {  get; set; }

        [DisplayAttributes(6, "Work time (HR)", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public string? WorkTime { get; set; }

        [DisplayAttributes(7, "Efficiency (%)", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.SecondLevel)]
        public string EfficiencyFormatted => Efficiency.ToUserNumber(_userFormat);

        [DisplayAttributes(8, "Productivity (%)", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string ProductivityFormatted => Productivity.ToUserNumber(_userFormat);

        [DisplayAttributes(9, "Total productivity (%)", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string TotalProductivityFormatted => TotalProductivity.ToUserNumber(_userFormat);

        [DisplayAttributes(10, "Utility (%)", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public string UtilityFormatted => Utility.ToUserNumber(_userFormat);

        [DisplayAttributes(11, "Total utility (%)", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.FirstLevel)]
        public string TotalUtilityFormatted => TotalUtility.ToUserNumber(_userFormat);

        [DisplayAttributes(12, "Total orders", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.SecondLevel)]
        public int? TotalOrders { get; set; }

        [DisplayAttributes(13, "Closed orders", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.SecondLevel)]
        public int? ClosedOrders { get; set; }

        [DisplayAttributes(12, "Total activities", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public int? TotalActivities { get; set; }

        [DisplayAttributes(13, "Closed activities", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public int? ClosedActivities { get; set; }

        [DisplayAttributes(5, "Activity", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string? ActivityTitle { get; set; }

        [DisplayAttributes(16, "Committed time", true, ComponentType.DateTime, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.SecondLevel)]
        public override string CommittedHour { get => base.CommittedHour; }

        public double? Efficiency { get; set; }
        public double Productivity { get; set; }
        public double? TotalProductivity { get; set; }
        public double? Utility { get; set; }
        public double? TotalUtility { get; set; }
    }
}
