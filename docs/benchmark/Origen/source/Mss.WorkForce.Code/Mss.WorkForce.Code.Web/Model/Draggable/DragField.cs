namespace Mss.WorkForce.Code.Web.Model
{
    public class DragField
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public bool HasItems => Items?.Any() == false;
        public List<DragSubItem> Items { get; set; } = new();
        public int Priority { get; set; }
        public bool IsActive { get; set; } =false;

    }
}
