using CardActions.Application.Services;
using CardActions.Application.Services.Interfaces;
using CardActions.Domain;
using CardActions.Domain.Policies;
using CardActions.Domain.Policies.Interfaces;
using CardActions.Domain.Services;
using CardActions.Domain.Services.Interfaces;
using CardActions.Infrastructure.Services;
using CardActions.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace CardActions.Infrastructure;

/// <summary>
///     Klasa rozszerzająca konfigurację dla warstwy infrastruktury.
///     Architektura projektu opiera się na zasadach Domain-Driven Design (DDD) z wyraźnym podziałem na warstwy:
///     - Domain - zawiera modele domenowe, polityki i podstawowe reguły biznesowe
///     - Application - zawiera przypadki użycia i logikę aplikacji
///     - Infrastructure - zawiera implementacje techniczne interfejsów zdefiniowanych w wyższych warstwach
///     - API - warstwa prezentacji (kontrolery)
///     Projekt wykorzystuje następujące wzorce projektowe:
///     - Dependency Injection (DI) - wstrzykiwanie zależności przez konstruktor
///     - Repository Pattern - oddzielenie logiki dostępu do danych od logiki biznesowej
///     - Strategy Pattern - różne strategie określania dozwolonych akcji
///     - Policy Pattern - enkapsulacja reguł biznesowych w osobnych klasach
///     - Value Object - niemutowalne obiekty wartościowe
///     - Factory Method - fabryki do tworzenia obiektów
///     Jakość kodu zapewniają:
///     - Zasady SOLID
///     - Obsługa błędów i walidacja danych wejściowych
///     - Niemutowalność obiektów (immutability)
///     - Interfejsy dla kluczowych komponentów
///     - System regułowy oparty na konfiguracji
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Dodaje usługi infrastruktury do kontenera DI.
    /// </summary>
    /// <param name="services">Kolekcja usług.</param>
    /// <param name="configuration">Konfiguracja aplikacji.</param>
    /// <returns>Kolekcja usług z dodanymi usługami infrastruktury.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Serilog
        ConfigureSerilog(configuration);

        // Add domain services
        services.AddDomain();

        // Rejestracja inicjalizatora infrastruktury

        // Rejestracja serwisów
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Rejestracja loadera reguł akcji karty
        services.AddSingleton<ICardActionRulesLoader>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CsvCardActionRulesLoader>>();
            var csvPath = configuration["CardActionRulesPath"] ??
                          Path.Combine(AppContext.BaseDirectory, "Resources", "Allowed_Actions_Table.csv");
            return new CsvCardActionRulesLoader(csvPath, logger);
        });

        // Rejestracja dostawcy reguł akcji
        services.AddSingleton<ICardActionRulesProvider>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CardActionRulesProvider>>();
            var rulesLoader = provider.GetRequiredService<ICardActionRulesLoader>();
            return new CardActionRulesProvider(rulesLoader, logger);
        });

        services.AddScoped<ICardService, CardService>();

        // Rejestracja polityk domenowych
        services.AddSingleton<ICardActionPolicy>(provider =>
        {
            var rulesProvider = provider.GetRequiredService<ICardActionRulesProvider>();
            return new CardActionPolicy(rulesProvider.GetAllRules());
        });

        // Rejestracja serwisów domenowych
        services.AddScoped<ICardActionService>(provider =>
        {
            var policy = provider.GetRequiredService<ICardActionPolicy>();
            var rulesProvider = provider.GetRequiredService<ICardActionRulesProvider>();
            return new CardActionService(policy, rulesProvider.GetAllActionNames());
        });

        // Rejestracja serwisu wersji
        services.AddScoped<IVersionService, VersionService>();

        return services;
    }

    /// <summary>
    ///     Konfiguruje Serilog na podstawie ustawień z konfiguracji.
    /// </summary>
    /// <param name="configuration">Konfiguracja aplikacji.</param>
    private static void ConfigureSerilog(IConfiguration configuration)
    {
        var logToConsole = configuration.GetValue("Logging:LogToConsole", true);
        var logToFile = configuration.GetValue("Logging:LogToFile", true);
        var logLevel = configuration.GetValue<string>("Logging:LogLevel:Default", "Information");
        var logPath = configuration.GetValue<string>("Logging:FilePath", "logs/cardactions-.log");

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(ParseLogLevel(logLevel))
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId();

        if (logToConsole) loggerConfiguration.WriteTo.Console();

        if (logToFile)
        {
            var logDirectory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            loggerConfiguration.WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }

    /// <summary>
    ///     Parsuje poziom logowania z tekstu na odpowiednią wartość enum.
    /// </summary>
    /// <param name="logLevel">Tekstowa reprezentacja poziomu logowania.</param>
    /// <returns>Wartość enum reprezentująca poziom logowania.</returns>
    private static LogEventLevel ParseLogLevel(string logLevel)
    {
        return logLevel?.ToLower() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    ///     Inicjalizuje infrastrukturę aplikacji.
    /// </summary>
    /// <param name="serviceProvider">Provider usług.</param>
    /// <returns>Task reprezentujący operację asynchroniczną.</returns>
    public static Task InitializeInfrastructureAsync(this IServiceProvider serviceProvider)
    {
        // Używamy ILoggerFactory zamiast ILogger<DependencyInjection>
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("CardActions.Infrastructure.Initialization");

        logger.LogInformation("Starting infrastructure initialization...");

        try
        {
            // Inicjalizacja infrastruktury

            logger.LogInformation("Infrastructure initialization completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during infrastructure initialization");
            throw;
        }

        return Task.CompletedTask;
    }
}