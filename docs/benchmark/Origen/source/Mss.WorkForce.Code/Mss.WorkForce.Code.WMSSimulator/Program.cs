using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.WMSSimulator;
using Mss.WorkForce.Code.WMSSimulator.WMSModel;
using Newtonsoft.Json;
using InputOrder = Mss.WorkForce.Code.WMSSimulator.WMSModel.InputOrder;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IWorkForceTaskScraperDetector, WorkForceTaskScraperDetector>();
builder.Services.AddHostedService(provider =>
    (WorkForceTaskScraperDetector)provider.GetRequiredService<IWorkForceTaskScraperDetector>());



builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("WFMConnection")));

builder.Services.AddDbContext<WMSDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("WFMConnection")));

var WFMConnectionString = builder.Configuration.GetConnectionString("WFMConnection");
var WMSConnectionString = builder.Configuration.GetConnectionString("WMSConnection");
var ApiServiceURL = builder.Configuration["ExternalServices:ApiServiceAPI"];

builder.Services.AddScoped<DataBaseActions>();

builder.Services.AddScoped<DBWorkForceTask>();

DataBaseActions.WFMConnectionString = WFMConnectionString;
DBWorkForceTask.WFMConnectionString = WFMConnectionString;

DataBaseActions.WMSConnectionString = WMSConnectionString;
DBWorkForceTask.WMSConnectionString = WMSConnectionString;

builder.Services.AddHttpClient();
builder.Services.Configure<AppSettings>(builder.Configuration);



//builder.Services.AddDbContext<WMSDBContext>(options =>
//options.UseNpgsql(builder.Configuration.GetConnectionString("MetricsConnection")));

//builder.Services.AddScoped<DatabaseMetricUtilities>();

var app = builder.Build();

var scraperService = app.Services.GetRequiredService<IWorkForceTaskScraperDetector>();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

using (var context = new WMSDbContext(optionsBuilderWMS.Options))
{
    try
    {
        bool running = (context.Parameters.FirstOrDefault(w => w.Code == "IS_ACTIVE").Value == 0) ? false : true;

        if (!running)
        {
            scraperService.StopExecution();
        }
    }
    catch
    {
        scraperService.StopExecution();
    }
}


