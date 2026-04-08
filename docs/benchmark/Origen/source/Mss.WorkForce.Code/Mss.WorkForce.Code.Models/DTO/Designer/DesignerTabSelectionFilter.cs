using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;

namespace Mss.WorkForce.Code.Models.DTO.Designer
{
    public class DesignerTabSelectionFilter<T>
    {
        public HashSet<T> Selected { get; set; } = new();

        public bool IsAll { get; set; }

        public void Toggle(T type)
        {
            if (!Selected.Add(type))
                Selected.Remove(type);
        }

        public void SelectAll(IEnumerable<T> all)
        {
            Selected = all.ToHashSet();
            IsAll = true;
        }

        public void Clear()
        {
            Selected.Clear();
            IsAll = false;
        }
    }

    public class SelectionState
    {
        public List<int> AreaTypeCodes { get; set; } = new();

        public List<Guid> FlowIds { get; set; } = new();

        public List<int> ProcessTypeCodes { get; set; } = new();

        public List<Guid> RouteIds { get; set; } = new();
    }

    public class CanvasSelectionRequest
    {
        public List<int> AreaTypeCodes { get; set; } = new();

        public object? Flows { get; set; } = new();

        public List<int> ProcessTypeCodes { get; set; } = new();

        public List<Guid> RouteIds { get; set; } = new();
    }
}
