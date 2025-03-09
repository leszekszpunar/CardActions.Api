using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace CardActions.Api.Extensions;

/// <summary>
///     Rozszerzenia konfiguracyjne dla logowania i monitorowania
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    ///     Dodaje konfigurację logowania do usług
    /// </summary>
    public static ILoggingBuilder AddLoggingConfiguration(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        return logging;
    }

    /// <summary>
    ///     Konfiguruje Serilog dla aplikacji
    /// </summary>
    public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug()
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    ///     Dodaje konfigurację OpenTelemetry do usług
    /// </summary>
    public static IServiceCollection AddOpenTelemetryConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService("CardActions.Api",
                serviceVersion: typeof(LoggingExtensions).Assembly.GetName().Version?.ToString() ?? "unknown")
            .AddAttributes(new Dictionary<string, object>
            {
                ["environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production",
                ["application"] = "CardActions.Api"
            });

        services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter())
            .WithTracing(tracing => tracing
                .AddSource("CardActions.Api")
                .SetResourceBuilder(resourceBuilder)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation());

        return services;
    }
}