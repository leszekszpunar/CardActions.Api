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

    // Configure API
    //builder.Services.AddApiConfiguration();
    builder.Services.AddRateLimitingConfiguration();
    builder.Services.AddSwaggerDocumentation(builder.Configuration);
    
    // Configure health checks
    builder.Services.AddHealthChecks();

    // Add localization
    builder.Services.AddLocalizationConfiguration();

    var app = builder.Build();

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
    
    logger.Information("CardActions.Api application started successfully");
    logger.Information("ReDoc documentation available at: http://localhost:5003/docs");
    logger.Information("Swagger UI available at: http://localhost:5003/swagger");
    logger.Information("Health check endpoint available at: http://localhost:5003/health");
    logger.Information("Prometheus metrics available at: http://localhost:5003/metrics");

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