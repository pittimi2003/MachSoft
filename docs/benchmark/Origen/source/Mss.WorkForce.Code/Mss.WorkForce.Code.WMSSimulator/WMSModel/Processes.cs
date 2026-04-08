namespace Mss.WorkForce.Code.WMSSimulator.WMSModel
{
    public class Processes
    {
        public Guid Id { get; set; }
        public Guid SimulationResultsId { get; set; }
        public string UserCode { get; set; }
        public string EquipmentTypeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProcessType { get; set; }
    }
}
