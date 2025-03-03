using CardActions.Api;
using CardActions.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;

namespace CardActions.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var localizationServiceMock = new Mock<ILocalizationService>();
            localizationServiceMock.Setup(x => x.GetString(It.Is<string>(s => s == "Error.CardNotFound.Title")))
                .Returns("Card not found");
            localizationServiceMock.Setup(x => x.GetString(It.Is<string>(s => s == "Error.CardNotFound.Detail"), It.IsAny<object[]>()))
                .Returns("Card not found for specified user");
            localizationServiceMock.Setup(x => x.GetString(It.IsAny<string>()))
                .Returns((string key) => $"[{key}]");

            services.AddSingleton<ILocalizationService>(localizationServiceMock.Object);
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