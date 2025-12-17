using ToDo.API.Service;
using ToDo.API.Services;
using ToDo.Application.Services;

namespace ToDo.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IStatusService, StatusService>();

        return services;
    }
}
