using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web.Model
{
    public class ActionItem
    {

        #region Properties

        public EventActions ActionType { get; set; }
        public string EventListener { get; set; }
        public string IconName { get; set; }
        public bool IsVisible { get; set; }
        public string Name { get; set; }

        #endregion

    }
}
