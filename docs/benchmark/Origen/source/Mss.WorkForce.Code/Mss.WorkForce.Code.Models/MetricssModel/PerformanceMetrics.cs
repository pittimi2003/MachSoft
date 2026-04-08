namespace Mss.WorkForce.Code.Models.MetricssModel
{
    public class PerformanceMetrics
    {
        public Guid Id { get; set; }
        public string Organization {  get; set; }

        public DateTime When { get; set; }

        public DateTime Date { get; set; }

        public string Site { get; set; }

        public string Layout { get; set; }

        public double NumberOfWorkers { get; set; }

        public double NumberOfOrders { get; set; }

        public double NumberOfLines { get; set; }

        public double SimulationTime { get; set; }

        public double NumberOfSimulations { get; set; }

    }
}
