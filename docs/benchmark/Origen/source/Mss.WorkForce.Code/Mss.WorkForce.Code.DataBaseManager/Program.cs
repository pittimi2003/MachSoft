using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Alerts;
using Mss.WorkForce.Code.ConfigurationCheckLogs;
using Mss.WorkForce.Code.DataBaseManager;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.HeaderEnums;
using Mss.WorkForce.Code.Models.MetricssModel;
using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Mss.WorkForce.Code.Models.WMSCommunications;
using Newtonsoft.Json;
using Npgsql;
using Polly;
using Polly.Retry;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

ResiliencePipeline<HttpResponseMessage> pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    // For retry defaults, see: https://www.pollydocs.org/strategies/retry.html#defaults 
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 1,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential
    })
    // For timeout defaults, see: https://www.pollydocs.org/strategies/timeout.html#defaults 
    .AddTimeout(TimeSpan.FromSeconds(500))
    .Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Mss.WorkForce.Code.DataBaseManager")));

builder.Services.AddScoped<DataBaseUtilities>();
builder.Services.AddScoped<Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck.Check>();

builder.Services.AddDbContext<MetricsDBContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("MetricsConnection")));

builder.Services.AddScoped<DatabaseMetricUtilities>();

builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<TriggerMethods>();

builder.Services.AddHostedService<PlanningCleanupBackgroundService>();

var app = builder.Build();

app.UseHealthChecks("/health");

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var dbUtils = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>();
    dbUtils.EnsurePackingModes();
}

// We set the default data for a deployment
AppSettings deployData = new();
builder.Configuration.Bind(deployData);
await app.Services.SeedDatabaseAsync(deployData);

app.MapPost("/status", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        Console.WriteLine("Starting input order status changes...");

        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        Console.WriteLine("Deserializing input order status changes...");
        var responseSimulation = System.Text.Json.JsonSerializer.Deserialize<InputOrderStatusChangesInformation>(body, options);

        if (responseSimulation == null)
        {
            Console.WriteLine("Error during input order changes.");
            Console.WriteLine("Input order status changes deserialization null.");
            return Results.BadRequest("Invalid request body");
        }
        Console.WriteLine("Input order status changes deserialized.");


        using var scope = app.Services.CreateScope();

        //Actualiza el estado de base de datos
        Console.WriteLine("Saving input order status changes...");
        scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().SaveInputOrderStatus(responseSimulation);
        Console.WriteLine("Input order status changes saved.");

        //Devuelve el resultado del servicio o un código de estado adecuado
        Console.WriteLine("Input order status changed.");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        Console.WriteLine("Error during input order changes.");
        Console.Write(ex.ToString());
        return Results.StatusCode(500);
    }
});

app.MapPost("/config", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        Console.WriteLine("Starting updating configuration...");

        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();


        Console.WriteLine("Deserializing actions in configuration...");
        var Actions = JsonConvert.DeserializeObject<Actions>(body);

        if (Actions == null)
        {
            Console.WriteLine("Error during updating configuration.");
            Console.WriteLine("Actions in coniguration deserialization null.");
            return Results.BadRequest("Invalid request body");
        }
        Console.WriteLine("Actions in configuration deserialized.");

        using var scope = app.Services.CreateScope();

        var response = new BasicRequest();

        // Llamada al servicio para guardar en base de datos los parámetros que el usuario ha actualizado
        Console.WriteLine("Updating configuration...");
        scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().UpdateConfig(Actions);
        Console.WriteLine("Configuration updated.");

        // Devuelve el resultado del servicio o un código de estado adecuado
        Console.WriteLine("Configuration updated.");
        return Results.Ok(response);
    }
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
    {
        Console.WriteLine($"PG ERROR SqlState={pg.SqlState} Constraint={pg.ConstraintName} Message={pg.MessageText}");
        Console.WriteLine(ex.ToString());
        return Results.Problem(title: "Database error", detail: pg.MessageText, statusCode: 500);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return Results.StatusCode(500);
    }
});

