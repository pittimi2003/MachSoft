using Microsoft.OpenApi.Models;
using Mss.WorkForce.Code.ApiService;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Models.HeaderEnums;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Mss.WorkForce.Code.Models.Resources;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.Actions;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<ActionRoutingOptions>(
    builder.Configuration.GetSection("ActionRouting"));

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.


builder.Services.AddEndpointsApiExplorer(); // Necesario para que Swagger detecte los endpoints de Minimal APIs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Ejemplo",
        Version = "v1",
        Description = "Una API de ejemplo usando Swagger con Minimal APIs"
    });

    // Configurar los comentarios XML
    //var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //c.IncludeXmlComments(xmlPath);
});

builder.Services.AddProblemDetails();

builder.Services.AddSingleton<DataBaseAPIClient>();

builder.Services.AddSingleton<SimulatorAPIClient>();

string simulatorApiUri = builder.Configuration["ExternalServices:SimulatorAPI"];
string dataBaseApiUri = builder.Configuration["ExternalServices:DataBaseAPI"];

HttpClient SimulatorAPI = new HttpClient
{
    BaseAddress = new Uri(simulatorApiUri),
    Timeout = TimeSpan.FromSeconds(180)
};

HttpClient DataBaseAPI = new HttpClient
{
    BaseAddress = new Uri(dataBaseApiUri),
    Timeout = TimeSpan.FromSeconds(180)
};

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Ejemplo v1");
    c.RoutePrefix = "swagger"; // Hace que Swagger UI esté en la raíz (http://localhost:<puerto>)
});

//app.UseMiddleware<SimplifiedMiddleware>();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapPost("/action/{action}", async (HttpRequest request, string action, IOptions <ActionRoutingOptions> routingOptions) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    var db = new DataBaseAPIClient(DataBaseAPI);
    var sim = new SimulatorAPIClient(SimulatorAPI);

    Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);
    request.Headers.TryGetValue("UserFormat", out var userFormat);

    try
    {
        if (warehouseId == Guid.Empty)
            return Results.BadRequest("Empty warehouse Id in header");

        switch (action)
        {
            case "orderblock":
                await db.BlockOrder(body);
                break;

            case "ordercancel":
                await db.CancelOrder(body);
                break;

            case "changepriority":
                await db.ChangePriority(body, warehouseId);
                break;

            default:
                return Results.BadRequest($"Unknown action '{action}'");
        }

        if (routingOptions.Value.ActionsRequiringSimulator.Any(a => a.Equals(action)))
            await sim.GetMessage(warehouseId); 

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Action: {action}. Error message:{ex.Message}", statusCode: 500);
    }
})
.WithName("actionDispatcher")
.WithTags("action")
.WithDescription("Unified action endpoint. Executes DB action first, then triggers simulator-job for selected actions.")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);


app.MapPost("/database", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {

        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        // Llamada al servicio
        var responseDataBase = await DataBaseService.PostSaveConfig(body);

        // Devuelve el resultado del servicio o un código de estado adecuado
        return Results.StatusCode(200);
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.StatusCode(500);
    }
}).WithName("databasePost")
.WithTags("database")
.WithDescription("This endpoint would be used to store configuration data")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/database", async (HttpRequest request, HttpClient httpClient) =>
{

    try
    {
        if (request.Query.TryGetValue("id", out var id))
        {
            var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

            // Llamada al servicio para obtener la configuración
            var responseDataBase = await DataBaseService.GetMessage(id);
            return Results.Ok("( " + responseDataBase.Json + " desde ApiService )");
        }

        else
        {
            return Results.StatusCode(500);
        }

    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.StatusCode(500);
    }
}).WithName("databaseGet")
.WithTags("database")
.WithDescription("This endpoint would be used to get a single page data passed by id")
.Produces<BasicRequest>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/simulate", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        request.Headers.TryGetValue("UserFormat", out var UserFormatOptionsString);

        var requestData = JsonConvert.DeserializeObject<PreviewDto>(body);

        var SimulatorService = new SimulatorAPIClient(SimulatorAPI);
        var responseSimulate = await SimulatorService.PostMessage(requestData, warehouseId, UserFormatOptionsString);

        if (responseSimulate != null)
        {

            // Devuelve el resultado del servicio o un código de estado adecuado
            var json = JsonConvert.SerializeObject(responseSimulate, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });

            return Results.Content(json, "application/json");
        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.StatusCode(500);
    }
}).WithName("simulatePost")
.WithTags("simulate")
.WithDescription("This endpoint would be used to run the simulation with configuration parameters")
.Produces<GanttDataConvertDto<TaskData>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/simulatePreviewLog", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        var requestData = JsonConvert.DeserializeObject<PreviewDto>(body);

        var SimulatorService = new SimulatorAPIClient(SimulatorAPI);
        var responseSimulate = await SimulatorService.PostMessage(requestData, warehouseId);

        if (responseSimulate != null)
        {

            // Devuelve el resultado del servicio o un código de estado adecuado
            var json = JsonConvert.SerializeObject(responseSimulate, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });

            return Results.Content(json, "application/json");
        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.StatusCode(500);
    }
});

