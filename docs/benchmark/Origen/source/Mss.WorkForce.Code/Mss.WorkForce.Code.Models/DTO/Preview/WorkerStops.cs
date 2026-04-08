namespace Mss.WorkForce.Code.Models.DTO.Preview
{
    public class WorkerStops : ICloneable
    {
        public Guid workerId { get; set; }

        public string workerName { get; set; } = string.Empty;

        public string shiftName { get; set; } = string.Empty;

        public int workerNumber { get; set; }

        public TimeSpan init { get; set; }

        public TimeSpan end { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

    }
}