app.MapGet("/sim", async (HttpRequest request, HttpClient httpClient) =>
{

    try
    {
        Console.WriteLine("Starting preparing data for simulation (get)...");

        Guid? warehouseId;

        using (var scope = app.Services.CreateScope())
        {
            Console.WriteLine("Parsing warehouseId...");
            warehouseId = Guid.TryParse(request.Headers["WarehouseID"], out Guid parsedGuid) && parsedGuid != Guid.Empty ?
                            parsedGuid: scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().GetWarehousePlaceHolder();

            if (warehouseId != null)
            {
                Console.WriteLine("WarehouseId parsed.");
                // Creas json vacío

                Console.WriteLine("Creating empy actions from json with null value...");
                Actions json = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().CreateJson(null);
                Console.WriteLine("Created empty actions from json with null value...");

                // Select data, create from configsequence and get current input orders
                Console.WriteLine("Selecting data for simulation...");
                DataSimulatorTablaRequest response = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().SelectDataForSimulatorPlanning(json, warehouseId.Value);
                Console.WriteLine("Data selected for simualation.");

                // Devuelve el resultado del servicio o un código de estado adecuado
                Console.WriteLine("Data for simulation (get) prepared.");
                return Results.Ok(response);
            }
            else
            {
                Console.WriteLine("Error during getting data for simulation (get).");
                Console.WriteLine("Not possible to parse warehouseId.");
                return Results.Ok();
            }
        }
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        Console.WriteLine("Error during getting data for simulation (get).");
        Console.WriteLine(ex.ToString());
        return Results.StatusCode(500);
    }
});


app.MapPost("/sim", async (HttpRequest request, HttpClient httpClient) =>
{

    Console.WriteLine("Starting preparing data for simulation (post)...");
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    Actions? actions = null;
    try
    {
        Guid? warehouseId;
        using (var scope = app.Services.CreateScope())
        {
            Console.WriteLine("Parsing warehouseId...");
            warehouseId = Guid.TryParse(request.Headers["WarehouseID"], out Guid parsedGuid) && parsedGuid != Guid.Empty ?
                            parsedGuid: scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().GetWarehousePlaceHolder();

            if (warehouseId != null)
            {
                Console.WriteLine("WarehouseId parsed.");

                Console.WriteLine("Deserializing simulation parameters...");
                var parametersRequest = System.Text.Json.JsonSerializer.Deserialize<PreviewDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parametersRequest == null)
                {
                    Console.WriteLine("Error during getting data for simulation (post).");
                    Console.WriteLine("Simulation parameters deserialization null.");
                    return Results.BadRequest("Invalid request body");
                }
                Console.WriteLine("Simulation parameters deserializated.");

                // Select data, create from preview window and get current input orders
                Console.WriteLine("Selecting data for simulation...");
                DataSimulatorTablaRequest response = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().SelectDataForSimulatorPreview(actions, warehouseId.Value, parametersRequest);
                Console.WriteLine("Data selected for simualation.");

                response.Date = null;

                Console.WriteLine("Data for simulation (post) prepared.");
                return Results.Ok(response);
            }
            else
            {
                Console.WriteLine("Error during getting data for simulation (post).");
                Console.WriteLine("Not possible to parse warehouseId.");
                return Results.Ok();
            }
        }
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        Console.WriteLine("Error during getting data for simulation (post).");
        Console.WriteLine(ex.ToString());
        return Results.StatusCode(500);
    }

});

app.MapPost("/Scenarioplanner", async (HttpRequest request, HttpClient httpClient) =>
{
    Console.WriteLine("Staring saving preview data...");
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        Guid? warehouseId;

        using (var scope = app.Services.CreateScope())
        {

            Console.WriteLine("Parsing warehouseId...");
            warehouseId = Guid.TryParse(request.Headers["WarehouseID"], out Guid parsedGuid) && parsedGuid != Guid.Empty ?
                            parsedGuid: scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().GetWarehousePlaceHolder();

            if (warehouseId != null)
            {

                Console.WriteLine("WarehouseId parsed.");

                Console.WriteLine("Deserializing json parameters...");
                var requestData = JsonConvert.DeserializeObject<PreviewDto>(body);
                if (requestData == null)
                {
                    Console.WriteLine("Error during saving preview data.");
                    Console.WriteLine("Json parameters deserialization null.");
                    return Results.BadRequest("Invalid request body");
                }
                Console.WriteLine("Json parameters deserializated.");

                //  Insert metrics into database
                Console.WriteLine("Saving data preview...");
                scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().SavePreview(requestData, warehouseId.Value);
                Console.WriteLine("Preview data saved.");

                return Results.Content("{}", "application/json");
            }
            else
            {
                Console.WriteLine("Error during saving preview data.");
                Console.WriteLine("Not possible to parse warehouseId.");
                return Results.Ok();
            }
        }
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        Console.WriteLine("Error during saving preview data.");
        Console.WriteLine(ex.ToString());
        return Results.StatusCode(500);
    }

});

