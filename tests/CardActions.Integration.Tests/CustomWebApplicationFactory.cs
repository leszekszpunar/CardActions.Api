using CardActions.Api;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Services;
using CardActions.Infrastructure.Services;
using CardActions.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;

namespace CardActions.Integration.Tests;

/// <summary>
///     Klasa fabryczna do tworzenia klienta HTTP dla testów integracyjnych.
///     Konfiguruje środowisko testowe, zastępując wybrane zależności mockami i umożliwiając
///     testowanie API bez potrzeby uruchamiania pełnej aplikacji.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    ///     Konfiguruje środowisko testowe poprzez podmianę wybranych serwisów na mocki
    ///     oraz dostosowanie konfiguracji do potrzeb testowych.
    /// </summary>
    /// <param name="builder">Builder aplikacji webowej</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Mockowanie serwisu lokalizacji
            ConfigureLocalizationService(services);

            // Konfiguracja loadera reguł akcji karty
            ConfigureCardActionRulesLoader(services);

            // Wymuszenie użycia dostawcy CSV
            ConfigureCardActionRulesProvider(services);
        });
    }

    /// <summary>
    ///     Konfiguruje serwis lokalizacji zastępując go mockiem, który zwraca znane wartości
    ///     lub prefiksuje klucz jako [key] dla nieznanych kluczy.
    /// </summary>
    private void ConfigureLocalizationService(IServiceCollection services)
    {
        var localizationServiceMock = new Mock<ILocalizationService>();

        // Konfiguracja dla znanych kluczy
        localizationServiceMock.Setup(x => x.GetString(It.Is<string>(s => s == "Error.CardNotFound.Title")))
            .Returns("Card not found");

        localizationServiceMock.Setup(x =>
                x.GetString(It.Is<string>(s => s == "Error.CardNotFound.Detail"), It.IsAny<object[]>()))
            .Returns("Card not found for specified user");

        // Fallback dla nieznanych kluczy
        localizationServiceMock.Setup(x => x.GetString(It.IsAny<string>()))
            .Returns((string key) => $"[{key}]");

        services.AddSingleton<ILocalizationService>(localizationServiceMock.Object);
    }

    /// <summary>
    ///     Konfiguruje loader reguł akcji karty, używając ścieżki z konfiguracji
    ///     lub stosując ścieżkę domyślną.
    /// </summary>
    private void ConfigureCardActionRulesLoader(IServiceCollection services)
    {
        services.AddSingleton<ICardActionRulesLoader>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CsvCardActionRulesLoader>>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var csvPath = configuration["CardActionRulesPath"] ??
                          "../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv";
            return new CsvCardActionRulesLoader(csvPath, logger);
        });
    }

    /// <summary>
    ///     Konfiguruje provider reguł akcji karty, używając wcześniej zarejestrowanego loadera.
    /// </summary>
    private void ConfigureCardActionRulesProvider(IServiceCollection services)
    {
        services.AddSingleton<ICardActionRulesProvider>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CardActionRulesProvider>>();
            var rulesLoader = provider.GetRequiredService<ICardActionRulesLoader>();
            return new CardActionRulesProvider(rulesLoader, logger);
        });
    }

    /// <summary>
    ///     Zwalnia zasoby używane przez fabrykę testową, w tym buforowane logi Serilog.
    /// </summary>
    /// <param name="disposing">
    ///     True, jeśli metoda została wywołana bezpośrednio przez użytkownika; false, jeśli wywołana przez
    ///     finalizator
    /// </param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) Log.CloseAndFlush();
    }
}