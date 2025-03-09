using Microsoft.Extensions.DependencyInjection;

namespace CardActions.Infrastructure.Data;

public static class DependencyInjection
{
    /// <summary>
    ///     Dodaje usługi warstwy danych do kontenera DI.
    /// </summary>
    /// <param name="services">Kolekcja usług.</param>
    /// <returns>Kolekcja usług z dodanymi usługami warstwy danych.</returns>
    public static IServiceCollection AddInfrastructureData(this IServiceCollection services)
    {
        return services;
    }
}