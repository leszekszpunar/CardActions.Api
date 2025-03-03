using System.Net;
using System.Net.Http.Json;
using CardActions.Api;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace CardActions.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureServices(services =>
        {
            // Remove the existing ILocalizationService registration if it exists
            // var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ILocalizationService));
            // if (descriptor != null)
            // {
            //     services.Remove(descriptor);
            // }

            // Add mock ILocalizationService
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

public class CardActionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ILogger<CardActionsControllerTests> _logger;

    public CardActionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        var loggerFactory = _factory.Services.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<CardActionsControllerTests>();
    }

    [Theory]
    [InlineData("User1", "Card17", new[] { "ACTION3", "ACTION4", "ACTION9" })] // Prepaid Closed
    [InlineData("User1", "Card119", new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION8", "ACTION9" })] // Credit Blocked bez PIN
    [InlineData("User1", "Card118", new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" })] // Credit Restricted z PIN
    [InlineData("User1", "Card13", new[] { "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" })] // Debit Active bez PIN
    public async Task GetAllowedActions_ForValidCard_ShouldReturnExpectedActions(string userId, string cardNumber, string[] expectedActions)
    {
        // Act
        _logger.LogInformation("Testing GetAllowedActions for user {UserId} and card {CardNumber}", userId, cardNumber);
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetAllowedCardActionsResponse>();
        result.ShouldNotBeNull();
        result.AllowedActions.ShouldBe(expectedActions);
    }

    [Theory]
    [InlineData("User999", "Card17")] // Nieistniejący użytkownik
    [InlineData("User1", "Card999")] // Nieistniejąca karta
    public async Task GetAllowedActions_ForNonExistentCard_ShouldReturnNotFound(string userId, string cardNumber)
    {
        // Act
        _logger.LogInformation("Testing GetAllowedActions for non-existent card: user {UserId} and card {CardNumber}", userId, cardNumber);
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");
        
        // Assert
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Expected NotFound but got {StatusCode}. Response content: {Content}", 
                response.StatusCode, content);
        }
        
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
} 