app.MapPost("/metrics", async (HttpRequest request, HttpClient httpClient) =>
{
    Console.WriteLine("Starting saving metrics...");
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        Console.WriteLine("Deserializing object for metrics...");
        var parametersRequest = JsonConvert.DeserializeObject<PerformanceMetrics>(body);
        if (parametersRequest == null)
        {
            Console.WriteLine("Error during saving metrics.");
            Console.WriteLine("Object for metrics deserialization null.");
            return Results.BadRequest("Invalid request body");
        }
        Console.WriteLine("Object for metrics deserialized.");

        using var scope = app.Services.CreateScope();

        //  Insert metrics into database
        Console.WriteLine("Saving metrics...");
        scope.ServiceProvider.GetRequiredService<DatabaseMetricUtilities>().SendMetrics(parametersRequest);
        Console.WriteLine("Metrics saved");

        return Results.Ok();
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        Console.WriteLine("Error during saving metrics.");
        Console.WriteLine(ex.ToString());
        return Results.StatusCode(500);
    }

});

app.MapGet("/clone", async (HttpRequest request, HttpClient httpClient) =>
{
    Console.WriteLine("Staring cloning layout...");

    Microsoft.Extensions.Primitives.StringValues layoutId = "";
    Console.WriteLine("Parsing layoutId...");
    request.Headers.TryGetValue(HeaderEnums.LayoutId.ToString(), out  layoutId);
    if (layoutId == string.Empty)
    {
        Console.WriteLine("Error during cloning layout.");
        Console.WriteLine("Not possible to parse layoutId");
        Results.BadRequest("There is no Layout Id for the current request");
    }
    Console.WriteLine("LayoutId parsed");

    try
    {
        using var scope = app.Services.CreateScope();

        Console.WriteLine("Cloning layout...");
        var clonedLayoutId = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().CloneLayout(Guid.Parse(layoutId));
        Console.WriteLine("Layout cloned.");

        return Results.Ok(clonedLayoutId);

    }
    catch (Exception ex)
    {
        Console.WriteLine("Error during cloning layout.");
        Console.WriteLine(ex.ToString());
        return Results.BadRequest(ex);
    }
});

app.MapPost("/blockOrder", async (HttpRequest request, HttpClient httpClient) =>
{
    Console.WriteLine("Blocking order...");
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        Console.WriteLine("Deserializing object...");
        var blockOrdersRequest = JsonConvert.DeserializeObject<List<WorkOrderBlock>>(body);
        if (blockOrdersRequest == null)
        {
            Console.WriteLine("Error during blocking order.");
            Console.WriteLine("Object deserialization null.");
            return Results.BadRequest("Invalid request body");
        }
        Console.WriteLine("Object deserializated.");

        using var scope = app.Services.CreateScope();
        Console.WriteLine("Blocking order...");
        scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().BlockOrder(blockOrdersRequest);
        Console.WriteLine("Order blocked.");

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error during blocking order.");
        Console.WriteLine(ex.ToString());
        return Results.BadRequest(ex);
    }
});

app.MapPost("/changepriority", async (HttpRequest request, HttpClient httpClient) =>
{
    Guid.TryParse(request.Headers["WarehouseID"], out Guid parsedGuid);

    if (parsedGuid.Equals(Guid.Empty))
        return Results.BadRequest("Empty warehouse Id in header");

    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    Console.WriteLine("Deserializing object...");
    var inputOrders = JsonConvert.DeserializeObject<ChangePriorityDto>(body);

    if (inputOrders.WorkOrderId == null || inputOrders.Priority == null)
        return Results.BadRequest("Invalid request body");


    using var scope = app.Services.CreateScope();

    try
    {
        scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().ChangePriority(inputOrders, parsedGuid);
    }

    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }

    catch (DbException dbEx)
    {
        return Results.Problem("Problem with database", statusCode: 500);
    }

    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }

    return Results.Ok(true);


});

