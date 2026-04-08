using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class PivotTaskData
    {
        #region Properties

        [DisplayAttributes(2, "Activity", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string? ActivityTitle { get; set; }

        [DisplayAttributes(17, "Carrier", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? Carrier { get; set; }

        [DisplayAttributes(6, "Committed time", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string CommittedHour { get; set; }

        [DisplayAttributes(11, "Customer", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? Customer { get; set; }

        //[DisplayAttributes(18, "Order delay", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? DelayedOrder { get; set; }

        [DisplayAttributes(12, "Dock", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? Dock { get; set; }

        [DisplayAttributes(20, "End date", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string end { get; set; }

        [DisplayAttributes(19, "Locked status", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? EndBlockDate { get; set; }

        [DisplayAttributes(1, "Flow", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string? Flow { get; set; }

        [DisplayAttributes(14, "Locked", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public bool isBlock { get; set; }

        [DisplayAttributes(21, "Order committed time", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string OrderCommittedHour { get; set; }

        [DisplayAttributes(10, "Order end date", false, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string OrderEnd { get; set; }

        [DisplayAttributes(10, "Order Progress (%)", false, ComponentType.None, "", GroupTypes.None, false, false, false)]
        [MeasureAttributes(Enums.MeasuresType.Percent)]
        public int OrderProgress { get; set; }

        [DisplayAttributes(8, "Order start date", false, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string OrderStart { get; set; }

        [DisplayAttributes(11, "Order status", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string OrderStatus { get; set; }

        [DisplayAttributes(5, "Priority", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string? Priority { get; set; }

        [DisplayAttributes(23, "Progress (%)", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        [MeasureAttributes(Enums.MeasuresType.Percent)]
        public int progress { get; set; }

        [DisplayAttributes(15, "Resource", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? Resource { get; set; }

        [DisplayAttributes(9, "Start date", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string start { get; set; }

        [DisplayAttributes(7, "Status", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string? Status { get; set; }

        [DisplayAttributes(16, "Supplier", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? Supplier { get; set; }

        [DisplayAttributes(3, "WMS code", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string? title { get; set; }

        [DisplayAttributes(13, "Trailer", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? Trailer { get; set; }

        [DisplayAttributes(4, "Total Orders", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public int PivotCount { get; set; }


        #region Campos SLA
        //[DisplayAttributes(23, "Order delay (min)", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        //public string? TimeOrderDelay { get; set; }

        //[DisplayAttributes(25, "Actual work time (min)", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        //public string? ActualWorkTime { get; set; }

        [DisplayAttributes(26, "SLA target time", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public string SLATargetTime { get; set; }

        [DisplayAttributes(27, "SLA met", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public string SLAMet { get; set; }


        [DisplayAttributes(29, "Shift", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public string Shift { get; set; }

        public string HandlingUnitType { get; set; }

        public string AssignedResources { get; set; }

        #endregion

        #region Total fields 
        [DisplayAttributes(30, "Order delay (min)", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public double? SumTimeOrderDelay { get; set; }

        [DisplayAttributes(31, "Actual work time (min)", true, ComponentType.None, "", GroupTypes.None, false, true, false)]
        public double? SumActualWorkTime { get; set; }

        #endregion

        #endregion
    }
}