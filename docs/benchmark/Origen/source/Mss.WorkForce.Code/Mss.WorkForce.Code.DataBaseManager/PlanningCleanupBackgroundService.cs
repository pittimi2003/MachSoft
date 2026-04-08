namespace Mss.WorkForce.Code.DataBaseManager
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using System.Data.Common;

    public class PlanningCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PlanningCleanupBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _runTime; 

        public PlanningCleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PlanningCleanupBackgroundService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
            _runTime = new TimeSpan(_configuration.GetValue<int>("PlanningCleanupAt:Hours",3), _configuration.GetValue<int>("PlanningCleanupAt:Minutes", 0), _configuration.GetValue<int>("PlanningCleanupAt:Seconds", 0));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[PCBS] PlanningCleanupBackgroundService started.");

            try
            {
                await RunCleanup(stoppingToken, isStart: true);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "[PCBS] Fatal error during initial startup cleanup. Stopping background service.");
                return; 
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date + _runTime;

                if (nextRun <= now)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;

                _logger.LogInformation(
                    "[PCBS] Next cleanup scheduled at {NextRun} (in {Delay}).",
                    nextRun.ToString("dd/MM/yyyy HH:mm:ss"), delay
                );

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    await RunCleanup(stoppingToken);
                }

                catch (DbUpdateException ex)
                {
                    _logger.LogCritical(ex,
                        "[PCBS] Fatal DbUpdateException during daily cleanup. Stopping background service.");
                    break;
                }
                catch (DbException ex)
                {
                    _logger.LogCritical(ex,
                        "[PCBS] Fatal DbException during daily cleanup. Stopping background service.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex,
                        "[PCBS] Unexpected fatal error during daily cleanup. Stopping background service.");
                }
            }

            _logger.LogInformation("[PCBS] PlanningCleanupBackgroundService stopped.");
        }

        private async Task RunCleanup(CancellationToken token, bool isStart = false)
        {
            using var scope = _scopeFactory.CreateScope();
            var utils = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>();

            var threshold = DateTime.UtcNow.AddDays(-(Math.Max(_configuration.GetValue<int>("PlanningRetentionDays", 1), 1)));

            if (isStart)
            {
                _logger.LogInformation(
                    "[PCBS] Running initial startup cleanup. Threshold = {Threshold}.",
                    threshold.ToString("dd/MM/yyyy HH:mm:ss")
                );
            }

            else
            {
                _logger.LogInformation(
                    "[PCBS] Running daily cleanup. Threshold = {Threshold}.",
                    threshold.ToString("dd/MM/yyyy HH:mm:ss")
                );
            }

            var deleted = await utils.CleanupOldPlanningsAsync(threshold, token);

            if (deleted == 0)
            {
                _logger.LogInformation("[PCBS] No outdated plannings found.");
            }
            else
            {
                _logger.LogInformation("[PCBS] Cleanup finished. Deleted {Count} plannings.", deleted);
            }
        }

    }
}



