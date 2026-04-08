namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class PreviewData
    {
        public GanttDataConvertDto<TaskData> GanttData { get; set; }
        public List<VehicleMetricsDto> VehicleMetrics { get; set; }
        public List<WorkerWhatIf> WorkersDistribution { get; set; }
    }
}
