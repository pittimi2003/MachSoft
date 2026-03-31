using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace MachSoft.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMachSoftUi(this IServiceCollection services)
    {
        services.AddMudServices();
        return services;
    }
}
