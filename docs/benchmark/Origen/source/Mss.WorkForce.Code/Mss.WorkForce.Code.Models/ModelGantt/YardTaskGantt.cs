using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class YardTaskGantt : GanttTaskBase
    {
        public YardTaskGantt() { }

        public YardTaskGantt(UserFormatOptions userFormat) : base(userFormat) { }

        // For YardMetricsPerDock
        [DisplayAttributes(1, "Dock name", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string DockName { get; set; }

        [DisplayAttributes(2, "Process type", true, ComponentType.None, "", GroupTypes.None, false, true, true, traslateCaption : true)]
        public string ProcessType { get; set; }

        [DisplayAttributes(3, "Attended appointments", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public int? AttendedAppointments { get; set; }

        [DisplayAttributes(4, "Total appointments", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public int? TotalAppointments { get; set; }

        [DisplayAttributes(5, "Saturation (%)", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string SaturationFormatted => Saturation.ToUserNumber(_userFormat);

        // For YardMetricsAppointments
        [DisplayAttributes(6, "Vehicle type", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string VehicleType { get; set; }

        [DisplayAttributes(7, "Arrival", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string AppointmentDate { get; set; }

        [DisplayAttributes(8, "Trailer", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string VehicleCode { get; set; }

        [DisplayAttributes(9, "Appointment code", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string AppointmentCode { get; set; }

        [DisplayAttributes(10, "License", true, ComponentType.None, "", GroupTypes.None, false, true, true)]
        public string License { get; set; }

        [DisplayAttributes(11, "Customer", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string Customer { get; set; }

        [DisplayAttributes(12, "Yard code", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string YardCode { get; set; }

        [DisplayAttributes(13, "Total orders", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public int? TotalOrders { get; set; }

        [DisplayAttributes(14, "Completed orders", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public int? CompletedOrders { get; set; }

        [DisplayAttributes(15, "Utilization (%)", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string UtilizationFormatted => Utilization.ToUserNumber(_userFormat);

        public double? Utilization { get; set; }
        public double? Saturation { get; set; }
        public string? WorkFlow { get; set; }
        public int progress { get; set; }
        public string CommittedHour => base.CommittedHour;
    }
}
