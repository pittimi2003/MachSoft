using Mss.WorkForce.Code.Models.SignalR;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface ISignalRClientService
    {

        #region Methods

        Task DisconnectAsync();

        Task InitConnectionAsync();

        Task SuscribeToReloadGantt(Func<GanttView, Task> handler);

        void UnsubscribeFromReloadGantt(Func<GanttView, Task> handler);

        #endregion

    }
}
