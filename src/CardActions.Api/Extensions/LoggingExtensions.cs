using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace CardActions.Api.Extensions;

/// <summary>
/// Configuration extensions for logging and monitoring
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds logging configuration to services
    /// </summary>
    public static ILoggingBuilder AddLoggingConfiguration(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        return logging;
    }

    /// <summary>
    /// Configures Serilog for the application
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
    /// Adds OpenTelemetry configuration to services
    /// </summary>
    public static IServiceCollection AddOpenTelemetryConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: "CardActions.Api",
                serviceVersion: typeof(LoggingExtensions).Assembly.GetName().Version?.ToString() ?? "unknown");

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