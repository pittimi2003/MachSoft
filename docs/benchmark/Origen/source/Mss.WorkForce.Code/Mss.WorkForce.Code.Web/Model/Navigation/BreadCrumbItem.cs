namespace Mss.WorkForce.Code.Web.Model
{
    public class BreadcrumbItem
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string? Url { get; set; }
        public bool IsActive { get; set; }
        public bool HasSubMenu { get; set; } = false;
        public string? TabKey { get; set; }

        //Propiedad unicamente para breadcrum de projects
        public string? ProjectName { get; set; }
        public List<BreadcrumbItem> SubMenuItems { get; set; } = new();
    }
}
