using Mss.WorkForce.Code.WMSSimulator.Generator;
using Mss.WorkForce.Code.WMSSimulator.Helper;
using Mss.WorkForce.Code.WMSSimulator.Update;
using static System.Net.WebRequestMethods;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System;

namespace Mss.WorkForce.Code.WMSSimulator
{
    public interface IWorkForceTaskScraperDetector
    {
        void StopExecution();
        void PauseExecution();
        void ContinueExecution();
        void StartExecution();
        Task ForceUpdateAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Initializes background services
    /// </summary>
    public class WorkForceTaskScraperDetector : BackgroundService, IWorkForceTaskScraperDetector
    {
        private CancellationTokenSource? _cts;
        private Task? _executionTask;
        private readonly object _lock = new();
        private readonly ManualResetEventSlim _pauseEvent = new(true);

        public WorkForceTaskScraperDetector()
        {
            _cts = new CancellationTokenSource();
        }

        public void StopExecution()
        {
            lock (_lock)
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                    Console.WriteLine("[StopExecution] SGA has been stopped.");
                }
                else
                {
                    Console.WriteLine("[StopExecution] SGA is not running or already stopped.");
                }
            }
        }

        public void PauseExecution()
        {
            lock (_lock)
            {
                _pauseEvent.Reset();
                Console.WriteLine("[PauseExecution] SGA has been paused.");
            }
        }

        public void ContinueExecution()
        {
            lock (_lock)
            {
                _pauseEvent.Set();
                Console.WriteLine("[ContinueExecution] SGA has been continued.");
            }
        }

        public void StartExecution()
        {
            lock (_lock)
            {
                if (_executionTask == null || _executionTask.IsCompleted)
                {
                    _cts = new CancellationTokenSource();
                    _executionTask = Task.Run(() => ExecuteAsync(_cts.Token));
                    Console.WriteLine("[StartExecution] SGA has been restarted.");
                }
                else
                {
                    Console.WriteLine("[StartExecution] SGA is currently working, cannot start again.");
                }
            }
        }

        public async Task ForceUpdateAsync(CancellationToken cancellationToken)
        {
            await UpdateLoop(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            lock (_lock)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cts.Token);
            }

            var token = _cts.Token;

