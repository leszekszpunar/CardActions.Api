using CardActions.Api;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Services;
using CardActions.Domain.Services;
using CardActions.Domain.Services.Interfaces;
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

            // Mockowanie serwisu kart
            ConfigureCardService(services);

            // Konfiguracja loadera reguł akcji karty
            ConfigureCardActionRulesLoader(services);

            // Wymuszenie użycia dostawcy CSV
            ConfigureCardActionRulesProvider(services);
            
            // Rejestracja serwisu akcji karty
            ConfigureCardActionService(services);
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
    ///     Konfiguruje serwis kart zastępując go mockiem, który zwraca testowe dane kart.
    /// </summary>
    private void ConfigureCardService(IServiceCollection services)
    {
        var cardServiceMock = new Mock<ICardService>();

        // Konfiguracja dla karty PREPAID + CLOSED - przykład z wymagań
        cardServiceMock.Setup(x => x.GetCardDetailsAsync("User1", "Card17"))
            .ReturnsAsync(new Domain.Models.CardDetails("Card17", Domain.Enums.CardType.Prepaid, Domain.Enums.CardStatus.Closed, true));

        // Konfiguracja dla karty CREDIT + BLOCKED - przykład z wymagań
        cardServiceMock.Setup(x => x.GetCardDetailsAsync("User1", "Card5"))
            .ReturnsAsync(new Domain.Models.CardDetails("Card5", Domain.Enums.CardType.Credit, Domain.Enums.CardStatus.Blocked, true));

        // Konfiguracja dla karty DEBIT + ACTIVE - wszystkie akcje
        cardServiceMock.Setup(x => x.GetCardDetailsAsync("User2", "Card24"))
            .ReturnsAsync(new Domain.Models.CardDetails("Card24", Domain.Enums.CardType.Debit, Domain.Enums.CardStatus.Active, true));

        // Konfiguracja dla nieistniejących kart
        cardServiceMock.Setup(x => x.GetCardDetailsAsync("NonExistentUser", "NonExistentCard"))
            .ReturnsAsync((Domain.Models.CardDetails)null);
            
        services.AddSingleton<ICardService>(cardServiceMock.Object);
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
    ///     Konfiguruje serwis akcji karty.
    /// </summary>
    private void ConfigureCardActionService(IServiceCollection services)
    {
        var cardActionServiceMock = new Mock<CardActions.Domain.Services.Interfaces.ICardActionService>();
        
        // Konfiguracja dla karty PREPAID + CLOSED
        cardActionServiceMock.Setup(x => x.GetAllowedActionsAsync(
                Domain.Enums.CardType.Prepaid, 
                Domain.Enums.CardStatus.Closed, 
                true))
            .ReturnsAsync(new List<Domain.Models.CardAction>
            {
                Domain.Models.CardAction.Create("ACTION3"),
                Domain.Models.CardAction.Create("ACTION4"),
                Domain.Models.CardAction.Create("ACTION9")
            });
            
        // Konfiguracja dla karty CREDIT + BLOCKED
        cardActionServiceMock.Setup(x => x.GetAllowedActionsAsync(
                Domain.Enums.CardType.Credit, 
                Domain.Enums.CardStatus.Blocked, 
                true))
            .ReturnsAsync(new List<Domain.Models.CardAction>
            {
                Domain.Models.CardAction.Create("ACTION3"),
                Domain.Models.CardAction.Create("ACTION4"),
                Domain.Models.CardAction.Create("ACTION5"),
                Domain.Models.CardAction.Create("ACTION6"),
                Domain.Models.CardAction.Create("ACTION7"),
                Domain.Models.CardAction.Create("ACTION8"),
                Domain.Models.CardAction.Create("ACTION9")
            });
            
        // Konfiguracja dla karty DEBIT + ACTIVE
        cardActionServiceMock.Setup(x => x.GetAllowedActionsAsync(
                Domain.Enums.CardType.Debit, 
                Domain.Enums.CardStatus.Active, 
                true))
            .ReturnsAsync(new List<Domain.Models.CardAction>
            {
                Domain.Models.CardAction.Create("ACTION1"),
                Domain.Models.CardAction.Create("ACTION2"),
                Domain.Models.CardAction.Create("ACTION3"),
                Domain.Models.CardAction.Create("ACTION4"),
                Domain.Models.CardAction.Create("ACTION6"),
                Domain.Models.CardAction.Create("ACTION7"),
                Domain.Models.CardAction.Create("ACTION8")
            });
            
        services.AddScoped<CardActions.Domain.Services.Interfaces.ICardActionService>(
            sp => cardActionServiceMock.Object);
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