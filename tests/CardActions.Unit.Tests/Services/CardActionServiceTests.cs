using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Domain.Policies.Interfaces;
using CardActions.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardActions.Unit.Tests.Services;

public class CardActionServiceTests
{
    private readonly IReadOnlyList<string> _allActionNames;
    private readonly Mock<ICardActionPolicy> _policyMock;
    private readonly CardActionService _service;

    public CardActionServiceTests()
    {
        _policyMock = new Mock<ICardActionPolicy>();
        _allActionNames = new List<string>
        {
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION5",
            "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10",
            "ACTION11", "ACTION12", "ACTION13"
        };
        
        // Używam konstruktora tylko z loggerem
        var serviceLogger = new Mock<ILogger<CardActionService>>().Object;
        _service = new CardActionService(serviceLogger);
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Closed, true, new[] { "ACTION3", "ACTION4", "ACTION9" })]
    [InlineData(CardType.Credit, CardStatus.Blocked, true,
        new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" })]
    [InlineData(CardType.Debit, CardStatus.Active, true,
        new[]
        {
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10",
            "ACTION11", "ACTION12", "ACTION13"
        })]
    [InlineData(CardType.Credit, CardStatus.Restricted, false, new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION9" })]
    public async Task GetAllowedActionsAsync_ForVariousCardTypesAndStatuses_ShouldReturnCorrectActions(
        CardType cardType,
        CardStatus cardStatus,
        bool isPinSet,
        string[] expectedActions)
    {
        // Arrange: Ustawienia testowe nie są konieczne, ponieważ korzystamy z rzeczywistej implementacji

        // Act: Wywołujemy asynchroniczną metodę
        var result = await _service.GetAllowedActionsAsync(cardType, cardStatus, isPinSet);

        // Assert
        var resultNames = result.Select(a => a.Name).ToList();
        
        // Sprawdzamy czy wszystkie oczekiwane akcje są na liście
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction, 
                $"Akcja {expectedAction} powinna być dozwolona dla {cardType} w statusie {cardStatus} z PIN={isPinSet}");
        }
        
        // Sprawdzamy czy liczba akcji jest zgodna z oczekiwaniem
        result.Count.ShouldBe(expectedActions.Length,
            $"Liczba dozwolonych akcji dla {cardType} w statusie {cardStatus} z PIN={isPinSet} powinna wynosić {expectedActions.Length}, ale wynosi {result.Count}");
    }

    [Fact]
    public void GetAllActions_ShouldReturnAllActions()
    {
        // Act
        var result = _service.GetAllActions();

        // Assert
        result.Count.ShouldBe(_allActionNames.Count);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in _allActionNames)
        {
            resultNames.ShouldContain(expectedAction, 
                $"Lista wszystkich akcji powinna zawierać akcję {expectedAction}");
        }
    }
}