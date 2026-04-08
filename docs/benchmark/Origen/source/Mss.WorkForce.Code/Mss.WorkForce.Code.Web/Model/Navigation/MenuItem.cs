namespace Mss.WorkForce.Code.Web.Model
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public bool HasSubMenu { get; set; }
        public bool IsOpen { get; set; }
        public bool DependsOnWarehouse { get; set; }
        public List<MenuItem> SubMenuItems { get; set; } = new();
        public List<MenuItem> BreadcrumbChildren { get; set; } = new();
        public bool IsParent { get; set; } = false;
        public string? TabKey { get; set; }
        //Propiedad unicamente para breadcrum de projects
        public string? ProjectName { get; set; }

        public MenuItem Clone()
        {
            return new MenuItem
            {
                Name = this.Name,
                Icon = this.Icon,
                Url = this.Url,
                HasSubMenu = this.HasSubMenu,
                DependsOnWarehouse = this.DependsOnWarehouse,
                IsParent = this.IsParent,
                TabKey = this.TabKey,
                IsOpen = false,
                SubMenuItems = this.SubMenuItems?
                    .Select(sub => sub.Clone())
                    .ToList() ?? new List<MenuItem>()
            };
        }
    }
}
