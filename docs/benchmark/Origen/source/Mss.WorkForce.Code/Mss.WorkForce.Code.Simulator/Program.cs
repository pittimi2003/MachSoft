using Mss.WorkForce.Code.Alerts;
using Mss.WorkForce.Code.DataBaseManager;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.MetricssModel;
using Mss.WorkForce.Code.Simulator;
using System.Diagnostics;
using Simulation = Mss.WorkForce.Code.Simulator.Simulation.Simulation;
using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Simulator.DatabaseUtilities;
using Mss.WorkForce.Code.Models.Common;
using Newtonsoft.Json;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("WFMConnection")));
builder.Services.AddScoped<DataBaseUtilitiesSimulator>();
builder.Services.AddScoped<DataAccess>();

builder.Services.AddScoped<TriggerMethods>();

builder.AddServiceDefaults();

// Add services to the container.

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

string dataBaseApiUri = builder.Configuration["ExternalServices:DataBaseAPI"];

HttpClient DataBaseAPI = new HttpClient
{
	BaseAddress = new Uri(dataBaseApiUri),
    Timeout = TimeSpan.FromSeconds(180)
};


app.MapPost("/", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        PreviewDto requestData = JsonConvert.DeserializeObject<PreviewDto>(body);
        if (requestData == null)
        {
            return Results.BadRequest("Invalid request body");
        }

        request.Headers.TryGetValue("UserFormat", out var UserFormatOptionsString);

        UserFormatOptions userFormat = JsonConvert.DeserializeObject<UserFormatOptions>(UserFormatOptionsString);


        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        using var scope = app.Services.CreateScope();

        var dataBaseResponse = (await DataBaseService.PostMessageSim(requestData, warehouseId));

        if (dataBaseResponse.Area != null)
        {
            //var performanceMetrics = new PerformanceMetrics()
            //{
            //    Id = Guid.NewGuid(),
            //    Organization = dataBaseResponse.Warehouse.Name,
            //    Site = dataBaseResponse.Warehouse.Name,
            //    Layout = dataBaseResponse.Layout.FirstOrDefault().Name,
            //    When = DateTime.UtcNow,
            //    Date = DateTime.UtcNow,
            //    NumberOfLines = dataBaseResponse.InputOrderLine.Count(),
            //    NumberOfOrders = dataBaseResponse.InputOrder.Count(),
            //    NumberOfWorkers = dataBaseResponse.AvailableWorker.Count(),
            //};

            using (Simulation simulation = new Simulation(dataBaseResponse))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                simulation.Simulate(whatIf: false);

                DataResponseSimulation dataResponseSimulation = new DataResponseSimulation();
                dataResponseSimulation.Planning = simulation.PlanningReturn;
                sw.Stop();

                //performanceMetrics.NumberOfSimulations = 1;
                //performanceMetrics.SimulationTime = sw.ElapsedMilliseconds;

                //DataBaseService.PostMetrics(performanceMetrics);

                var processResponsePreviewSimulation = PreviewConverter
               .ConvertPlanningToPreview(dataResponseSimulation.Planning.WorkOrderPlanning, dataResponseSimulation.Planning.WarehouseProcessPlanning);

                if (processResponsePreviewSimulation != null)
                {
                    var planning = GanttConverter.ConvertToGanttDataPreview(processResponsePreviewSimulation, userFormat);

                    var metrics = GanttConverter.ConvertOrdersToMetrics(dataResponseSimulation.Planning.WorkOrderPlanning);

                    var workers = scope.ServiceProvider.GetRequiredService<DataBaseUtilitiesSimulator>().BuildWorkersDistribution(
                    dataResponseSimulation.Planning.WorkOrderPlanning,
                    dataBaseResponse.Rol,
                    dataBaseResponse.Shift,
                    dataBaseResponse.Worker,
                    warehouseId);

                    PreviewData previewData = new PreviewData
                    {
                        GanttData = planning,
                        VehicleMetrics = metrics,
                        WorkersDistribution = workers
                    };

                    Console.WriteLine($"Simulation finished OK. Total Time {sw.ElapsedMilliseconds} miliseconds");
                    Console.WriteLine($"------------------------------------------------------------------------------------------");
                    return Results.Ok(previewData);
                }

                return Results.BadRequest($"Error calling service: Simulation is not preduced");
            }
        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error calling service: {ex.Message}");
    }
});

app.MapPost("/previewLog", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        PreviewDto requestData = JsonConvert.DeserializeObject<PreviewDto>(body);
        if (requestData == null)
        {
            return Results.BadRequest("Invalid request body");
        }

        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        var dataBaseResponse = (await DataBaseService.PostMessageSim(requestData, warehouseId));

        if (dataBaseResponse.Area != null)
        {
            using (Simulation simulation = new Simulation(dataBaseResponse))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                simulation.Simulate(whatIf: false);

                DataResponseSimulation dataResponseSimulation = new DataResponseSimulation();
                dataResponseSimulation.Planning = simulation.PlanningReturn;
                sw.Stop();

                return Results.Ok(simulation.LogChecks);
            }
        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error calling service: {ex.Message}");
    }
});

