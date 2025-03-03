using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CardActions.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        // Rejestracja serwisów domenowych

        return services;
    }
}