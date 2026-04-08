namespace Mss.WorkForce.Code.Models
{
    public class YardResourceFilter
    {
        public Guid EntityId { get; set; }
        public string Name { get; set; }
        public YardResourceType ResourceType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