app.MapGet("/parameters", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        List<Parameter> parameters = new List<Parameter>();

        var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
        optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

        using (var context = new WMSDbContext(optionsBuilderWMS.Options))
        {
            foreach (var parameter in context.Parameters.Where(w => w.Code != "IS_ACTIVE"))
            {
                parameters.Add(parameter);
            }
        }

        return Results.Ok(parameters);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.MapGet("/inputorders", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        List<InputOrder> inputOrders = new List<InputOrder>();

        var requestId = request.Query["warehouseId"].ToString().Trim();

        if (!Guid.TryParse(requestId, out Guid requestGuid))
        {
            return Results.BadRequest(new { message = "warehouseId inválido o no proporcionado" });
        }

        var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
        optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

        using (var context = new WMSDbContext(optionsBuilderWMS.Options))
        {
            foreach (var inputorder in context.InputOrders.Where(i => i.WarehouseId == requestGuid && i.UpdateDate >= DateTime.UtcNow.Date))
            {
                inputOrders.Add(inputorder);
            }
        }

        return Results.Ok(inputOrders);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.MapGet("/warehouses", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        List<WarehouseData> warehouseDatas = new List<WarehouseData>();

        var optionsBuilderWMF = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilderWMF.UseNpgsql(DataBaseActions.WFMConnectionString);

        using (var context = new ApplicationDbContext(optionsBuilderWMF.Options))
        {
            warehouseDatas.AddRange(context.Warehouses
                .Select(w => new WarehouseData { Id = w.Id, Code = w.Code }));
        }

        return Results.Ok(warehouseDatas);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.MapPost("/parameters", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        var parameters = JsonConvert.DeserializeObject<Parameter>(body);

        var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
        optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

        using (var context = new WMSDbContext(optionsBuilderWMS.Options))
        {
            context.Parameters.FirstOrDefault(x => x.Code == parameters.Code).Value = parameters.Value;

            context.SaveChanges();
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.MapPost("/inputorders", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        var inputOrderRequest = await request.ReadFromJsonAsync<TableRequest>();

        if (inputOrderRequest == null || inputOrderRequest.Data == null || !inputOrderRequest.Data.Any())
        {
            return Results.BadRequest("Invalid request: Orders list is empty.");
        }

        var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
        optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

        using (var context = new WMSDbContext(optionsBuilderWMS.Options))
        {
            var ordersToUpdate = context.InputOrders
                .Where(x => inputOrderRequest.Data.Contains(x.Id))
                .ToList();

            if (!ordersToUpdate.Any())
            {
                return Results.NotFound("No matching orders found.");
            }

            foreach (var order in ordersToUpdate)
            {
                order.Status = inputOrderRequest.Action;
            }

            context.SaveChanges();
        }

        return Results.Ok(new { message = "Orders updated successfully", action = inputOrderRequest.Action });
    }
    catch (Exception ex)
    {
        // Log del error
        Console.WriteLine($"Error: {ex.Message}");
        return Results.StatusCode(500);
    }
});
app.MapGet("/workforcetasks", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        List<WorkForceTask> workForceTasks = new List<WorkForceTask>();

        var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
        optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

        var requestId = request.Query["warehouseId"].ToString().Trim();

        if (!Guid.TryParse(requestId, out Guid requestGuid))
        {
            using (var context = new WMSDbContext(optionsBuilderWMS.Options))
            {
                workForceTasks.AddRange(context.WorkForceTasks
                    .Where(w => w.Date.Date == DateTime.UtcNow.Date)
                    .Select(m => new WorkForceTask
                    {
                        WarehouseCode = m.WarehouseCode,
                        WarehouseId = m.WarehouseId,
                        IsOut = m.IsOut,
                        InitHour = m.InitHour,
                        EndHour = m.EndHour,
                        NumOrders = m.NumOrders,
                        NumOrdersCompleted = m.NumOrdersCompleted,
                        LinesPerOrder = m.LinesPerOrder,
                        Date = m.Date,
                        Id = m.Id
                    }));
            }

        }

        else
        {
            using (var context = new WMSDbContext(optionsBuilderWMS.Options))
            {
                workForceTasks.AddRange(context.WorkForceTasks
                    .Where(w => w.Date.Date == DateTime.UtcNow.Date && w.WarehouseId == requestGuid)
                    .Select(m => new WorkForceTask
                    {
                        WarehouseCode = m.WarehouseCode,
                        WarehouseId = m.WarehouseId,
                        IsOut = m.IsOut,
                        InitHour = m.InitHour,
                        EndHour = m.EndHour,
                        NumOrders = m.NumOrders,
                        NumOrdersCompleted = m.NumOrdersCompleted,
                        LinesPerOrder = m.LinesPerOrder,
                        Date = m.Date,
                        Id = m.Id
                    }));
            }

        }



        List<WarehouseData> warehouseDatas = new List<WarehouseData>();

        var optionsBuilderWMF = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilderWMF.UseNpgsql(DataBaseActions.WFMConnectionString);

        using (var context = new ApplicationDbContext(optionsBuilderWMF.Options))
        {
            warehouseDatas.AddRange(context.Warehouses
                .Select(w => new WarehouseData { Id = w.Id, Code = w.Code }));
        }

        var response = new
        {
            WorkForceTasks = workForceTasks.OrderBy(m => (m.WarehouseCode, m.IsOut, m.InitHour, m.EndHour)),
            Warehouses = warehouseDatas
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});


app.MapPost("/workforcetasks", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        var taskRequest = await request.ReadFromJsonAsync<TaskRequest>();

        var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
        optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

        using (var context = new WMSDbContext(optionsBuilderWMS.Options))
        {
            switch (taskRequest.Action)
            {
                case "Delete":
                    context.WorkForceTasks.Where(x => taskRequest.Data.Contains(x.Id)).ExecuteDelete();
                    break;

                case "Update":
                    var existingTask = context.WorkForceTasks.FirstOrDefault(x => taskRequest.Data.Contains(x.Id));
                    
                    if (existingTask != null && taskRequest.TaskData != null)
                    {
                        existingTask.NumOrders = taskRequest.TaskData.NumOrders;
                        existingTask.LinesPerOrder = taskRequest.TaskData.LinesPerOrder;
                        existingTask.IsOut = taskRequest.TaskData.IsOut;
                        existingTask.InitHour = taskRequest.TaskData.InitHour;
                        existingTask.EndHour = taskRequest.TaskData.EndHour;
                    }
                    break;

                case "Add":
                    if (taskRequest.TaskData != null)
                    {
                        taskRequest.TaskData.Id = Guid.NewGuid();
                        taskRequest.TaskData.Date = DateTime.SpecifyKind(taskRequest.TaskData.Date, DateTimeKind.Utc);
                        context.WorkForceTasks.Add(taskRequest.TaskData);
                    }
                    break;

                default:
                    return Results.BadRequest("Invalid action.");
            }

            context.SaveChanges();
        }

        return Results.Ok(new { message = "Table updated successfully", action = taskRequest.Action });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.StatusCode(500);
    }
});

app.MapPost("/service", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    ActionRequest actionRequest = JsonConvert.DeserializeObject<ActionRequest>(body);

    var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
    optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

    using (var context = new WMSDbContext(optionsBuilderWMS.Options))
    {

        var parameter = context.Parameters.FirstOrDefault(w => w.Code == "IS_ACTIVE");

        switch (actionRequest.Action)
        {
            case "Pause":
                scraperService.PauseExecution();
                break;

            case "Continue":
                scraperService.ContinueExecution();
                break;

            case "Stop":
                scraperService.StopExecution();

                if (parameter != null)
                    parameter.Value = 0; 
                break;

            case "Start":
                scraperService.StartExecution();
                if (parameter != null)
                    parameter.Value = 1; 
                break;

            case "ForceUpdate":
                await scraperService.ForceUpdateAsync(CancellationToken.None);
                break;

            default:
                return Results.BadRequest("Invalid action.");

        }

        context.SaveChanges();
    };

    return Results.Ok(new { message = "Service updated successfully", action = actionRequest.Action });
});

app.MapGet("/service", async (HttpRequest request, HttpClient httpClient) =>
{
    var optionsBuilderWMS = new DbContextOptionsBuilder<WMSDbContext>();
    optionsBuilderWMS.UseNpgsql(DataBaseActions.WMSConnectionString);

    using (var context = new WMSDbContext(optionsBuilderWMS.Options))
    {
        bool running = (context.Parameters.FirstOrDefault(w => w.Code == "IS_ACTIVE").Value == 0) ? false : true;


        return Results.Ok(running);
    }

    
});

app.MapGet("/results", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        var requestGuid = request.Query["warehouseId"].ToString().Trim();

        if (!Guid.TryParse(requestGuid, out Guid warehouseId))
        {
            return Results.BadRequest(new { message = "warehouseId inválido o no proporcionado" });
        }

        Console.WriteLine("Init gathering simulation results");
        List <SimulationResults> simulationResults = new List<SimulationResults>();
        simulationResults = DBWorkForceTask.GatherSimulationResults(warehouseId);
        Console.WriteLine("End gathering simulation results");

        return Results.Ok(simulationResults);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.UseMiddleware<SimplifiedMiddleware>();
app.Run();




