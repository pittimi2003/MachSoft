namespace Mss.WorkForce.Code.Web.Interfaces
{
    public interface IPageOperations
    {
        public Task SaveEvent();

        public void LoadEvent();

        public void ReLoadEvent();

        public void CancelEvent();

        public void DeleteEvent();

        public bool GridActions();

        public bool DetailsActions();

        Task GetUserDataAsync();

        public void ToolBarActions(EventArguments eventArguments);
        public void GridActions(EventArguments eventArguments);
        public void DetailsAction(EventArguments eventArguments);
        
        public  bool ToolBarActionsNew(EventArguments eventArguments);
        
        public  Task<bool> ToolBarActionsDelete(EventArguments eventArguments);

        public  bool ToolBarActionsUpdate(EventArguments eventArguments);

        public  bool ToolBarActionsEdit(EventArguments eventArguments);
        public Task<bool> ToolBarActionsClone(EventArguments eventArguments);
        public bool GridActionsSelected(EventArguments eventArguments);
        public bool GridActionsMultiSelected(EventArguments eventArguments);
        public bool GridActionsRefresh(EventArguments eventArguments);
        public bool GridActionsCollapseGrid(EventArguments eventArguments);
        public bool GridActionsExpandGrid(EventArguments eventArguments);
        public bool IsDetailsVisible { get; set; }
        public bool isExpandGrid { get; set; }

       public string Loc(string key);
       public string Loc(string key, params object[] p);

    }
}
