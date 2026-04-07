using MachSoft.UI.Extensions;
using MachSoft.Demo.WebAssembly;
using MachSoft.Demo.WebAssembly.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddMachSoftUi();
builder.Services.AddSingleton<ShellState>();
await builder.Build().RunAsync();
