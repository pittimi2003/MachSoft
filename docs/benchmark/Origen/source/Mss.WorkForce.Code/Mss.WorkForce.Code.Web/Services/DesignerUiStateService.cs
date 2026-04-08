using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public sealed class DesignerUiStateService
    {
        public DesignerSelectionContext Context { get; } = new();
        public void SetSelectedArea(Guid? areaId) => Context.SelectedAreaId = areaId;
        public Guid? GetSelectedArea() => Context.SelectedAreaId;
        public void SetTabsBaseState(SelectionState? state) => Context.TabsBaseState = state;
        public SelectionState? GetTabsBaseState() => Context.TabsBaseState;
        public void SetActiveTopTabIndex(int index) => Context.ActiveTopTabIndex = index;
        public int GetActiveTopTabIndex() => Context.ActiveTopTabIndex;
        public void SetSelectedProcess(Guid? id) => Context.SelectedProcessId = id;
    }
}
