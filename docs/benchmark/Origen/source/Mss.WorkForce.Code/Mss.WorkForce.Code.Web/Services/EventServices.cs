
namespace Mss.WorkForce.Code.Web
{
    public class EventServices: IEventServices
    {
        private readonly Dictionary<string, Dictionary<string, List<Action<EventArguments>>>> _subscribers = new();


        public void Subscribe(string eventListen, string eventSender, Action<EventArguments> action)
        {
            if (!_subscribers.ContainsKey(eventListen))
            {
                _subscribers[eventListen] = new Dictionary<string, List<Action<EventArguments>>>();
            }

            if (!_subscribers[eventListen].ContainsKey(eventSender))
            {
                _subscribers[eventListen][eventSender] = new List<Action<EventArguments>>();
            }
            if (!_subscribers[eventListen][eventSender].Contains(action))
            {
                _subscribers[eventListen][eventSender].Add(action);
            }
        }

        // Método para desuscribirse de un evento
        public void Unsubscribe(string eventListen, string eventSender, Action<EventArguments> action)
        {
            if (_subscribers.ContainsKey(eventListen) && _subscribers[eventListen].ContainsKey(eventSender))
            {
                _subscribers[eventListen][eventSender].Remove(action);

                // Limpia las listas vacías
                if (_subscribers[eventListen][eventSender].Count == 0)
                {
                    _subscribers[eventListen].Remove(eventSender);
                }

                if (_subscribers[eventListen].Count == 0)
                {
                    _subscribers.Remove(eventListen);
                }
            }
        }

        public void Publish(string eventSender, EventArguments eventData = null)
        {
            if (!string.IsNullOrEmpty(eventSender))
            {
                foreach (var eventListen in _subscribers.Keys)
                {
                    if (_subscribers[eventListen].ContainsKey(eventSender))
                    {
                        foreach (var action in _subscribers[eventListen][eventSender])
                        {
                            action.Invoke(eventData);
                        }
                    }
                }
            }
        }

        public async Task PublishAsync(string eventSender, EventArguments eventData = null)
        {
            if (!string.IsNullOrEmpty(eventSender))
            {
                var tasks = new List<Task>();

                foreach (var eventListen in _subscribers.Keys)
                {
                    if (_subscribers[eventListen].ContainsKey(eventSender))
                    {
                        foreach (var action in _subscribers[eventListen][eventSender])
                        {
                            tasks.Add(Task.Run(() => action.Invoke(eventData)));
                        }
                    }
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
