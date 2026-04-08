using Mss.WorkForce.Code.Models.DTO.Designer;

namespace Mss.WorkForce.Code.Web.Model
{
    public sealed class DesignerSelectionContext
    {
        public Guid? SelectedAreaId { get; set; }
        public SelectionState? TabsBaseState { get; set; }
        public int ActiveTopTabIndex { get; set; } = 0;
        public Guid? SelectedProcessId { get; internal set; }
    }
}
