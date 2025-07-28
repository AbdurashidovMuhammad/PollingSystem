using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polling.Application.Services;
using Polling.Application.Interfaces;
using Polling.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace Polling.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
        services.RegisterAutoMapper();
        services.AddServices(env);

        return services;
    }

    private static void AddServices(this IServiceCollection services, IWebHostEnvironment env)
    {
        // Register authentication services that use database directly
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();

        // Register validators
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<SignUpRequestValidator>();
    }

    private static void RegisterAutoMapper(this IServiceCollection services)
    {
        //services.AddAutoMapper(typeof(IMappingProfilesMarker));
    }
}
