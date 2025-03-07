using System;
using System.Collections.Generic;
using System.Linq;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CardActions.Domain.Services;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace CardActions.Unit.Tests.Services;

/// <summary>
/// Kompleksowe testy dla wszystkich akcji (ACTION1 do ACTION13) w różnych kombinacjach
/// typów kart, statusów i ustawień PIN.
/// </summary>
public class CardActionRulesTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ICardActionPolicy _policy;
    private readonly ICardActionService _service;
    
    public CardActionRulesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        // Przygotowanie reguł testowych dla wszystkich akcji
        var rules = PrepareTestRules();
        _policy = new CardActionPolicy(rules);
        
        var allActionNames = new List<string>
        {
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION5", 
            "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10", 
            "ACTION11", "ACTION12", "ACTION13"
        };
        
        _service = new CardActionService(_policy, allActionNames);
    }
    
    [Theory]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION1", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION1", CardType.Credit, CardStatus.Active, true, true)]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Inactive, true, false)]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Blocked, true, false)]
    public void Action1_ShouldBeAllowedOnlyForActiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);
        
        // Assert
        result.ShouldBe(expectedResult);
    }
    
    [Theory]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION2", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION2", CardType.Credit, CardStatus.Active, true, true)]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Blocked, true, false)]
    public void Action2_ShouldBeAllowedForActiveAndInactiveCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);
        
        // Assert
        result.ShouldBe(expectedResult);
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
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, false, false)]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION6", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION6", CardType.Credit, CardStatus.Active, true, true)]
    public void Action6_ShouldBeAllowedOnlyWhenPinIsSet(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);
        
        // Assert
        result.ShouldBe(expectedResult);
    }
    
    [Theory]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, false, true)]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, true, false)]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION7", CardType.Debit, CardStatus.Active, true, true)]
    [InlineData("ACTION7", CardType.Credit, CardStatus.Active, true, true)]
    public void Action7_ShouldBeAllowedOnlyWhenPinIsNotSetForOrderedCards(
        string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet, bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);
        
        // Assert
        result.ShouldBe(expectedResult);
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
        var expectedActions = new[] 
        { 
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7", 
            "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12", "ACTION13" 
        };
        
        // Act
        var result = _service.GetAllowedActions(CardType.Debit, CardStatus.Active, true);
        
        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction);
        }
    }
    
    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForBlockedCreditCard()
    {
        // Arrange
        var expectedActions = new[] 
        { 
            "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" 
        };
        
        // Act
        var result = _service.GetAllowedActions(CardType.Credit, CardStatus.Blocked, true);
        
        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction);
        }
    }
    
    [Fact]
    public void GetAllowedActions_ShouldReturnCorrectActionsForClosedPrepaidCard()
    {
        // Arrange
        var expectedActions = new[] { "ACTION3", "ACTION4", "ACTION9" };
        
        // Act
        var result = _service.GetAllowedActions(CardType.Prepaid, CardStatus.Closed, true);
        
        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction);
        }
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty prepaid w statusie Closed z ustawionym PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
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
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction);
        }
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty prepaid w statusie Ordered bez ustawionego PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
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
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction);
        }
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty credit w statusie Inactive z ustawionym PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
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
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction);
        }
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty debit w statusie Restricted z ustawionym PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
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
        foreach (var expectedAction in expectedActions)
        {
            resultNames.ShouldContain(expectedAction);
        }
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty credit w statusie Expired z ustawionym PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
    }
    
    /// <summary>
    /// Przygotowuje zestaw testowych reguł dla wszystkich akcji.
    /// </summary>
    private IReadOnlyCollection<CardActionRule> PrepareTestRules()
    {
        var rules = new List<CardActionRule>();
        
        // ACTION1 - tylko dla aktywnych kart
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION1", cardType, CardStatus.Active, true));
        }
        
        // ACTION2 - dla aktywnych i nieaktywnych kart
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION2", cardType, CardStatus.Active, true));
            rules.Add(new CardActionRule("ACTION2", cardType, CardStatus.Inactive, true));
        }
        
        // ACTION3 - dla wszystkich typów kart i statusów
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            foreach (var cardStatus in Enum.GetValues<CardStatus>())
            {
                rules.Add(new CardActionRule("ACTION3", cardType, cardStatus, true));
            }
        }
        
        // ACTION4 - dla wszystkich typów kart i statusów
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            foreach (var cardStatus in Enum.GetValues<CardStatus>())
            {
                rules.Add(new CardActionRule("ACTION4", cardType, cardStatus, true));
            }
        }
        
        // ACTION5 - tylko dla kart kredytowych
        foreach (var cardStatus in Enum.GetValues<CardStatus>())
        {
            rules.Add(new CardActionRule("ACTION5", CardType.Credit, cardStatus, true));
        }
        
        // ACTION6 - zależne od PIN
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            // Dla statusów Ordered i Inactive - tylko gdy PIN jest ustawiony
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Ordered, true, true));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Inactive, true, true));
            
            // Dla statusu Active - zawsze dozwolone
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Active, true));
            
            // Dla statusu Blocked - tylko gdy PIN jest ustawiony
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Blocked, true, true));
            
            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Closed, false));
        }
        
        // ACTION7 - zależne od PIN
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            // Dla statusów Ordered i Inactive - tylko gdy PIN NIE jest ustawiony
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Ordered, true, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Inactive, true, false));
            
            // Dla statusu Active - zawsze dozwolone
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Active, true));
            
            // Dla statusu Blocked - tylko gdy PIN jest ustawiony
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Blocked, true, true));
            
            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Closed, false));
        }
        
        // ACTION8 - dla Ordered, Inactive, Active i Blocked
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Active, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Blocked, true));
            
            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Closed, false));
        }
        
        // ACTION9 - dla wszystkich typów kart i statusów
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            foreach (var cardStatus in Enum.GetValues<CardStatus>())
            {
                rules.Add(new CardActionRule("ACTION9", cardType, cardStatus, true));
            }
        }
        
        // ACTION10 - dla Ordered, Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Active, true));
            
            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Closed, false));
        }
        
        // ACTION11 - dla Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Active, true));
            
            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Ordered, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Closed, false));
        }
        
        // ACTION12 - dla Ordered, Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Active, true));
            
            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Closed, false));
        }
        
        // ACTION13 - dla Ordered, Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Active, true));
            
            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Closed, false));
        }
        
        return rules;
    }
} 