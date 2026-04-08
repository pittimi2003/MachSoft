
namespace Mss.WorkForce.Code.Web
{
    public interface IEventServices
    {
        void Subscribe(string eventListen, string eventSender, Action<EventArguments> action);
        void Unsubscribe(string eventListen, string eventSender, Action<EventArguments> action);
        void Publish(string eventSender, EventArguments eventData = null);
        Task PublishAsync(string eventSender, EventArguments eventData = null);
    }
}
