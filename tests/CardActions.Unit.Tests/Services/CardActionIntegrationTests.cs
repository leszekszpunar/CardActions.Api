using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CardActions.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace CardActions.Unit.Tests.Services;

/// <summary>
///     Testy integracyjne dla CardActionService z użyciem testowej implementacji ICardActionRulesProvider.
/// </summary>
public class CardActionIntegrationTests
{
    private readonly CardActionPolicy _policy;
    private readonly TestCardActionRulesProvider _rulesProvider;
    private readonly CardActionService _service;
    private readonly ITestOutputHelper _testOutputHelper;

    public CardActionIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _rulesProvider = new TestCardActionRulesProvider();
        _policy = new CardActionPolicy(_rulesProvider.GetAllRules());
        
        // Używam konstruktora tylko z loggerem
        var serviceLogger = new Mock<ILogger<CardActionService>>().Object;
        _service = new CardActionService(serviceLogger);
    }

    [Fact]
    public async Task GetAllowedActions_ForActiveDebitCard_ShouldReturnCorrectActions()
    {
        // Arrange
        var expectedActions = new[]
        {
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7",
            "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13"
        };

        // Act
        var result = await _service.GetAllowedActionsAsync(CardType.Debit, CardStatus.Active, true);

        // Assert
        var resultNames = result.Select(a => a.Name).ToList();
        _testOutputHelper.WriteLine($"Znalezione akcje ({result.Count}): {string.Join(", ", resultNames)}");
        
        // Sprawdzamy czy wszystkie oczekiwane akcje są na liście
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction, 
                $"Akcja {expectedAction} powinna być dozwolona dla karty Debit w statusie Active z ustawionym PIN");
        }
        
        // Sprawdzamy czy liczba akcji jest zgodna z oczekiwaniem
        result.Count.ShouldBe(expectedActions.Length,
            $"Liczba dozwolonych akcji dla karty Debit w statusie Active z ustawionym PIN powinna wynosić {expectedActions.Length}, ale wynosi {result.Count}");
    }

    [Fact]
    public async Task GetAllowedActions_ForBlockedCreditCard_ShouldReturnCorrectActions()
    {
        // Arrange
        var expectedActions = new[]
        {
            "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9"
        };

        // Act
        var result = await _service.GetAllowedActionsAsync(CardType.Credit, CardStatus.Blocked, true);

        // Assert
        var resultNames = result.Select(a => a.Name).ToList();
        _testOutputHelper.WriteLine($"Znalezione akcje ({result.Count}): {string.Join(", ", resultNames)}");
        
        // Sprawdzamy czy wszystkie oczekiwane akcje są na liście
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction, 
                $"Akcja {expectedAction} powinna być dozwolona dla karty Credit w statusie Blocked z ustawionym PIN");
        }
        
        // Sprawdzamy czy liczba akcji jest zgodna z oczekiwaniem
        result.Count.ShouldBe(expectedActions.Length,
            $"Liczba dozwolonych akcji dla karty Credit w statusie Blocked z ustawionym PIN powinna wynosić {expectedActions.Length}");
    }

    [Fact]
    public async Task GetAllowedActions_ForOrderedPrepaidCardWithoutPin_ShouldReturnCorrectActions()
    {
        // Arrange
        var expectedActions = new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION8", "ACTION9", "ACTION10", "ACTION12", "ACTION13" };
        
        // Act
        var result = await _service.GetAllowedActionsAsync(CardType.Prepaid, CardStatus.Ordered, false);

        // Assert
        var resultNames = result.Select(a => a.Name).ToList();
        _testOutputHelper.WriteLine($"Znalezione akcje ({result.Count}): {string.Join(", ", resultNames)}");
        
        // Sprawdzamy zgodnie z tabelą: "TAK - ale jak nie ma pin to NIE"
        resultNames.ShouldNotContain("ACTION6", "ACTION6 nie powinna być dozwolona dla karty Prepaid w statusie Ordered bez PIN");
        
        // Sprawdzamy zgodnie z tabelą: "TAK - jeżeli brak pin"
        resultNames.ShouldContain("ACTION7", "ACTION7 powinna być dozwolona dla karty Prepaid w statusie Ordered bez PIN");
        
        // Sprawdzamy czy liczba akcji jest zgodna z oczekiwaniem
        result.Count.ShouldBe(expectedActions.Length,
            $"Liczba dozwolonych akcji dla karty Prepaid w statusie Ordered bez PIN powinna wynosić {expectedActions.Length}");
    }

    [Fact]
    public async Task GetAllowedActions_ForOrderedPrepaidCardWithPin_ShouldReturnCorrectActions()
    {
        // Arrange
        var expectedActions = new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION8", "ACTION9", "ACTION10", "ACTION12", "ACTION13" };
        
        // Act
        var result = await _service.GetAllowedActionsAsync(CardType.Prepaid, CardStatus.Ordered, true);

        // Assert
        var resultNames = result.Select(a => a.Name).ToList();
        _testOutputHelper.WriteLine($"Znalezione akcje ({result.Count}): {string.Join(", ", resultNames)}");
        
        // Sprawdzamy zgodnie z tabelą: "TAK - ale jak nie ma pin to NIE"
        resultNames.ShouldContain("ACTION6", "ACTION6 powinna być dozwolona dla karty Prepaid w statusie Ordered z PIN");
        
        // Sprawdzamy zgodnie z tabelą: "TAK - jeżeli brak pin"
        resultNames.ShouldNotContain("ACTION7", "ACTION7 nie powinna być dozwolona dla karty Prepaid w statusie Ordered z PIN");
        
        // Sprawdzamy czy liczba akcji jest zgodna z oczekiwaniem
        result.Count.ShouldBe(expectedActions.Length,
            $"Liczba dozwolonych akcji dla karty Prepaid w statusie Ordered z PIN powinna wynosić {expectedActions.Length}");
    }

    [Fact]
    public async Task GetAllowedActions_ForAllCardTypesAndStatuses_ShouldAlwaysIncludeBasicActions()
    {
        // Arrange
        var basicActions = new[] { "ACTION3", "ACTION4" };

        // Act & Assert
        foreach (var cardType in Enum.GetValues<CardType>())
        foreach (var cardStatus in Enum.GetValues<CardStatus>())
        foreach (var isPinSet in new[] { true, false })
        {
            var result = await _service.GetAllowedActionsAsync(cardType, cardStatus, isPinSet);
            var resultNames = result.Select(a => a.Name).ToList();
            
            _testOutputHelper.WriteLine($"Testowanie dla {cardType} w statusie {cardStatus} z isPinSet={isPinSet}");
            _testOutputHelper.WriteLine($"Znalezione akcje ({result.Count}): {string.Join(", ", resultNames)}");

            foreach (var basicAction in basicActions)
                resultNames.ShouldContain(basicAction,
                    $"Akcja {basicAction} powinna być dozwolona dla karty {cardType} w statusie {cardStatus} z isPinSet={isPinSet}");
        }
    }
}