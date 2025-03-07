using Microsoft.Extensions.DependencyInjection;

namespace CardActions.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Nie rejestrujemy CardActionService tutaj, ponieważ jest już zarejestrowany w warstwie infrastruktury
        return services;
    }
}