using MachSoft.UI.Extensions;
using MachSoft.Template.Server;
using MachSoft.Template.Server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMachSoftUi();
builder.Services.AddScoped<ShellState>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