            try
            {
                var updateTask = Task.Run(() => UpdateLoop(token), token);
                var deleteTask = Task.Run(() => DeleteLoop(token), token);
                var updateOutboundTask = Task.Run(() => UpdateOrdersLoop(token), token);
                var generateTask = Task.Run(() => GenerateLoop(token), token);

                await Task.WhenAny(
                    updateTask,
                    deleteTask,
                    updateOutboundTask,
                    generateTask
                );
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("SGA has been canceled.");
            }
            finally
            {
                lock (_lock)
                {
                    _cts?.Dispose();
                    _cts = null;
                    _executionTask = null;
                }
            }
        }



        private async Task UpdateOrdersLoop(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _pauseEvent.Wait(stoppingToken);

                try
                {
                    List<WMSModel.Parameter> parameters = DataBaseActions.GetParameters();
                    DataBaseResponse wfmData = DataBaseActions.GetData();

                    int delay = Converter.ConvertMinutesToMiliseconds(
                        parameters.FirstOrDefault(x => x.Code == ParameterValue.UpdateOutboundOrders)?.Value ?? 5
                    );

                    foreach (var w in wfmData.Warehouses.Select(x => x.Id).Distinct())
                    {
                        try
                        {
                            OrdersUpdater.DoUpdate(true, delay, parameters, w, wfmData);
                            OrdersUpdater.DoUpdate(false, delay, parameters, w, wfmData);
                        }
                        catch (Exception ex) 
                        {
                            Console.WriteLine($"Warehouse {wfmData.Warehouses.First(x => x.Id == w).Name} can not be updated. {ex}");
                            continue;
                        }
                    }

                    parameters = null;
                    wfmData = null;
                    GC.Collect();

                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while updating outbound orders. {ex.Message}");
                    await Task.Delay(180000, stoppingToken);
                }
            }
        }

        private async Task UpdateInboundLoop(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _pauseEvent.Wait(stoppingToken);

                try
                {
                    List<WMSModel.Parameter> parameters = DataBaseActions.GetParameters();
                    DataBaseResponse wfmData = DataBaseActions.GetData();

                    int delay = Converter.ConvertMinutesToMiliseconds(
                        parameters.FirstOrDefault(x => x.Code == ParameterValue.UpdateInboundOrders)?.Value ?? 5
                    );

                    foreach (var w in wfmData.Warehouses.Select(x => x.Id).Distinct())
                    {
                        OrdersUpdater.DoUpdate(false, delay, parameters, w, wfmData);
                    }

                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while updating inbound orders. {ex.Message}");
                    await Task.Delay(180000, stoppingToken);
                }
            }
        }

        private async Task UpdateLoop(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _pauseEvent.Wait(stoppingToken);

                try
                {
                    List<WMSModel.Parameter> parameters = DataBaseActions.GetParameters();
                    int updateDelay = Converter.ConvertMinutesToMiliseconds(
                        parameters.FirstOrDefault(x => x.Code == ParameterValue.UpdateWorkForceTasks)?.Value ?? 10
                    );

                    DBWorkForceTask.GatherWFData();

                    Console.WriteLine("[Update] SGA update service has been executed.");
                    GC.Collect();
                    await Task.Delay(updateDelay, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Update] Error while updating. {ex.Message}");
                    await Task.Delay(180000, stoppingToken);
                }
            }
        }

        private async Task DeleteLoop(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _pauseEvent.Wait(stoppingToken);

                try
                {
                    List<WMSModel.Parameter> parameters = DataBaseActions.GetParameters();
                    int deleteDelay = Converter.ConvertMinutesToMiliseconds(
                        parameters.FirstOrDefault(x => x.Code == ParameterValue.DeleteWorkForceTasksDelay)?.Value ?? 480
                    );

                    DBWorkForceTask.DeleteOldData(
                        (int)(parameters.FirstOrDefault(x => x.Code == ParameterValue.DeleteWorkForceTasks)?.Value ?? 30)
                    );

                    Console.WriteLine("[Delete] SGA disposal service has been executed.");
                    GC.Collect();
                    await Task.Delay(deleteDelay, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Delete] Error while deleting old data. {ex.Message}");
                    await Task.Delay(180000, stoppingToken);
                }
            }
        }

        private async Task GenerateLoop(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _pauseEvent.Wait(stoppingToken);

                try
                {
                    List<WMSModel.Parameter> parameters = DataBaseActions.GetParameters();
                    DataBaseResponse wfmData = DataBaseActions.GetData();

                    List<WMSModel.WorkForceTask> workForceTasksInbound = DataBaseActions.GetWorkForceTasks(wfmData.Warehouses)
                        .Where(x => !x.IsOut)
                        .ToList();

                    List<WMSModel.WorkForceTask> workForceTasksOutbound = DataBaseActions.GetWorkForceTasks(wfmData.Warehouses)
                        .Where(x => x.IsOut)
                        .ToList();

                    if (!workForceTasksInbound.Any())
                    {
                        Console.WriteLine("No inbound work force tasks to generate inbound orders.\n");
                    }

                    else
                    {
                        foreach (var w in workForceTasksInbound)
                        {
                            if (DateTime.UtcNow.DayOfYear == w.Date.DayOfYear)
                            {
                                try
                                {
                                    DoGenerate.GenerateInboundOrders(parameters, w, wfmData);
                                    Console.WriteLine("Inbound orders generated successfully!\n");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[{w.WarehouseCode}] could generate inbound orders.");
                                    continue; 
                                }
                            }
                            else
                            {
                                Console.WriteLine("It's not time to generate inbound orders.\n");
                            }
                        }
                    }

                    if (!workForceTasksOutbound.Any())
                    {
                        Console.WriteLine("No outbound work force tasks to generate outbound orders.\n");
                    }

                    else
                    {
                        foreach (var w in workForceTasksOutbound)
                        {
                            if (DateTime.UtcNow.DayOfYear == w.Date.DayOfYear)
                            {
                                try
                                {
                                    DoGenerate.GenerateOutboundOrders(parameters, w, wfmData);
                                    Console.WriteLine("Outbound orders generated successfully!\n");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[{w.WarehouseCode}] could generate outbound orders.");
                                    continue;
                                }
                            }
                            else
                            {
                                Console.WriteLine("It's not time to generate outbound orders.\n");
                            }
                        }
                    }

                    var warehousesPlanningsToQuit = wfmData.Plannings.Where(p => p.CreationDate.Date == DateTime.UtcNow.Date && wfmData.WorkOrderPlannings.Any(x => !x.IsEstimated)).Select(x => x.WarehouseId);
                    var warehousesWithoutPlanning = wfmData.Warehouses.Select(x => x.Id).Except(warehousesPlanningsToQuit);

                    String url = "";

                    if (OperatingSystem.IsWindows())
                        url = "https://localhost:6002/";

                    else
                        url = "http://simulator-job:8080/";


                    foreach (var wwp in warehousesWithoutPlanning)
                    {
                        HttpClient _http = new HttpClient();

                        using var request = new HttpRequestMessage(HttpMethod.Get, url);

                        var warehouseId = wwp.ToString();

                        request.Headers.Add("WarehouseID", warehouseId);

                        using var response = await _http.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            var body = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"[GET] Respuesta correcta:\n{body}");
                        }
                        else
                        {
                            var body = await response.Content.ReadAsStringAsync();

                            if (Guid.TryParse(body.Trim().Trim('"'), out var guid) && guid == Guid.Empty)
                            {
                                Console.WriteLine($"[GET] No layout found for Warehouse with Guid: {warehouseId}. Simulation skipped.");
                            }
                            else
                            {
                                Console.WriteLine($"[GET] Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase} - Body: {body}");
                            }
                        }

                    }

                    wfmData = DataBaseActions.GetData();

                    var firstPlanningsWSM = DataBaseActions.GetTodayPlannings();

                    List<Models.Models.Planning> newPlannings = null;

                    int delay = Converter.ConvertMinutesToMiliseconds(
                        parameters.FirstOrDefault(x => x.Code == ParameterValue.InputCreationWindowTime)?.Value ?? 5
                    );

                    if (wfmData?.Plannings != null && wfmData.WorkOrderPlannings != null && wfmData.Plannings.Count > 0)
                    {

                        var firstPlanningsWFM = wfmData.Plannings
                            .Where(p => p.CreationDate.Date == DateTime.UtcNow.Date)
                            .Where(x => wfmData.WorkOrderPlannings.Any(m => m.PlanningId == x.Id && !m.IsEstimated))
                            .GroupBy(p => p.WarehouseId)
                            .Select(g => g.OrderBy(p => p.CreationDate).First())
                            .ToList();

                        if (firstPlanningsWFM.Any())
                        {
                            
                            if(firstPlanningsWSM.Any())
                            {
                                //Para cada planning de WFM, mira si su Id está dentro de la lista de Id de WSM. Si no está, lo conserva en newPlannings:
                                newPlannings = firstPlanningsWFM
                                    .Where(WFM => !firstPlanningsWSM.Select(WSM => WSM.Id).Contains(WFM.Id))
                                    .ToList();
                            }

                            else
                            {
                                newPlannings = firstPlanningsWFM;
                            }

                            DataBaseActions.SetLastInfo(newPlannings, wfmData);
                        }

                        parameters = null;
                        workForceTasksInbound = null;
                        workForceTasksOutbound = null;
                        wfmData = null;
                        firstPlanningsWFM = null;
                        firstPlanningsWSM = null;
                        GC.Collect();
                    }

                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while creating inbound orders. {ex.Message}");
                    await Task.Delay(180000, stoppingToken);
                }
            }
        }
    }
}
