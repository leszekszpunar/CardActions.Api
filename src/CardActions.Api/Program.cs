using System.Reflection;
using System.Text.Json;
using System.Threading.RateLimiting;
using CardActions.Api.Extensions;
using CardActions.Application;
using CardActions.Infrastructure;
using CardActions.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using Serilog;
using System.Globalization;
using CardActions.Api.Middleware;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using CardActions.Application.Common.Interfaces;
using CardActions.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var logger = Log.ForContext<Program>();

try
{
    logger.Information("Starting CardActions.Api application...");

    // Configure logging and monitoring
    builder.AddSerilogConfiguration();
    builder.Logging.AddLoggingConfiguration();
    builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);
    
    // Configure API
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure memory cache
    builder.Services.AddMemoryCache();
    
    // Register application layers
    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddInfrastructureData();

    // Add localization service
    builder.Services.AddSingleton<ILocalizationService, StringLocalizerWrapper>();

    // // Configure CORS
    // builder.Services.AddCors(options =>
    // {
    //     options.AddDefaultPolicy(policy =>
    //     {
    //         policy.AllowAnyOrigin()
    //               .AllowAnyHeader()
    //               .AllowAnyMethod();
    //     });
    // });

    // Configure API
    //builder.Services.AddApiConfiguration();
    builder.Services.AddRateLimitingConfiguration();
    builder.Services.AddSwaggerDocumentation(builder.Configuration);
    
    // Configure health checks
    builder.Services.AddHealthChecks();

    // Add localization
    builder.Services.AddLocalizationConfiguration();

    var app = builder.Build();

    // Configure HTTP pipeline
    app.UseRouting();
    //app.UseCors();

    // Configure API
    app.MapControllers();
    
    // Configure middleware
    app.UseSwaggerDocumentation(builder.Configuration);

    // Use localization
    app.UseRequestLocalization();

    // Add middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Obsługa błędów 404 i innych
    app.UseStatusCodePages();

    // Redirect root to docs
    app.MapGet("/", () => Results.Redirect("/docs"));
    
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var addresses = app.Services.GetService<IServer>()?.Features?.Get<IServerAddressesFeature>()?.Addresses;
        var baseUrl = addresses?.FirstOrDefault() ?? "http://localhost:8080";
        
        logger.Information("CardActions.Api application started successfully");
        logger.Information("Base URL: {BaseUrl}", baseUrl);
        logger.Information("ReDoc documentation available at: {BaseUrl}/docs", baseUrl);
        logger.Information("Swagger UI available at: {BaseUrl}/swagger", baseUrl);
        logger.Information("Health check endpoint available at: {BaseUrl}/health", baseUrl);
        logger.Information("Prometheus metrics available at: {BaseUrl}/metrics", baseUrl);
    });

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

namespace CardActions.Api
{
    // Class needed for integration tests
    public class Program { }
}