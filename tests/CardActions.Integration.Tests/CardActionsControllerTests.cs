using System.Net;
using System.Net.Http.Json;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using CardActions.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace CardActions.Integration.Tests;

/// <summary>
///     Testy integracyjne dla kontrolera CardActions, weryfikujące poprawność odpowiedzi API
///     na zapytania o dozwolone akcje dla kart użytkowników.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Component", "API")]
public class CardActionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly ILogger<CardActionsControllerTests> _logger;

    public CardActionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        var loggerFactory = _factory.Services.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<CardActionsControllerTests>();
    }

    [Theory(DisplayName =
        "Endpoint /api/users/{userId}/cards/{cardNumber}/actions powinien zwrócić prawidłowe akcje dla istniejącej karty")]
    [InlineData("User1", "Card17", new[] { "ACTION3", "ACTION4", "ACTION9" }, "PREPAID + CLOSED - przykład z wymagań")]
    [InlineData("User1", "Card5", new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" }, "CREDIT + BLOCKED - przykład z wymagań")]
    [InlineData("User2", "Card24", new[] { "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7", "ACTION8" }, "DEBIT + ACTIVE - wszystkie akcje")]
    public async Task GetAllowedActions_ForValidCard_ShouldReturnExpectedActions(
        string userId,
        string cardNumber,
        string[] expectedActions,
        string testCase)
    {
        // Arrange
        _logger.LogInformation("Testing GetAllowedActions for user {UserId} and card {CardNumber} - {TestCase}",
            userId, cardNumber, testCase);

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Expected OK but got {StatusCode}. Response content: {Content}",
                response.StatusCode, content);
        }

        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"Endpoint powinien zwrócić status 200 OK dla {testCase}");

        var result = await response.Content.ReadFromJsonAsync<GetAllowedCardActionsResponse>();
        result.ShouldNotBeNull("Odpowiedź nie powinna być null");

        // 🛠 Logujemy zwrócone wartości:
        _logger.LogInformation("Received allowed actions: {Actions}", string.Join(", ", result.AllowedActions));
        _logger.LogInformation("Expected actions: {ExpectedActions}", string.Join(", ", expectedActions));

        if (!result.AllowedActions.SequenceEqual(expectedActions))
            _logger.LogError("Mismatch! Expected: {Expected}, but received: {Actual}",
                string.Join(", ", expectedActions), string.Join(", ", result.AllowedActions));

        result.AllowedActions.ShouldBe(expectedActions,
            $"Zwrócone akcje powinny być zgodne z oczekiwanymi dla {testCase}");
    }

    [Theory(DisplayName =
        "Endpoint /api/users/{userId}/cards/{cardNumber}/actions powinien zwrócić 404 NotFound dla nieistniejącej karty lub użytkownika")]
    [InlineData("User999", "Card17", "Nieistniejący użytkownik")]
    [InlineData("User1", "Card999", "Nieistniejąca karta")]
    public async Task GetAllowedActions_ForNonExistentCard_ShouldReturnNotFound(
        string userId,
        string cardNumber,
        string testCase)
    {
        // Arrange
        _logger.LogInformation(
            "Testing GetAllowedActions for non-existent card: user {UserId} and card {CardNumber} - {TestCase}",
            userId, cardNumber, testCase);

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound, 
            $"Endpoint powinien zwrócić status 404 NotFound dla {testCase}");
    }
    
    [Theory(DisplayName =
        "Endpoint /api/users/{userId}/cards/{cardNumber}/actions z pustymi parametrami nie powinien poprawnie dopasować ścieżki")]
    [InlineData("", "Card17", "Brak ID użytkownika")]
    [InlineData("User1", "", "Brak numeru karty")]
    public async Task GetAllowedActions_WithEmptyRouteParams_ShouldNotMatch(
        string userId,
        string cardNumber,
        string testCase)
    {
        // Arrange
        _logger.LogInformation(
            "Testing GetAllowedActions with invalid route parameters: user {UserId} and card {CardNumber} - {TestCase}",
            userId, cardNumber, testCase);

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/cards/{cardNumber}/actions");
        
        // Assert
        // ASP.NET Core traktuje puste segmenty URL jako niedopasowanie trasy, co skutkuje kodem 404
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound, 
            $"Endpoint z pustym parametrem w URL powinien zwrócić status 404 NotFound dla {testCase}");
    }

    [Theory(DisplayName = "Metoda ParseRuleValue powinna poprawnie interpretować wartości z tabeli CSV")]
    [InlineData("TAK", true, null, "Bezwarunkowe TAK")]
    [InlineData("NIE", false, null, "Bezwarunkowe NIE")]
    [InlineData("TAK - ale jak nie ma pin to NIE", true, true, "TAK z warunkiem PIN ustawiony")]
    [InlineData("TAK - jeżeli pin nadany", true, true, "TAK gdy PIN nadany")]
    [InlineData("TAK - jeżeli brak pin", true, false, "TAK gdy PIN nie jest ustawiony")]
    public void ParseRuleValue_ShouldReturnCorrectValues(
        string value,
        bool expectedIsAllowed,
        bool? expectedRequiresPin,
        string testCase)
    {
        // Arrange & Act
        var (isAllowed, requiresPin) = CsvCardActionRulesLoader.ParseRuleValue(value);

        // Assert
        isAllowed.ShouldBe(expectedIsAllowed,
            $"Niepoprawna interpretacja wartości dozwolonej dla '{value}' - {testCase}");
        requiresPin.ShouldBe(expectedRequiresPin, $"Niepoprawna interpretacja warunku PIN dla '{value}' - {testCase}");
    }
}