app.MapPost("/whatIf", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        var requestData = JsonConvert.DeserializeObject<PreviewDto>(body);

        var SimulatorService = new SimulatorAPIClient(SimulatorAPI);
        var responseSimulate = await SimulatorService.PostMessageWhatIf(requestData, warehouseId);

        if (responseSimulate != null)
        {
            // Devuelve el resultado del servicio o un código de estado adecuado
            var json = JsonConvert.SerializeObject(responseSimulate, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });

            return Results.Content(json, "application/json");
        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.StatusCode(500);
    }
});

app.MapPost("/recalculation", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        var requestData = JsonConvert.DeserializeObject<InputOrderStatusChangesInformation>(body);

        //Actualización de la base de datos y añadido de lo que no esté guardado
        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);
        var DataSimulatorTableRequest = await DataBaseService.UpdateInputOrderStatus(requestData);

        //Guardado de la información y simulación
        var SimulatorService = new SimulatorAPIClient(SimulatorAPI);
        DataResponseSimulation responseSimulate = await SimulatorService.GetMessage(warehouseId);
        if (responseSimulate.Planning != null)
        {
            //La respuesta de la simulación el api service la envia al databasemanager para guardarla en bd
            var responseDataBase = await DataBaseService.PostSaveSimulation(responseSimulate);

            //Debido a que toda la información enviada no está siendo soportada se envía únicamente el identificador de la planificación
            return Results.Json(responseSimulate.Planning.Id);
        }
        else
        {
            return Results.Ok();
        }
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.StatusCode(500);
    }
}).WithName("recalculationPost")
.WithTags("recalculation")
.WithDescription("This endpoint would be used to recalculate the simulation with inputorder status changes information")
.Produces<PlanningPreview>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);


app.MapGet("/simulate", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        Guid? lastPlanning = await DataBaseService.GetLastPlanning(warehouseId);

        return Results.Ok(lastPlanning);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("simulateGet")
.WithTags("simulate")
.WithDescription("This endpoint runs the simulation based on headers: WarehouseID and Simulation-Case")
.Produces<Guid>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);


app.MapPost("/savepreview", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);

        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        // La respuesta de la simulación el api service la envia al databasemanager para guardarla en bd
        await DataBaseService.PostSavePreview(body, warehouseId);

        // Debido a que toda la información enviada no está siendo soportada se envía únicamente el identificador de la planificación
        return Results.Ok();
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        var errorResponse = new
        {
            Message = "An error occurred while processing the request.",
            Details = ex.Message // Incluye solo el mensaje de error
        };

        // Serializa manualmente usando Newtonsoft.Json para evitar problemas con System.Text.Json
        var json = JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented // Para mejor legibilidad
        });

        return Results.Content(json, "application/json");
    }
})
.WithName("savepreview")
.WithTags("savepreview")
.WithDescription("This endpoint is used to store configuration sequences data")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.MapGet("/layout", async (HttpRequest request, HttpClient httpClient) =>
{
    Microsoft.Extensions.Primitives.StringValues layoutId = "";
    request.Headers.TryGetValue(HeaderEnums.LayoutId.ToString(), out layoutId);

    if (layoutId == string.Empty) Results.BadRequest("There is no Layout Id for the current request");

    try
    {
        var DataBaseService = new DataBaseAPIClient(DataBaseAPI);

        var clonedLayoutId = await DataBaseService.GetClone(Guid.Parse(layoutId));

        if (layoutId == string.Empty) Results.BadRequest($"Can not generate the cloned layout for {layoutId}");

        return Results.Ok(clonedLayoutId);

    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.BadRequest(ex);
    }
}).WithName("layoutGet")
.WithTags("layout")
.WithDescription("This endpoint would be used to clone a layout")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/configcheck", async (HttpRequest request, HttpClient httpClient) =>
{

    Guid.TryParse(request.Headers["WarehouseID"], out Guid warehouseId);
    var DataBaseService = new DataBaseAPIClient(DataBaseAPI);
    Dictionary<string, List<ResourceMessage>> responseSimulate = await DataBaseService.GetConfigCheck(warehouseId);

    return Results.Json(responseSimulate);
}


).WithName("configcheck")
.WithTags("configcheck")
.WithDescription("")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);
app.MapDefaultEndpoints();

app.Run();