namespace Mss.WorkForce.Code.Web.Model
{
    public class DragSubItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string IconClass { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