app.MapPost("/cancelOrder", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var idOrderList = JsonConvert.DeserializeObject<List<Guid>>(body);

        using var scope = app.Services.CreateScope();
        var canncelledOrdersCount = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().CancelOrders(idOrderList);

        if(canncelledOrdersCount > 0) return Results.Ok();
        else return Results.BadRequest("No Order(s) were cancelled");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapGet("/getLastPlanning", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        Guid.TryParse(request.Headers["WarehouseID"], out Guid parsedGuid);

        return Results.Ok(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataBaseUtilities>().getLastPlanning(parsedGuid));


    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapGet("/getLastUpdateDateInputOrderProcessesClosing", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        if (!request.Headers.TryGetValue("WarehouseCode", out var warehouseCode) || string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Results.BadRequest("Invalid or missing WarehouseCode");
        }

        warehouseCode = Uri.UnescapeDataString(warehouseCode!);

        return Results.Ok(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataBaseUtilities>().GetLastUpdateDateInputOrderProcessesClosing(warehouseCode));


    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapGet("/getReleaseDateOutboundOrder", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        if (!request.Headers.TryGetValue("WarehouseCode", out var warehouseCode) || string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Results.BadRequest("Invalid or missing WarehouseCode");
        }

        warehouseCode = Uri.UnescapeDataString(warehouseCode!);

        return Results.Ok(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataBaseUtilities>().GetReleaseDateOutboundOrders(warehouseCode));


    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});
app.MapGet("/getLastUpdateDateInputOrders", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        if (!request.Headers.TryGetValue("WarehouseCode", out var warehouseCode) || string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Results.BadRequest("Invalid or missing WarehouseCode");
        }

        warehouseCode = Uri.UnescapeDataString(warehouseCode!);

        return Results.Ok(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataBaseUtilities>().GetLastUpdateDateInputOrders(warehouseCode));


    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapPost("/updateInputOrderProcessesClosing", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        request.Headers.TryGetValue(HeaderEnums.UserName.ToString(), out var user);
        request.Headers.TryGetValue(HeaderEnums.Sender.ToString(), out var sender);
        var warehouse = request.Headers["Warehouse"].FirstOrDefault();

        if (string.IsNullOrEmpty(warehouse))
        {
            return Results.BadRequest("Header 'Warehouse' is required");
        }

        warehouse = Uri.UnescapeDataString(warehouse!);

        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        List<InputOrderProcessesClosingCommunication> inputOrderList = JsonConvert.DeserializeObject<List<InputOrderProcessesClosingCommunication>>(body);

        using var scope = app.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().UpdateInputOrderProcessesClosingAsync(inputOrderList, warehouse, user, sender);

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapPost("/updateInputOrders", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();

        request.Headers.TryGetValue(HeaderEnums.UserName.ToString(), out var user);
        request.Headers.TryGetValue(HeaderEnums.Sender.ToString(), out var sender);

        List<InputOrderCommunication> inputOrderList = JsonConvert.DeserializeObject<List<InputOrderCommunication>>(body);

        using var scope = app.Services.CreateScope();
        // TODO: revisar 
        Guid warehouseId = await scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().UpdateInputOrdersAsync(inputOrderList, user, sender);

        return Results.Ok(warehouseId);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapGet("/configurationCheck", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        DataSimulatorTablaRequest? response;
        bool result = false;
        var configCheck = new ConfigCheck();
        using (var scope = app.Services.CreateScope())
        {
            Console.WriteLine("Parsing warehouseId...");
            Guid? warehouseId = Guid.TryParse(request.Headers["WarehouseID"], out Guid parsedGuid) && parsedGuid != Guid.Empty ?
                            parsedGuid : scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().GetWarehousePlaceHolder();

            if (warehouseId != null)
            {
                Actions json = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().CreateJson(null);
                response = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().SelectDataForSimulatorPlanning(json, warehouseId.Value);

                result = scope.ServiceProvider.GetRequiredService<Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck.Check>()
                        .Configuration(response, ref configCheck);

                scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().setWarehouseConfig(warehouseId, result);


            }
        }

        return Results.Ok(configCheck.KeyValuePairs);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
        Console.WriteLine($"{ex.StackTrace}");
        return Results.BadRequest($"Error calling service: {ex.Message}");
    }
});

app.MapGet("/configurationCheckBackground", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        DataSimulatorTablaRequest? response;
        bool result = false;
        var configCheck = new ConfigCheck();
        using (var scope = app.Services.CreateScope())
        {
            Console.WriteLine("Parsing warehouseId...");
            Guid? warehouseId = Guid.TryParse(request.Headers["WarehouseID"], out Guid parsedGuid) && parsedGuid != Guid.Empty ?
                            parsedGuid : scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().GetWarehousePlaceHolder();

            if (warehouseId != null)
            {
                Actions json = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().CreateJson(null);
                response = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().SelectDataForSimulatorPlanning(json, warehouseId.Value);

                result = scope.ServiceProvider.GetRequiredService<Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck.Check>()
                        .Configuration(response, ref configCheck);

                scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().setWarehouseConfig(warehouseId, result);

            }

            return Results.Ok(result);
        }


    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.Message}");
        Console.WriteLine($"{ex.StackTrace}");
        return Results.BadRequest($"Error calling service: {ex.Message}");
    }
});

