using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CardActions.Domain.Policies.Interfaces;
using CardActions.Domain.Services;
using CardActions.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace CardActions.Unit.Tests.Services;

/// <summary>
///     Kompleksowe testy dla wszystkich akcji (ACTION1 do ACTION13) w różnych kombinacjach
///     typów kart, statusów i ustawień PIN.
/// </summary>
public class CardActionRulesTests
{
    private readonly ICardActionPolicy _policy;
    private readonly IReadOnlyCollection<CardActionRule> _rules;
    private readonly ICardActionService _service;
    private readonly ITestOutputHelper _testOutputHelper;

    public CardActionRulesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        // Wczytanie reguł z pliku CSV
        var csvPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv"));

        var logger = new Mock<ILogger<CsvCardActionRulesLoader>>().Object;
        var loader = new CsvCardActionRulesLoader(csvPath, logger);
        _rules = loader.LoadRules();

        _policy = new CardActionPolicy(_rules);

        var cardServiceLogger = new Mock<ILogger<CardActionService>>().Object;
        _service = new CardActionService(cardServiceLogger);
    }

    [Theory]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Active, true)]
    [InlineData("ACTION1", CardType.Debit, CardStatus.Active, true)]
    [InlineData("ACTION1", CardType.Credit, CardStatus.Active, true)]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Inactive, false)]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Blocked, false)]
    public void Action1_ShouldBeAllowedOnlyForActiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var expectedResult = IsActionAllowedInCsv(actionName, cardType, cardStatus, isPinSet);

        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult,
            $"Akcja {actionName} dla karty {cardType} w statusie {cardStatus} z PIN={isPinSet} powinna być {(expectedResult ? "dozwolona" : "niedozwolona")}");
    }

    [Theory]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Active, true)]
    [InlineData("ACTION2", CardType.Debit, CardStatus.Active, true)]
    [InlineData("ACTION2", CardType.Credit, CardStatus.Active, true)]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Inactive, true)]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Blocked, false)]
    public void Action2_ShouldBeAllowedForActiveAndInactiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var expectedResult = IsActionAllowedInCsv(actionName, cardType, cardStatus, isPinSet);

        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult,
            $"Akcja {actionName} dla karty {cardType} w statusie {cardStatus} z PIN={isPinSet} powinna być {(expectedResult ? "dozwolona" : "niedozwolona")}");
    }

    [Theory]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Restricted, true, true)]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Blocked, true, true)]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Expired, true, true)]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Closed, true, true)]
    [InlineData("ACTION3", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION3", CardType.Credit, CardStatus.Active, true, true)]
    public void Action3_ShouldBeAllowedForAllCardTypesAndStatuses(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION4", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION4", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION4", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION4", CardType.Prepaid, CardStatus.Restricted, true, true)]
    [InlineData("ACTION4", CardType.Prepaid, CardStatus.Blocked, true, true)]
    [InlineData("ACTION4", CardType.Prepaid, CardStatus.Expired, true, true)]
    [InlineData("ACTION4", CardType.Prepaid, CardStatus.Closed, true, true)]
    [InlineData("ACTION4", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION4", CardType.Credit, CardStatus.Active, true, true)]
    public void Action4_ShouldBeAllowedForAllCardTypesAndStatuses(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION5", CardType.Credit, CardStatus.Active, true, true)]
    [InlineData("ACTION5", CardType.Credit, CardStatus.Restricted, true, true)]
    [InlineData("ACTION5", CardType.Credit, CardStatus.Blocked, true, true)]
    [InlineData("ACTION5", CardType.Prepaid, CardStatus.Active, true, false)]
    [InlineData("ACTION5", CardType.Debit, CardStatus.Active, true, false)]
    public void Action5_ShouldBeAllowedOnlyForCreditCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, true)]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, false)]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Active, true)]
    [InlineData("ACTION6", CardType.Debit, CardStatus.Active, true)]
    [InlineData("ACTION6", CardType.Credit, CardStatus.Active, true)]
    public void Action6_ShouldBeAllowedOnlyWhenPinIsSet(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var expectedResult = IsActionAllowedInCsv(actionName, cardType, cardStatus, isPinSet);

        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult,
            $"Akcja {actionName} dla karty {cardType} w statusie {cardStatus} z PIN={isPinSet} powinna być {(expectedResult ? "dozwolona" : "niedozwolona")}");
    }

    [Theory]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, false)]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, true)]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Active, true)]
    [InlineData("ACTION7", CardType.Debit, CardStatus.Active, true)]
    [InlineData("ACTION7", CardType.Credit, CardStatus.Active, true)]
    public void Action7_ShouldBeAllowedOnlyWhenPinIsNotSetForOrderedCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var expectedResult = IsActionAllowedInCsv(actionName, cardType, cardStatus, isPinSet);

        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult,
            $"Akcja {actionName} dla karty {cardType} w statusie {cardStatus} z PIN={isPinSet} powinna być {(expectedResult ? "dozwolona" : "niedozwolona")}");
    }

    [Theory]
    [InlineData("ACTION8", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION8", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION8", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION8", CardType.Prepaid, CardStatus.Blocked, true, true)]
    [InlineData("ACTION8", CardType.Prepaid, CardStatus.Restricted, true, false)]
    [InlineData("ACTION8", CardType.Prepaid, CardStatus.Expired, true, false)]
    [InlineData("ACTION8", CardType.Prepaid, CardStatus.Closed, true, false)]
    [InlineData("ACTION8", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION8", CardType.Credit, CardStatus.Active, true, true)]
    public void Action8_ShouldBeAllowedForOrderedInactiveActiveAndBlockedCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION9", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION9", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION9", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION9", CardType.Prepaid, CardStatus.Restricted, true, true)]
    [InlineData("ACTION9", CardType.Prepaid, CardStatus.Blocked, true, true)]
    [InlineData("ACTION9", CardType.Prepaid, CardStatus.Expired, true, true)]
    [InlineData("ACTION9", CardType.Prepaid, CardStatus.Closed, true, true)]
    [InlineData("ACTION9", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION9", CardType.Credit, CardStatus.Active, true, true)]
    public void Action9_ShouldBeAllowedForAllCardTypesAndStatuses(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION10", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION10", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION10", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION10", CardType.Prepaid, CardStatus.Restricted, true, false)]
    [InlineData("ACTION10", CardType.Prepaid, CardStatus.Blocked, true, false)]
    [InlineData("ACTION10", CardType.Prepaid, CardStatus.Expired, true, false)]
    [InlineData("ACTION10", CardType.Prepaid, CardStatus.Closed, true, false)]
    [InlineData("ACTION10", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION10", CardType.Credit, CardStatus.Active, true, true)]
    public void Action10_ShouldBeAllowedForOrderedInactiveAndActiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION11", CardType.Prepaid, CardStatus.Ordered, true, false)]
    [InlineData("ACTION11", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION11", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION11", CardType.Prepaid, CardStatus.Restricted, true, false)]
    [InlineData("ACTION11", CardType.Prepaid, CardStatus.Blocked, true, false)]
    [InlineData("ACTION11", CardType.Prepaid, CardStatus.Expired, true, false)]
    [InlineData("ACTION11", CardType.Prepaid, CardStatus.Closed, true, false)]
    [InlineData("ACTION11", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION11", CardType.Credit, CardStatus.Active, true, true)]
    public void Action11_ShouldBeAllowedForInactiveAndActiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION12", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION12", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION12", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION12", CardType.Prepaid, CardStatus.Restricted, true, false)]
    [InlineData("ACTION12", CardType.Prepaid, CardStatus.Blocked, true, false)]
    [InlineData("ACTION12", CardType.Prepaid, CardStatus.Expired, true, false)]
    [InlineData("ACTION12", CardType.Prepaid, CardStatus.Closed, true, false)]
    [InlineData("ACTION12", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION12", CardType.Credit, CardStatus.Active, true, true)]
    public void Action12_ShouldBeAllowedForOrderedInactiveAndActiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("ACTION13", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION13", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION13", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION13", CardType.Prepaid, CardStatus.Restricted, true, false)]
    [InlineData("ACTION13", CardType.Prepaid, CardStatus.Blocked, true, false)]
    [InlineData("ACTION13", CardType.Prepaid, CardStatus.Expired, true, false)]
    [InlineData("ACTION13", CardType.Prepaid, CardStatus.Closed, true, false)]
    [InlineData("ACTION13", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION13", CardType.Credit, CardStatus.Active, true, true)]
    public void Action13_ShouldBeAllowedForOrderedInactiveAndActiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForActiveDebitCard()
    {
        // Arrange
        var cardType = CardType.Debit;
        var cardStatus = CardStatus.Active;
        var isPinSet = true;

        var expectedActions = _rules
            .Where(r => r.CardType == cardType &&
                        r.CardStatus == cardStatus &&
                        r.IsAllowed &&
                        (!r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet))
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        // Act
        var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        result.Count.ShouldBe(expectedActions.Count);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty {cardType} w statusie {cardStatus} z PIN={isPinSet}:");
        foreach (var action in result) _testOutputHelper.WriteLine($"- {action.Name}");
    }

    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForBlockedCreditCard()
    {
        // Arrange
        var cardType = CardType.Credit;
        var cardStatus = CardStatus.Blocked;
        var isPinSet = true;

        var expectedActions = _rules
            .Where(r => r.CardType == cardType &&
                        r.CardStatus == cardStatus &&
                        r.IsAllowed &&
                        (!r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet))
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        // Act
        var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        result.Count.ShouldBe(expectedActions.Count);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty {cardType} w statusie {cardStatus} z PIN={isPinSet}:");
        foreach (var action in result) _testOutputHelper.WriteLine($"- {action.Name}");
    }

    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForClosedPrepaidCard()
    {
        // Arrange
        var cardType = CardType.Prepaid;
        var cardStatus = CardStatus.Closed;
        var isPinSet = true;

        var expectedActions = _rules
            .Where(r => r.CardType == cardType &&
                        r.CardStatus == cardStatus &&
                        r.IsAllowed &&
                        (!r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet))
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        // Act
        var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        result.Count.ShouldBe(expectedActions.Count);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty {cardType} w statusie {cardStatus} z PIN={isPinSet}:");
        foreach (var action in result) _testOutputHelper.WriteLine($"- {action.Name}");
    }

    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForOrderedPrepaidCardWithoutPin()
    {
        // Arrange
        var expectedActions = new[]
        {
            "ACTION3", "ACTION4", "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION12", "ACTION13"
        };

        // Act
        var result = _service.GetAllowedActions(CardType.Prepaid, CardStatus.Ordered, false);

        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine("Dozwolone akcje dla karty prepaid w statusie Ordered bez ustawionego PIN:");
        foreach (var action in result) _testOutputHelper.WriteLine($"- {action.Name}");
    }

    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForInactiveCreditCardWithPin()
    {
        // Arrange
        var expectedActions = new[]
        {
            "ACTION2", "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION8", "ACTION9",
            "ACTION10", "ACTION11", "ACTION12", "ACTION13"
        };

        // Act
        var result = _service.GetAllowedActions(CardType.Credit, CardStatus.Inactive, true);

        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine("Dozwolone akcje dla karty credit w statusie Inactive z ustawionym PIN:");
        foreach (var action in result) _testOutputHelper.WriteLine($"- {action.Name}");
    }

    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForRestrictedDebitCardWithPin()
    {
        // Arrange
        var expectedActions = new[] { "ACTION3", "ACTION4", "ACTION9" };

        // Act
        var result = _service.GetAllowedActions(CardType.Debit, CardStatus.Restricted, true);

        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine("Dozwolone akcje dla karty debit w statusie Restricted z ustawionym PIN:");
        foreach (var action in result) _testOutputHelper.WriteLine($"- {action.Name}");
    }

    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForExpiredCreditCardWithPin()
    {
        // Arrange
        var expectedActions = new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION9" };

        // Act
        var result = _service.GetAllowedActions(CardType.Credit, CardStatus.Expired, true);

        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine("Dozwolone akcje dla karty credit w statusie Expired z ustawionym PIN:");
        foreach (var action in result) _testOutputHelper.WriteLine($"- {action.Name}");
    }

    /// <summary>
    ///     Sprawdza, czy akcja jest dozwolona na podstawie reguł wczytanych z pliku CSV.
    /// </summary>
    private bool IsActionAllowedInCsv(string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        var matchingRules = _rules
            .Where(r => r.ActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase) &&
                        r.CardType == cardType &&
                        r.CardStatus == cardStatus)
            .ToList();

        if (!matchingRules.Any())
            return false;

        var pinMatchingRules = matchingRules
            .Where(r => !r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet)
            .ToList();

        if (!pinMatchingRules.Any())
            return false;

        if (pinMatchingRules.Any(r => !r.IsAllowed))
            return false;

        return true;
    }
}