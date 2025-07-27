using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    }

    private static void RegisterAutoMapper(this IServiceCollection services)
    {
        //services.AddAutoMapper(typeof(IMappingProfilesMarker));
    }
}
