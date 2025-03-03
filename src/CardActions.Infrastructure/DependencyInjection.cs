using CardActions.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using CardActions.Application.Services;
using CardActions.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace CardActions.Infrastructure;

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
        services.AddDomain(configuration);
        
        // Rejestracja inicjalizatora infrastruktury

        // Rejestracja serwisów
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
        // Rejestracja CardActionRulesProvider z ścieżką do pliku CSV
        var csvPath = configuration["CardActionRulesPath"] 
            ?? throw new InvalidOperationException("CardActionRulesPath configuration is missing");
        services.AddSingleton<ICardActionRulesProvider>(sp => 
            new CardActionRulesProvider(csvPath, sp.GetRequiredService<ILogger<CardActionRulesProvider>>()));
            
        services.AddScoped<ICardService, CardService>();

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