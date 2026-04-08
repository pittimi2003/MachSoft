using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.HubSignalR.SignalR;
using Mss.WorkForce.Code.Models.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
//IgnoreAntiforgeryTokenAttribute esto es temporal NO puede pasar a un ambiente productivo
builder.Services.AddRazorPages( options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Solo ignoramos antiforgery en desarrollo
        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
    }
});
builder.Services.AddSignalR();
var app = builder.Build();

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
app.MapHub<SignalServerHub>(SignalRHubRoutes.GanttNotificationHub);
app.Run();
