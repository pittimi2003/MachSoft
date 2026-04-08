using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.WFMConnector;
using Mss.WorkForce.Code.WFMConnector.Midleware;
using System.Data.Common;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString); // o UseSqlServer, UseMySql, etc.
});

builder.Services.AddScoped<DataAccess>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("DataBaseClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:DataBaseAPI"] ?? "");
    client.Timeout = TimeSpan.FromSeconds(180);
});


builder.Services.AddHttpClient("SimulatorClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:SimulatorJob"] ?? "");
    client.Timeout = TimeSpan.FromSeconds(180);
});

var app = builder.Build();

app.MapDefaultEndpoints();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string ApiClientApiUri = builder.Configuration["ExternalServices:ApiServiceAPI"];

HttpClient ApiClientAPI = new HttpClient
{
    BaseAddress = new Uri(ApiClientApiUri),
    Timeout = TimeSpan.FromSeconds(180)
};

string simulatorClientApiUri = builder.Configuration["ExternalServices:SimulatorJob"];

HttpClient simulatorJobClientAPI = new HttpClient
{
    BaseAddress = new Uri(simulatorClientApiUri),
    Timeout = TimeSpan.FromSeconds(180)
};

string dbmanagerClientApiUri = builder.Configuration["ExternalServices:DataBaseAPI"];

HttpClient dbmanagerClientAPI = new HttpClient
{
    BaseAddress = new Uri(dbmanagerClientApiUri),
    Timeout = TimeSpan.FromSeconds(180)
};

var simulatorClient = new SimulatorAPIClient(simulatorJobClientAPI);

var apiClient = new ApiServiceApiClient(ApiClientAPI);

var dbmanagerClient = new DataBaseAPIClient(dbmanagerClientAPI);

await dbmanagerClient.WaitForHealthAsync();

_ = Task.Run(async () =>
{
    while (true)
    {

        try
        {

            using var scope = app.Services.CreateScope();

            foreach (var warehouse in scope.ServiceProvider.GetRequiredService<DataAccess>().GetWarehouses())
            {

                if (!warehouse.IsConfigValid.HasValue)
                    warehouse.IsConfigValid = await dbmanagerClient.CheckConfigAsync(warehouse.Id);

                if (warehouse.IsConfigValid == true)
                    _ = simulatorClient.GetMessage(warehouse.Id);

                Task.Delay(int.TryParse(builder.Configuration["JobWaitingTime"], out var ms) ? ms : 3000).Wait();

            }

            await Task.Delay(TimeSpan.FromMinutes(double.TryParse(builder.Configuration["JobInterval"], out var m) ? m : 15));

        }

        catch (Exception ex) when (
            ex is DbUpdateException ||
            ex is DbException ||
            ex is SocketException
        )
        {
            Console.WriteLine($"[Job][DB ERROR] {ex.Message}");
            break;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"[Job][ERROR] {ex.Message}");
        }
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();


app.MapPost("/statusNotification", async (HttpRequest request, HttpClient httpClient) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        var ApiClient = new ApiServiceApiClient(ApiClientAPI);

        await ApiClient.PostNotification(body);

        // Devuelve el resultado del servicio o un código de estado adecuado
        return Results.StatusCode(200);
    }
    catch (Exception ex)
    {
        // Devuelve un error en caso de fallo en el servicio
        return Results.StatusCode(500);
    }
});

app.Run();




