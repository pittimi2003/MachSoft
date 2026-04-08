using Microsoft.AspNetCore.SignalR.Client;
using Mss.WorkForce.Code.Models.SignalR;

namespace Mss.WorkForce.Code.Web.Services
{
    public class SignalRClientService : ISignalRClientService
    {

        #region Fields

        private readonly List<Func<GanttView, Task>> _reloadGanttHandlers = new();
        private HubConnection? _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        #endregion

        public SignalRClientService(IConfiguration configuration, ILogger<SignalRClientService> logger)
        {
            _configuration = configuration;
              _logger = logger;
        }

        #region Methods

        public async Task DisconnectAsync()
        {
            if (_connection != null)
                await _connection.StopAsync();
        }

        public async Task InitConnectionAsync()
        {
            // Verifico si ya existe una conexión
            if (_connection != null && _connection.State == HubConnectionState.Connected)
                return;

            try
            {
                string baseUri = _configuration["ExternalServices:SignalRService"]?.TrimEnd('/') ?? string.Empty;
                string urlServer = $"{baseUri}{SignalRHubRoutes.GanttNotificationHub}";
           
                _logger.LogInformation($"SignalRClientService::InitConnectionAsync → Starting SignalR connection to server: {urlServer}");
                _connection = new HubConnectionBuilder()
                .WithUrl(urlServer)
                .WithAutomaticReconnect()
                .Build();

                _connection.On<string>(SignalREventNames.ReloadDataGantt, ganttViewString =>
                {
                    if (Enum.TryParse<GanttView>(ganttViewString, out var ganttView))
                    {
                        foreach (var handler in _reloadGanttHandlers)
                        {
                            handler(ganttView);
                        }
                    }
                });

                await _connection.StartAsync();
                _logger.LogInformation($"SignalRClientService::InitConnectionAsync → Connected to SignalR server at {urlServer}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalRClientService::InitConnectionAsync → Error establishing connection with SignalR server.");
            }
        }

        public async Task SuscribeToReloadGantt(Func<GanttView, Task> handler)
        {
            _reloadGanttHandlers.Add(handler);
        }

        public void UnsubscribeFromReloadGantt(Func<GanttView, Task> handler)
        {
            _reloadGanttHandlers.Remove(handler);
        }

        #endregion

    }
}
