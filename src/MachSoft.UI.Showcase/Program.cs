using MachSoft.UI.Extensions;
using MachSoft.UI.Showcase;
using MachSoft.UI.Showcase.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMachSoftUi();
builder.Services.AddSingleton<ShowcaseThemeState>();

await builder.Build().RunAsync();
