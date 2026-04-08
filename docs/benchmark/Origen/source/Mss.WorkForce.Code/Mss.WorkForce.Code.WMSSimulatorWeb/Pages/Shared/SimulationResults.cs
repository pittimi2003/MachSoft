using Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Mss.WorkForce.Code.WMSSimulator.WMSModel
{
    public class SimulationResults
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string? ClientCode { get; set; }
        public string? ProviderCode { get; set; }
        public string TypeCode { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime? ExpeditionEstimatedDate { get; set; }
        public DateTime? ArrivalEstimatedDate { get; set; }
        public DateTime LiberationDate { get; set; }
        public DateTime? DownloadDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? RealTruckArrivalDate { get; set; }
        public string AssignedDockCode { get; set; }
        public string ActionCode { get; set; }
        public Guid PlanningId { get; set; }
        [NotMapped]
        public List<Processes> processes { get; set; }
    }
}
