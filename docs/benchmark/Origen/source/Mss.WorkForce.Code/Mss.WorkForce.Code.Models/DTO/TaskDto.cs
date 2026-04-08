namespace Mss.WorkForce.Code.Models.DTO
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Progress { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
