using CaeDashboard.EngineeringRequests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CaeDashboard.EngineeringRequests;

public static class EngineeringRequestsRegistration
{
    public static IServiceCollection AddEngineeringRequests<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.Configure<EngineeringRequestUploadOptions>(_ => { });
        services.AddScoped<EngineeringRequestService<TDbContext>>();
        services.AddScoped<EngineeringSystemService<TDbContext>>();
        services.AddScoped<RunbookService<TDbContext>>();
        return services;
    }
}
