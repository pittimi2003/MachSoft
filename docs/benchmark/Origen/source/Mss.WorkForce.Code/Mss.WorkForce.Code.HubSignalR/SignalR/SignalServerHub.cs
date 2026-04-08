using Microsoft.AspNetCore.SignalR;
using Mss.WorkForce.Code.Models.SignalR;

namespace Mss.WorkForce.Code.HubSignalR.SignalR
{
    public class SignalServerHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

      
        public async Task NotifyGanttReloadAsync(GanttView viewName)
        {
            await Clients.All.SendAsync(SignalREventNames.ReloadDataGantt, viewName.ToString());
        }
    }
}
