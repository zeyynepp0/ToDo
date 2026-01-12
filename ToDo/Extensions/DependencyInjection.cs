using FluentValidation;
using Microsoft.Extensions.Configuration;
using ToDo.API.Security;
using ToDo.API.Service;
using ToDo.API.Services;
using ToDo.Application.Services;
using ToDo.Application.Validations;
using FluentValidation.AspNetCore;

namespace ToDo.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IStatusService, StatusService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IProjectStatusHistoryService, ProjectStatusHistoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddFluentValidationAutoValidation(); 
        //services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<UserRegisterDtoValidator>();
        return services;
    }
}
