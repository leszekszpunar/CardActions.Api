using CardActions.Domain.Enums;
using CardActions.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CardActions.Domain.Tests.Services;

public class CardActionServiceTests
{
    private readonly Mock<ILogger<CardActionService>> _loggerMock;
    private readonly CardActionService _service;

    public CardActionServiceTests()
    {
        _loggerMock = new Mock<ILogger<CardActionService>>();
        _service = new CardActionService(_loggerMock.Object);
    }

    [Fact]
    public void GetAllActions_ShouldReturnAllAvailableActions()
    {
        // Act
        var result = _service.GetAllActions();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(13, result.Count); // Sprawdzamy, czy załadowano wszystkie 13 standardowych akcji
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Active, true)]
    [InlineData(CardType.Debit, CardStatus.Active, true)]
    [InlineData(CardType.Credit, CardStatus.Active, true)]
    public void GetAllowedActions_ForActiveCards_ShouldReturnAvailableActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Act
        var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Aktywne karty powinny mieć dostępną akcję ACTION1 (ActiveCardsOnlyStrategy)
        Assert.Contains(result, a => a.Name == "ACTION1");
        
        // Aktywne karty powinny mieć dostępną akcję ACTION2 (BasicOperationalStatusStrategy)
        Assert.Contains(result, a => a.Name == "ACTION2");
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Blocked, true)]
    [InlineData(CardType.Debit, CardStatus.Blocked, true)]
    [InlineData(CardType.Credit, CardStatus.Blocked, true)]
    public void GetAllowedActions_ForBlockedCards_ShouldExcludeActiveOnlyActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Act
        var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        Assert.NotNull(result);
        
        // Zablokowane karty nie powinny mieć dostępnej akcji ACTION1 (ActiveCardsOnlyStrategy)
        Assert.DoesNotContain(result, a => a.Name == "ACTION1");
        
        // Zablokowane karty powinny mieć dostępną akcję ACTION4 (AlwaysAvailableStrategy)
        Assert.Contains(result, a => a.Name == "ACTION4");
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Active, false)]
    [InlineData(CardType.Debit, CardStatus.Active, false)]
    [InlineData(CardType.Credit, CardStatus.Active, false)]
    public void GetAllowedActions_ForActiveCardsWithoutPin_ShouldExcludePinDependentActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Act
        var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Karty bez PINu nie powinny mieć dostępu do ACTION6 (ComplexPinDependentStrategy)
        Assert.DoesNotContain(result, a => a.Name == "ACTION6");
    }

    [Fact]
    public async Task GetAllowedActionsAsync_ShouldReturnSameResultsAsSync()
    {
        // Arrange
        var cardType = CardType.Debit;
        var cardStatus = CardStatus.Active;
        var isPinSet = true;

        // Act
        var syncResult = _service.GetAllowedActions(cardType, cardStatus, isPinSet);
        var asyncResult = await _service.GetAllowedActionsAsync(cardType, cardStatus, isPinSet);

        // Assert
        Assert.Equal(syncResult.Count, asyncResult.Count);
        foreach (var action in syncResult)
        {
            Assert.Contains(asyncResult, a => a.Name == action.Name);
        }
    }
    
    [Fact]
    public async Task ExecuteActionAsync_ShouldReturnSuccessfulResult_ForAvailableAction()
    {
        // Arrange
        var actionName = "ACTION1"; // Akcja dostępna dla aktywnych kart
        var cardType = CardType.Debit;
        var cardStatus = CardStatus.Active;
        var isPinSet = true;

        // Act
        var result = await _service.ExecuteActionAsync(actionName, cardType, cardStatus, isPinSet);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.Contains("Wykonano akcję", result.Message);
    }
    
    [Fact]
    public async Task ExecuteActionAsync_ShouldReturnFailureResult_ForUnavailableAction()
    {
        // Arrange
        var actionName = "ACTION1"; // Akcja niedostępna dla zablokowanych kart
        var cardType = CardType.Debit;
        var cardStatus = CardStatus.Blocked;
        var isPinSet = true;

        // Act
        var result = await _service.ExecuteActionAsync(actionName, cardType, cardStatus, isPinSet);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccessful);
        Assert.Equal("Akcja nie jest dostępna dla tej karty", result.Message);
    }
    
    [Fact]
    public async Task ExecuteActionAsync_ShouldReturnFailureResult_ForNonexistentAction()
    {
        // Arrange
        var actionName = "NONEXISTENT_ACTION";
        var cardType = CardType.Debit;
        var cardStatus = CardStatus.Active;
        var isPinSet = true;

        // Act
        var result = await _service.ExecuteActionAsync(actionName, cardType, cardStatus, isPinSet);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccessful);
        Assert.Contains("Nie znaleziono akcji", result.Message);
    }
} 