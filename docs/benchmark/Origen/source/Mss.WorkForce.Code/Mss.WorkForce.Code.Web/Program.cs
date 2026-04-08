using DevExpress.Blazor;
using DevExpress.Blazor.Localization;
using DevExpress.Blazor.Localization;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardWeb;
using DevExpress.XtraCharts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DBContext;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Services;
using Mss.WorkForce.Code.Web.Services.ExtraServices;
using Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces;
using Mss.WorkForce.Code.Web.Services.Interfaces;
using Mss.WorkForce.Code.Web.Services.LocalizationServices;
using Mss.WorkForce.Code.Web.Services.LocalizationServices;
using Polly;
using Polly.Retry;
using Radzen;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

//Add locatlization
builder.Services.AddLocalization();

// Add log config
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add service defaults & Aspire components.
builder.Configuration.AddEnvironmentVariables();

builder.AddServiceDefaults();
builder.Services.AddControllers()
    .AddDataAnnotationsLocalization();

builder.Services.AddEndpointsApiExplorer();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<SendParamService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IAvailableWorkerService, AvailableWorkerService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<ISimulationLogicService, SimulationLogicService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContextConfig, ContextConfig>();
builder.Services.AddScoped<IMlxDialogService, Mss.WorkForce.Code.Web.Services.MlxDialogService>();
builder.Services.AddTransient<IDesignerServices, DesignerServices>();
builder.Services.AddScoped<IInitialDataService, InitialDataService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IGridService, GridService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ICatalogueService<RolProcessSequencesDto>, RolProcessSequenceService>();
builder.Services.AddScoped<ICatalogueService<Mss.WorkForce.Code.Models.DTO.ExtraConfiguration.OrderScheduleDto>, OrderScheduleService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddScoped(typeof(IDxLocalizationService), typeof(LocalizationDevExpressService));


builder.Services.AddScoped<IPivotGridService, PivotGridService>();
builder.Services.AddSingleton<IFieldLabelService, JsonFieldLabelService>();
builder.Services.AddSingleton<ISignalRClientService, SignalRClientService>();
builder.Services.AddScoped<IScheduleDataService, ScheduleDataService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<NavigationDataSource>();
builder.Services.AddScoped<EnvironmentService>();
builder.Services.AddTransient(typeof(GetAttributesDto<>));

builder.Services.AddOutputCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDevExpressBlazor(configure => configure.BootstrapVersion = BootstrapVersion.v5);
builder.Services.AddScoped<DesignerUiStateService>();

builder.Services
    .AddServerSideBlazor()
    .AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = 5 * 1024 * 1024; // 5MB 
    });

//builder.Services.AddHttpClient<WeatherApiClient>(client =>
//    {
//        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
//        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
//        client.BaseAddress = new("https+http://apiservice");
//    });

string ApiUri = builder.Configuration["ExternalServices:ApiserviceAPI"];

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IConfiguration>().GetSection("GlobalSettings").Get<GlobalSettings>()
);

builder.Services.AddScoped<GanttJsService>();
builder.Services.AddHttpClient(Constants.ApiClientName, client =>
{
    client.BaseAddress = new Uri(ApiUri);
    client.Timeout = Timeout.InfiniteTimeSpan; // Sin limite de espera
});


ResiliencePipeline<HttpResponseMessage> pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    // For retry defaults, see: https://www.pollydocs.org/strategies/retry.html#defaults 
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 1,
        Delay = TimeSpan.FromSeconds(20),
        BackoffType = DelayBackoffType.Exponential
    })
    // For timeout defaults, see: https://www.pollydocs.org/strategies/timeout.html#defaults 
    .AddTimeout(TimeSpan.FromSeconds(500))
    .Build();

builder.Services.AddScoped<ISimulateService, SimulateService>();
//builder.Services.AddScoped<IDatabaseService, DatabaseService>();

builder.Services.AddScoped<DashboardConfigurator>((IServiceProvider serviceProvider) =>
{
    DashboardConfigurator configurator = new DashboardConfigurator();
    configurator.SetConnectionStringsProvider(new DashboardConnectionStringsProvider(builder.Configuration));
    return configurator;

});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"), // Ingles neutro
        new CultureInfo("es"), // Español neutro
        new CultureInfo("fr"), // Frances neutro
        new CultureInfo("en-US"),
        new CultureInfo("en-GB"),
        new CultureInfo("es-ES"),
        new CultureInfo("es-MX"),
        new CultureInfo("fr-FR")
    };

    options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
    options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
});

//Event Services 
builder.Services.AddScoped<IEventServices,EventServices>();

//builder.Services.AddSession();

// ***** Add authorization & authentication **** //
builder.Services.AddAuthentication("WFMAuthCookie")
    .AddCookie("WFMAuthCookie", options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/forbidden";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Es correcto o se baja a 20 min  ? 
        options.SlidingExpiration = true;

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {

                //aunque no existen servicios expuestos en server nos protegemos
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                // Guarda la URL original en sesión antes de redirigir
                context.HttpContext.Session.SetString("PendingReturnUrl",
                    context.Request.Path + context.Request.QueryString);

                // Redirige al login limpio (sin ?ReturnUrl=...)
                //context.Response.Redirect(context.Options.LoginPath);

                // para retornar 302 notfound solo comentar esta linea, esto hace que URL del sitio redirigan al login
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddDistributedMemoryCache();


// Habilita el manejo de sesión (debe estar ANTES del pipeline)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // opcional
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true; // evita problemas con GDPR
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
// ***** End authorization & authentication **** //



var app = builder.Build();
var labelService = app.Services.GetRequiredService<IFieldLabelService>();
labelService.Load();

if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, must-revalidate");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Activar localización en tiempo de ejecución
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);
app.UseOutputCache();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAntiforgery();
app.UseAuthorization();

app.MapDashboardRoute("api/dashboard", "DefaultDashboard");
app.MapDashboardRoute("api/dashboardMetrics", "MetricsDashBoard");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapControllers();
app.Run();