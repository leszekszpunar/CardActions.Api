using System.Net;
using System.Net.Http.Json;
using CardActions.Api;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using CardActions.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
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
    [InlineData("User1", "Card17", new[] { "ACTION3", "ACTION4", "ACTION9" })]
    public async Task GetAllowedActions_ForValidCard_ShouldReturnExpectedActions(string userId, string cardNumber, string[] expectedActions)
    {
        _logger.LogInformation("Testing GetAllowedActions for user {UserId} and card {CardNumber}", userId, cardNumber);
    
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Expected OK but got {StatusCode}. Response content: {Content}", response.StatusCode, content);
        }

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetAllowedCardActionsResponse>();
        result.ShouldNotBeNull();

        // 🛠 Logujemy zwrócone wartości:
        _logger.LogInformation("Received allowed actions: {Actions}", string.Join(", ", result.AllowedActions));
        _logger.LogInformation("Expected actions: {ExpectedActions}", string.Join(", ", expectedActions));

        if (!result.AllowedActions.SequenceEqual(expectedActions))
        {
            _logger.LogError("Mismatch! Expected: {Expected}, but received: {Actual}", 
                string.Join(", ", expectedActions), string.Join(", ", result.AllowedActions));
        }

        result.AllowedActions.ShouldBe(expectedActions);
    }


    [Theory]
    [InlineData("User999", "Card17")] // Nieistniejący użytkownik
    [InlineData("User1", "Card999")] // Nieistniejąca karta
    public async Task GetAllowedActions_ForNonExistentCard_ShouldReturnNotFound(string userId, string cardNumber)
    {
        _logger.LogInformation("Testing GetAllowedActions for non-existent card: user {UserId} and card {CardNumber}", userId, cardNumber);
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");

        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Expected NotFound but got {StatusCode}. Response content: {Content}", 
                response.StatusCode, content);
        }

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("TAK", true, null)]
    [InlineData("NIE", false, null)]
    [InlineData("TAK - ale jak nie ma pin to NIE", false, false)]
    [InlineData("TAK - jeżeli pin nadany", true, true)]
    [InlineData("TAK - jeżeli brak pin", true, false)]
    public void ParseRuleValue_ShouldReturnCorrectValues(string value, bool expectedIsAllowed, bool? expectedRequiresPin)
    {
        var (isAllowed, requiresPin) = CardActionRulesProvider.ParseRuleValue(value);
        isAllowed.ShouldBe(expectedIsAllowed);
        requiresPin.ShouldBe(expectedRequiresPin);
    }
}
