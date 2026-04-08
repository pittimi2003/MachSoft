using Mss.WorkForce.Code.Models.Enums;

namespace Mss.WorkForce.Code.Models
{
    public abstract class YardResourceBase
    {
        public int Id { get; set; }
        public Guid EntityId { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public bool IsGroup { get; set; } = true;
        public bool IsBlock { get; set; }
        public string BackgroundCss { get; set; }
        public string TextCss { get; set; }
        public bool HasAnyChild { get; set; } = false;
        public eRosourceType ResourceType { get; set; } = eRosourceType.None;
        public List<AppointmentMetrics> Appointments { get; set; } = new();
    }
}
