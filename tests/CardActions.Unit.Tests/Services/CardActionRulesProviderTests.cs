using CardActions.Domain.Models;
using CardActions.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.IO;
using Xunit;

namespace CardActions.Unit.Tests.Services;

public class CardActionRulesProviderTests
{
    private readonly Mock<ILogger<CardActionRulesProvider>> _loggerMock;
    private readonly string _csvPath;

    public CardActionRulesProviderTests()
    {
        _loggerMock = new Mock<ILogger<CardActionRulesProvider>>();
        _csvPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv"));
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Closed, true, new[] { "ACTION3", "ACTION4", "ACTION9" })]
    [InlineData(CardType.Credit, CardStatus.Blocked, true, new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" })]
    [InlineData(CardType.Debit, CardStatus.Active, true, new[] { "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" })]
    [InlineData(CardType.Credit, CardStatus.Restricted, false, new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION9" })]
    public void GetAllowedActions_ForVariousCardTypesAndStatuses_ShouldReturnCorrectActions(
        CardType cardType, 
        CardStatus cardStatus, 
        bool isPinSet, 
        string[] expectedActions)
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);

        // Act
        var result = provider.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedActions);
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, true)]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, false)]
    [InlineData(CardType.Debit, CardStatus.Ordered, true)]
    [InlineData(CardType.Debit, CardStatus.Ordered, false)]
    [InlineData(CardType.Credit, CardStatus.Ordered, true)]
    [InlineData(CardType.Credit, CardStatus.Ordered, false)]
    public void GetAllowedActions_ForOrderedCards_ShouldHandlePinStatusCorrectly(
        CardType cardType, 
        CardStatus cardStatus, 
        bool isPinSet)
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);

        // Act
        var result = provider.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        if (isPinSet)
        {
            result.ShouldNotContain("ACTION7"); // ACTION7 jest tylko dla kart bez PIN
        }
        else
        {
            result.ShouldNotContain("ACTION6"); // ACTION6 jest tylko dla kart z PIN
        }
    }

    [Fact]
    public void Constructor_WhenFileNotFound_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "nonexistent.csv";

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => 
            new CardActionRulesProvider(nonExistentPath, _loggerMock.Object));
    }

    [Theory]
    [InlineData(CardType.Credit, CardStatus.Active)]
    [InlineData(CardType.Debit, CardStatus.Active)]
    [InlineData(CardType.Prepaid, CardStatus.Active)]
    public void GetAllowedActions_ForActiveCards_ShouldIncludeBasicActions(CardType cardType, CardStatus cardStatus)
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);
        var basicActions = new[] { "ACTION3", "ACTION4", "ACTION9" }; // Akcje dostępne dla wszystkich aktywnych kart

        // Act
        var result = provider.GetAllowedActions(cardType, cardStatus, true);

        // Assert
        result.ShouldContain(x => basicActions.Contains(x));
    }

    [Theory]
    [InlineData(CardStatus.Restricted)]
    [InlineData(CardStatus.Blocked)]
    [InlineData(CardStatus.Expired)]
    [InlineData(CardStatus.Closed)]
    public void GetAllowedActions_ForRestrictedStatuses_ShouldLimitActions(CardStatus restrictedStatus)
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);
        var restrictedActions = new[] { "ACTION10", "ACTION11", "ACTION12", "ACTION13" }; // Akcje zazwyczaj niedostępne dla ograniczonych statusów

        // Act
        var result = provider.GetAllowedActions(CardType.Prepaid, restrictedStatus, true);

        // Assert
        result.ShouldNotContain(x => restrictedActions.Contains(x));
    }

    [Fact]
    public void GetAllowedActions_ForCreditCardSpecificActions_ShouldBeAvailableOnlyForCreditCards()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);
        var creditOnlyAction = "ACTION5"; // Akcja dostępna tylko dla kart kredytowych

        // Act & Assert
        var creditCardResult = provider.GetAllowedActions(CardType.Credit, CardStatus.Active, true);
        creditCardResult.ShouldContain(creditOnlyAction);

        var debitCardResult = provider.GetAllowedActions(CardType.Debit, CardStatus.Active, true);
        debitCardResult.ShouldNotContain(creditOnlyAction);

        var prepaidCardResult = provider.GetAllowedActions(CardType.Prepaid, CardStatus.Active, true);
        prepaidCardResult.ShouldNotContain(creditOnlyAction);
    }
} 