using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web
{
    public class EventArguments : EventArgs
    {
     

        public EventArguments(string eventOrigin, EventActions eventActions, object eventData) {
            this.EventOrigin = eventOrigin;
            this.EventActions = eventActions;
            this.EventActions = eventActions;
            this.EventData = eventData;
        }

        public   string EventOrigin { get;  } 
        public EventActions EventActions { get;  } 
        public   object EventData { get;  }

    }
}
