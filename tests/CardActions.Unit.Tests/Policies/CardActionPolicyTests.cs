using CardActions.Domain.Models;
using CardActions.Domain.Policies;

namespace CardActions.Unit.Tests.Policies;

/// <summary>
///     Testy jednostkowe dla polityki CardActionPolicy, która decyduje o tym,
///     czy dana akcja jest dozwolona dla określonego typu i statusu karty z uwzględnieniem
///     stanu PIN-u.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Policies")]
public class CardActionPolicyTests
{
    private readonly CardActionPolicy _policy;
    private readonly IReadOnlyCollection<CardActionRule> _rules;

    public CardActionPolicyTests()
    {
        _rules = new List<CardActionRule>
        {
            // Reguły dla ACTION1 - dozwolone tylko dla aktywnych kart
            new("ACTION1", CardType.Prepaid, CardStatus.Active, true),
            new("ACTION1", CardType.Debit, CardStatus.Active, true),
            new("ACTION1", CardType.Credit, CardStatus.Active, true),

            // Reguły dla ACTION2 - dozwolone dla aktywnych wszystkich typów kart oraz dla nieaktywnych kart przedpłaconych
            new("ACTION2", CardType.Prepaid, CardStatus.Active, true),
            new("ACTION2", CardType.Debit, CardStatus.Active, true),
            new("ACTION2", CardType.Credit, CardStatus.Active, true),
            new("ACTION2", CardType.Prepaid, CardStatus.Inactive, true),

            // Reguły dla ACTION3 (dostępne dla wszystkich statusów karty przedpłaconej)
            new("ACTION3", CardType.Prepaid, CardStatus.Ordered, true),
            new("ACTION3", CardType.Prepaid, CardStatus.Inactive, true),
            new("ACTION3", CardType.Prepaid, CardStatus.Active, true),
            new("ACTION3", CardType.Prepaid, CardStatus.Restricted, true),
            new("ACTION3", CardType.Prepaid, CardStatus.Blocked, true),
            new("ACTION3", CardType.Prepaid, CardStatus.Expired, true),
            new("ACTION3", CardType.Prepaid, CardStatus.Closed, true),

            // Reguły dla ACTION6 (zależne od PIN) - dozwolone dla zamówionych kart tylko gdy PIN jest ustawiony
            new("ACTION6", CardType.Prepaid, CardStatus.Ordered, false, false),
            new("ACTION6", CardType.Prepaid, CardStatus.Ordered, true, true),
            new("ACTION6", CardType.Prepaid, CardStatus.Active, true),

            // Reguły dla ACTION7 (zależne od PIN w odwrotny sposób) - dozwolone dla zamówionych kart tylko gdy PIN NIE jest ustawiony
            new("ACTION7", CardType.Prepaid, CardStatus.Ordered, true, false),
            new("ACTION7", CardType.Prepaid, CardStatus.Ordered, false, true),
            new("ACTION7", CardType.Prepaid, CardStatus.Active, true),

            // Reguły dla ACTION5 (tylko dla kart kredytowych) - dostępne tylko dla kart kredytowych
            new("ACTION5", CardType.Credit, CardStatus.Active, true),
            new("ACTION5", CardType.Credit, CardStatus.Restricted, true),
            new("ACTION5", CardType.Credit, CardStatus.Blocked, true),
            new("ACTION5", CardType.Credit, CardStatus.Expired, true),
            new("ACTION5", CardType.Credit, CardStatus.Closed, true)
        };

        _policy = new CardActionPolicy(_rules);
    }

