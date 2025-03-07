using CardActions.Api;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Services;
using CardActions.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using System.IO;

namespace CardActions.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Mockowanie serwisu lokalizacji
            var localizationServiceMock = new Mock<ILocalizationService>();
            localizationServiceMock.Setup(x => x.GetString(It.Is<string>(s => s == "Error.CardNotFound.Title")))
                .Returns("Card not found");
            localizationServiceMock.Setup(x => x.GetString(It.Is<string>(s => s == "Error.CardNotFound.Detail"), It.IsAny<object[]>()))
                .Returns("Card not found for specified user");
            localizationServiceMock.Setup(x => x.GetString(It.IsAny<string>()))
                .Returns((string key) => $"[{key}]");

            services.AddSingleton<ILocalizationService>(localizationServiceMock.Object);
            
            // Wymuszenie użycia dostawcy CSV
            services.AddSingleton<ICardActionRulesProvider>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<CardActionRulesProvider>>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                var csvPath = configuration["CardActionRulesPath"] ?? "../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv";
                return new CardActionRulesProvider(csvPath, logger);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Log.CloseAndFlush();
        }
    }
}