app.MapPost("/whatIf", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        PreviewDto requestData = JsonConvert.DeserializeObject<PreviewDto>(body);
        if (requestData == null)
        {
            return Results.BadRequest("Invalid request body");
        }

        using var scope = app.Services.CreateScope();

        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        var dataBaseResponse = (await DataBaseService.PostMessageSim(requestData, warehouseId));

        var shifts = dataBaseResponse.Shift
            .Select(s => new Shift
            {
                Id = s.Id,
                Name = s.Name,
                InitHour = s.InitHour,
                EndHour = s.EndHour,
                WarehouseId = s.WarehouseId,
                Warehouse = s.Warehouse,
            })
            .ToList();

        var initShift = dataBaseResponse.Shift.Min(x => x.InitHour);
        var endShift = dataBaseResponse.Shift.Max(x => x.EndHour);

        dataBaseResponse.Worker.Clear();
        dataBaseResponse.AvailableWorker.Clear();
        dataBaseResponse.Break.Clear();
        dataBaseResponse.Schedule.Clear();

        if (dataBaseResponse.Area != null)
        {
            List<WorkerWhatIf> workers = new List<WorkerWhatIf>();

            using (Simulation simulation = new Simulation(dataBaseResponse))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                simulation.Simulate(whatIf: true);
                sw.Stop();

                DataResponseSimulation dataResponseSimulation = new DataResponseSimulation();
                dataResponseSimulation.Planning = simulation.PlanningReturn;

                workers = scope.ServiceProvider.GetRequiredService<DataBaseUtilitiesSimulator>().BuildWorkersWhatIf(
                    dataResponseSimulation.Planning.WorkOrderPlanning,
                    dataBaseResponse.Rol,
                    shifts,
                    warehouseId);
            }

            return Results.Ok(workers);
        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error calling service: {ex.Message}");
    }
});

app.MapGet("/", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        using var scope = app.Services.CreateScope();

        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        var dataBaseResponse = (await DataBaseService.GetMessageSim(warehouseId));

        if (dataBaseResponse.Area != null)
        {
            var performanceMetrics = new PerformanceMetrics()
            {
                Id = Guid.NewGuid(),
                Organization = dataBaseResponse.Warehouse?.Name ?? string.Empty,
                Site = dataBaseResponse.Warehouse.Name,
                Layout = dataBaseResponse.Layout.FirstOrDefault()?.Name ?? string.Empty,
                When = DateTime.UtcNow,
                Date = DateTime.UtcNow,
                NumberOfLines = dataBaseResponse.InputOrderLine.Count(),
                NumberOfOrders = dataBaseResponse.InputOrder.Count(),
                NumberOfWorkers = dataBaseResponse.AvailableWorker.Count(),
            };

            if (dataBaseResponse.Layout.Any())
            {

                using (Simulation simulation = new Simulation(dataBaseResponse))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    simulation.Simulate(whatIf: false);

                    DataResponseSimulation dataResponseSimulation = new DataResponseSimulation();
                    dataResponseSimulation.Planning = simulation.PlanningReturn;
                    sw.Stop();

                    performanceMetrics.NumberOfSimulations = 1;
                    performanceMetrics.SimulationTime = sw.ElapsedMilliseconds;

                    DataBaseService.PostMetrics(performanceMetrics);

                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Simulation finished OK. Total Time {sw.ElapsedMilliseconds} miliseconds");

                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Transformating response simulation...");
                    DBModelSimulationResult responseDBInsert = TransformToDBModel.GetTransformation(dataResponseSimulation);
                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Response simulation transformated.");

                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Calculating alerts...");
                    var alertManager = scope.ServiceProvider.GetRequiredService<TriggerMethods>();
                    alertManager.SimulationPlanning = dataResponseSimulation.Planning;
                    var alertResponses = alertManager.GetTriggeredAlerts();
                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Alerts calculated.");

                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Removing deprecated alerts...");
                    scope.ServiceProvider.GetRequiredService<DataBaseUtilitiesSimulator>().RemoveDeprecatedAlertResponses(alertManager.SimulationPlanning.WarehouseId);
                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Deprecated alerts removed.");

                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Saving response simulation...");
                    scope.ServiceProvider.GetRequiredService<DataBaseUtilitiesSimulator>().SaveSimulationResult(responseDBInsert);
                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Response simulation saved.");

                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Saving alerts...");
                    scope.ServiceProvider.GetRequiredService<DataBaseUtilitiesSimulator>().SaveAlertResponses(alertResponses);
                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Alerts saved.");

                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Saving labor response...");
                    scope.ServiceProvider.GetRequiredService<DataBaseUtilitiesSimulator>().CalculateAndSaveLabor(responseDBInsert.ItemPlanning.Where(x => !x.WorkOrderPlanning.IsEstimated), dataBaseResponse);
                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Labor operations saved.");

                    //Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Saving yard response...");
                    //scope.ServiceProvider.GetRequiredService<DataBaseUtilitiesSimulator>().CalculateAndSaveYard(responseDBInsert.ItemPlanning.Where(x => !x.WorkOrderPlanning.IsEstimated));
                    //Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Yard operations saved.");


                    Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] Response simulation saved.");
                    Console.WriteLine($"------------------------------------------------------------------------------------------");
                    return Results.Ok(dataResponseSimulation.Planning.Id);
                }
            }

            else
            {
                Console.WriteLine($"[{dataBaseResponse.Warehouse.Name}] No layout found, skipping simulation...");
                return Results.BadRequest(Guid.Empty);
            }

        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
        Console.WriteLine($"{ex.StackTrace}");
        return Results.BadRequest($"Error calling service: {ex.Message}");
    }
});

app.Run();

public class BasicRequest()
{
    public string? Json { get; set; }
}