    [Theory(DisplayName =
        "IsActionAllowed powinno zwracać prawidłowy wynik na podstawie typu, statusu karty i stanu PIN")]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Active, true, true,
        "ACTION1 dozwolone dla aktywnej karty przedpłaconej")]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Inactive, true, false,
        "ACTION1 niedozwolone dla nieaktywnej karty przedpłaconej")]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Inactive, true, true,
        "ACTION2 dozwolone dla nieaktywnej karty przedpłaconej")]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Closed, true, true,
        "ACTION3 dozwolone dla zamkniętej karty przedpłaconej")]
    [InlineData("ACTION5", CardType.Credit, CardStatus.Active, true, true,
        "ACTION5 dozwolone dla aktywnej karty kredytowej")]
    [InlineData("ACTION5", CardType.Prepaid, CardStatus.Active, true, false,
        "ACTION5 niedozwolone dla karty przedpłaconej")]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, true, true,
        "ACTION6 dozwolone dla zamówionej karty z ustawionym PIN-em")]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, false, false,
        "ACTION6 niedozwolone dla zamówionej karty bez PIN-u")]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, false, true,
        "ACTION7 dozwolone dla zamówionej karty z nieustawionym PIN-em")]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, true, false,
        "ACTION7 niedozwolone dla zamówionej karty z ustawionym PIN-em")]
    public void IsActionAllowed_ShouldReturnCorrectResult(
        string actionName,
        CardType cardType,
        CardStatus cardStatus,
        bool isPinSet,
        bool expectedResult,
        string testCase)
    {
        // Arrange - setup done in constructor

        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);

        // Assert
        result.ShouldBe(expectedResult, $"Niepoprawny wynik dla testu: {testCase}");
    }

    [Fact(DisplayName = "IsActionAllowed powinno zwrócić false dla pustej nazwy akcji")]
    public void IsActionAllowed_WithEmptyActionName_ShouldReturnFalse()
    {
        // Arrange - setup done in constructor

        // Act
        var result = _policy.IsActionAllowed("", CardType.Prepaid, CardStatus.Active, true);

        // Assert
        result.ShouldBe(false, "Pusta nazwa akcji powinna zawsze zwracać false");
    }

    [Fact(DisplayName = "IsActionAllowed powinno zwrócić false dla nieistniejącej akcji")]
    public void IsActionAllowed_WithNonExistentAction_ShouldReturnFalse()
    {
        // Arrange - setup done in constructor

        // Act
        var result = _policy.IsActionAllowed("NON_EXISTENT_ACTION", CardType.Prepaid, CardStatus.Active, true);

        // Assert
        result.ShouldBe(false, "Nieistniejąca akcja powinna zawsze zwracać false");
    }

    [Fact(DisplayName = "IsActionAllowed powinno zwrócić false przy sprzecznych regułach")]
    public void IsActionAllowed_WithConflictingRules_ShouldReturnFalse()
    {
        // Arrange
        var conflictingRules = new List<CardActionRule>
        {
            new("ACTION1", CardType.Prepaid, CardStatus.Active, true),
            new("ACTION1", CardType.Prepaid, CardStatus.Active, false)
        };

        var policyWithConflictingRules = new CardActionPolicy(conflictingRules);

        // Act
        var result = policyWithConflictingRules.IsActionAllowed("ACTION1", CardType.Prepaid, CardStatus.Active, true);

        // Assert
        result.ShouldBe(false, "Sprzeczne reguły dla tej samej akcji, typu i statusu karty powinny zwracać false");
    }

    [Fact(DisplayName = "IsActionAllowed powinno uwzględniać wymagania dotyczące PIN-u")]
    public void IsActionAllowed_WithPinDependencies_ShouldRespectPinRequirements()
    {
        // Arrange - setup done in constructor

        // Act & Assert
        // ACTION6 wymaga ustawionego PIN-u dla zamówionych kart
        _policy.IsActionAllowed("ACTION6", CardType.Prepaid, CardStatus.Ordered, true)
            .ShouldBeTrue("ACTION6 powinno być dozwolone dla zamówionej karty z PIN-em");

        _policy.IsActionAllowed("ACTION6", CardType.Prepaid, CardStatus.Ordered, false)
            .ShouldBeFalse("ACTION6 powinno być niedozwolone dla zamówionej karty bez PIN-u");

        // ACTION7 wymaga NIE ustawionego PIN-u dla zamówionych kart
        _policy.IsActionAllowed("ACTION7", CardType.Prepaid, CardStatus.Ordered, false)
            .ShouldBeTrue("ACTION7 powinno być dozwolone dla zamówionej karty bez PIN-u");

        _policy.IsActionAllowed("ACTION7", CardType.Prepaid, CardStatus.Ordered, true)
            .ShouldBeFalse("ACTION7 powinno być niedozwolone dla zamówionej karty z PIN-em");
    }
}