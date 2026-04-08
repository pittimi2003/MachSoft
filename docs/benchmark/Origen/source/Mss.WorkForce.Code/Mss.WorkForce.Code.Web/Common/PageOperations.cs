using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Web.Interfaces;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;
namespace Mss.WorkForce.Code.Web.Common
{
    public abstract class PageOperations : ComponentBase, IPageOperations
    {
        public bool IsDetailsVisible { get; set; } = false;
        public bool isExpandGrid { get; set; } = true;
        [Inject]
        public ILocalizationService l { get; set; }
        [Inject] public IInitialDataService _InitialDataService { get; set; }
        public UserFormatOptions userFormat { get; set; } = new();

        public Datauser? userData { get; set; }

        public virtual void CancelEvent()
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteEvent()
        {
            throw new NotImplementedException();
        }

        public virtual bool DetailsActions()
        {
            throw new NotImplementedException();
        }

        public virtual bool GridActions()
        {
            throw new NotImplementedException();
        }

        public virtual void LoadEvent()
        {
            throw new NotImplementedException();
        }

        public virtual void ReLoadEvent()
        {
            throw new NotImplementedException();
        }

        public virtual async Task SaveEvent()
        {
            throw new NotImplementedException();
        }

        public async Task GetUserDataAsync()
        {
            await _InitialDataService.GetDataUserLocal();
            userData = _InitialDataService.GetDatauser();
            userFormat = _InitialDataService.GetUserFormat();
        }

        public virtual bool ToolBarActionsNew(EventArguments eventArguments) => true;

        public virtual async Task<bool> ToolBarActionsDelete(EventArguments eventArguments) => true;
        public virtual bool ToolBarActionsUpdate(EventArguments eventArguments) => true;
        public virtual bool ToolBarActionsEdit(EventArguments eventArguments) => true;
        public virtual async Task<bool> ToolBarActionsClone(EventArguments eventArguments) => true;
        public virtual bool DetailsActionsCancel(EventArguments eventArguments) => true;
        public virtual bool ToolBarActionsDesigner(EventArguments eventArguments) => true;
        public virtual async Task<bool> ToolBarActionsDisabled(EventArguments eventArguments) => true;
        public virtual async Task<bool> ToolBarActionsChangePassword(EventArguments eventArguments) => true;
        public virtual async Task<bool> ToolBarActionsEnabled(EventArguments eventArguments) => true;
        public virtual async Task<bool> DetailsActionsUpdate(EventArguments eventArguments) => true;

        protected void RefreshState()
        {
            InvokeAsync(StateHasChanged);
        }

        protected abstract void LoadData();

        public virtual void ToolBarActions(EventArguments eventArguments)
        {
            switch (eventArguments.EventActions)
            {
                case EventActions.New:
                    ToolBarActionsNew(eventArguments);
                    break;
                case EventActions.Delete:
                    ToolBarActionsDelete(eventArguments);
                    break;
                case EventActions.Update:
                    ToolBarActionsUpdate(eventArguments);
                    break;
                case EventActions.Edit:
                    ToolBarActionsEdit(eventArguments);
                    break;
                case EventActions.Clone:
                    ToolBarActionsClone(eventArguments);
                    break;
                case EventActions.Designer:
                    ToolBarActionsDesigner(eventArguments);
                    break;
                case EventActions.Disabled:
                    ToolBarActionsDisabled(eventArguments);
                    break;
                case EventActions.Enabled:
                    ToolBarActionsEnabled(eventArguments);
                    break;
                case EventActions.ChangePassword:
                    ToolBarActionsChangePassword(eventArguments);
                    break;
                default:
                    break;
            }
            StateHasChanged();
        }

        public virtual void GridActions(EventArguments eventArguments)
        {
            switch (eventArguments.EventActions)
            {
                case EventActions.Selected:
                    GridActionsSelected(eventArguments);
                    break;
                case EventActions.MultiSelected:
                    GridActionsMultiSelected(eventArguments);
                    break;

                case EventActions.Refresh:
                    GridActionsRefresh(eventArguments);
                    break;

                case EventActions.CollapseGrid:
                    GridActionsCollapseGrid(eventArguments);
                    break;

                case EventActions.ExpandGrid:
                    GridActionsExpandGrid(eventArguments);
                    break;
            }
            StateHasChanged();
        }

        public virtual void DetailsAction(EventArguments eventArguments)
        {
            switch (eventArguments.EventActions)
            {
                case EventActions.Cancel:
                    DetailsActionsCancel(eventArguments);
                    break;
                case EventActions.Update:
                    DetailsActionsUpdate(eventArguments);
                    break;
                default:
                    break;
            }
        }

        public virtual bool GridActionsSelected(EventArguments eventArguments) => true;
        public virtual bool GridActionsMultiSelected(EventArguments eventArguments) => true;
        public virtual bool GridActionsRefresh(EventArguments eventArguments) => true;
        public virtual bool GridActionsCollapseGrid(EventArguments eventArguments) => true;
        public virtual bool GridActionsExpandGrid(EventArguments eventArguments) => true;

        public virtual async Task CloseRightPanel()
        {
            IsDetailsVisible = false;
            isExpandGrid = true;
            await InvokeAsync(StateHasChanged);
        }

        public string Loc(string key) => l.Loc(key);
        public string Loc(string key, params object[] p) => l.Loc(key,p);

    }
}