app.MapPost("/addTransaction", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();

        var transaction = JsonConvert.DeserializeObject<Transaction>(body);

        using var scope = app.Services.CreateScope();
        
        var result = scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().AddTransaction(transaction);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapPost("/getWarehouse", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        if (!request.Headers.TryGetValue("Warehouse", out var warehouseCode) || string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Results.BadRequest("Invalid or missing Warehouse");
        }
        warehouseCode = Uri.UnescapeDataString(warehouseCode!);

        return Results.Ok(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataBaseUtilities>().GetWarehouse(warehouseCode!));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapPost("/getlastupdatewarehouseprocess", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {

        if (!request.Headers.TryGetValue("WarehouseCode", out var warehouseCode) || string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Results.BadRequest("Invalid or missing WarehouseCode");
        }

        warehouseCode = Uri.UnescapeDataString(warehouseCode!);

        return Results.Ok(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataBaseUtilities>().GetLastUpdateDateWarehouseProcesses(warehouseCode));


    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});

app.MapPost("/updatewarehouseprocess", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();

        request.Headers.TryGetValue(HeaderEnums.UserName.ToString(), out var user);
        request.Headers.TryGetValue(HeaderEnums.Sender.ToString(), out var sender);

        List<WarehouseProcessClosingCommunicaation> warehouseProcessList = JsonConvert.DeserializeObject<List<WarehouseProcessClosingCommunicaation>>(body);

        using var scope = app.Services.CreateScope();
        // TODO: revisar 
        await scope.ServiceProvider.GetRequiredService<DataBaseUtilities>().UpdateWarehouseProcessesClosingAsync(warehouseProcessList, user, sender);

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
});


app.Run();

public class Actions
{
    public Dictionary<string, List<Dictionary<string, object>>> New { get; set; }
    public Dictionary<string, List<Dictionary<string, object>>> Update { get; set; }
    public Dictionary<string, List<Dictionary<string, object>>> Delete { get; set; }
}

public class BasicRequest()
{
    public string? Json { get; set; }
}

public class DataResponseSimulation()
{
    public PlanningReturn Planning { get; set; }
}

public class DBModelSimulationResult()
{
    public Planning Planning { get; set; }
    public List<WorkOrderPlanning> WorkOrderPlanning { get; set; }
    public List<WarehouseProcessPlanning> WarehouseProcessPlanning { get; set; }
    public List<ItemPlanning> ItemPlanning { get; set; }
}

public class WorkOrderBlock()
{
    public Guid Id { get; set; }
    public bool? IsBlocked { get; set; }
    public DateTime? BlockDate { get; set; }
    public double? Duration { get; set; }
}

