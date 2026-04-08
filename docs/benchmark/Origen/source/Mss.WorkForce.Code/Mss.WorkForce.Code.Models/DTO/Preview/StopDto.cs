namespace Mss.WorkForce.Code.Models.DTO.Preview
{
    public class StopDto
    {
        public string name { get; set; } = string.Empty;

        public Guid id { get; set; }

        public TimeSpan initHour { get; set; }
        public TimeSpan endHour { get; set; }

        public List<WorkerStops> workerStops { get; set; }

        public string CompleteName {
            get
            {
                return $"{name}  ({(int)initHour.TotalHours:00}:{initHour.Minutes:00} - {(int)endHour.TotalHours:00}:{endHour.Minutes:00})";
            }
        }
    }
}
