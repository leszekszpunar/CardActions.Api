using System.Net;
using System.Net.Http.Json;
using CardActions.Api;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using CardActions.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CardActions.Integration.Tests.Controllers;

public class CardActionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CardActionsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Tutaj możemy podmienić serwisy na testowe implementacje jeśli potrzeba
            });
        });
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("User1", "Card17", new[] { "ACTION3", "ACTION4", "ACTION9" })] // PREPAID CLOSED
    [InlineData("User1", "Card26", new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" })] // CREDIT BLOCKED with PIN
    [InlineData("User1", "Card25", new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION8", "ACTION9" })] // CREDIT BLOCKED without PIN
    [InlineData("User1", "Card12", new[] { "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" })] // DEBIT ACTIVE
    public async Task GetAllowedActions_ForValidCards_ShouldReturnCorrectActions(
        string userId,
        string cardNumber,
        string[] expectedActions)
    {
        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetAllowedCardActionsResponse>();
        result.ShouldNotBeNull();
        result.AllowedActions.ShouldBe(expectedActions);
    }

    [Theory]
    [InlineData("User1", "NonExistentCard")]
    [InlineData("NonExistentUser", "Card11")]
    public async Task GetAllowedActions_ForNonExistentCards_ShouldReturn404(string userId, string cardNumber)
    {
        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", "Card11")]
    [InlineData("User1", "")]
    [InlineData(" ", "Card11")]
    [InlineData("User1", " ")]
    [InlineData(null, "Card11")]
    [InlineData("User1", null)]
    public async Task GetAllowedActions_WithInvalidParameters_ShouldReturn400(string? userId, string? cardNumber)
    {
        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllowedActions_WithConcurrentRequests_ShouldHandleMultipleRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_client.GetAsync("/api/users/User1/cards/Card11/actions"));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.All(r => r.IsSuccessStatusCode).ShouldBeTrue();
        foreach (var response in responses)
        {
            var result = await response.Content.ReadFromJsonAsync<GetAllowedCardActionsResponse>();
            result.ShouldNotBeNull();
            result.AllowedActions.ShouldNotBeEmpty();
        }
    }

    [Fact]
    public async Task GetAllowedActions_ShouldRespectRateLimiting()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 100; i++) // Próba wykonania dużej liczby requestów w krótkim czasie
        {
            tasks.Add(_client.GetAsync("/api/users/User1/cards/Card11/actions"));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests).ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("User1", "Card11")] // PREPAID ORDERED without PIN
    [InlineData("User1", "Card24")] // CREDIT RESTRICTED
    public async Task GetAllowedActions_ShouldReturnCorrectContentType(string userId, string cardNumber)
    {
        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Fact]
    public async Task GetAllowedActions_ShouldHandleOptionsRequest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/users/User1/cards/Card11/actions");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.Contains("Allow").ShouldBeTrue();
    }

    [Theory]
    [InlineData("User1", "Card11", "GET")]
    [InlineData("User1", "Card11", "POST")] // Powinno zwrócić 405 Method Not Allowed
    [InlineData("User1", "Card11", "PUT")] // Powinno zwrócić 405 Method Not Allowed
    [InlineData("User1", "Card11", "DELETE")] // Powinno zwrócić 405 Method Not Allowed
    public async Task GetAllowedActions_ShouldHandleHttpMethods(string userId, string cardNumber, string method)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), $"/api/users/{userId}/cards/{cardNumber}/actions");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        if (method == "GET")
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
        else
        {
            response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        }
    }

    [Theory]
    [InlineData("User1", "Card11", "application/json")]
    [InlineData("User1", "Card11", "application/xml")] // Powinno nadal zwrócić JSON
    [InlineData("User1", "Card11", "*/*")]
    public async Task GetAllowedActions_ShouldHandleAcceptHeaders(string userId, string cardNumber, string acceptHeader)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}/cards/{cardNumber}/actions");
        request.Headers.Add("Accept", acceptHeader);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Theory]
    [InlineData("pl-PL", "Numer karty może zawierać tylko litery i cyfry")]
    [InlineData("en-US", "Card number can only contain letters and numbers")]
    public async Task GetAllowedActions_WithInvalidData_ShouldReturnLocalizedValidationMessage(
        string culture, 
        string expectedMessage)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/User1/cards/Card@123/actions");
        request.Headers.AcceptLanguage.ParseAdd(culture);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain(expectedMessage);
    }

    [Theory]
    [InlineData("pl-PL")]
    [InlineData("en-US")]
    [InlineData("fr-FR")] // Nieobsługiwana kultura powinna zwrócić domyślną (pl-PL)
    public async Task GetAllowedActions_WithDifferentCultures_ShouldRespectAcceptLanguageHeader(string culture)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/User1/cards/Card-123!/actions");
        request.Headers.AcceptLanguage.ParseAdd(culture);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (culture == "pl-PL" || culture == "fr-FR") // fr-FR powinno użyć domyślnej kultury (pl-PL)
        {
            content.ShouldContain("Numer karty może zawierać tylko litery i cyfry");
        }
        else if (culture == "en-US")
        {
            content.ShouldContain("Card number can only contain letters and numbers");
        }
    }
}