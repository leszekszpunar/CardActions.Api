using System;
using System.Collections.Generic;
using System.Linq;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CardActions.Domain.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace CardActions.Unit.Tests.Services;

/// <summary>
/// Testy integracyjne dla CardActionService z użyciem testowej implementacji ICardActionRulesProvider.
/// </summary>
public class CardActionIntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TestCardActionRulesProvider _rulesProvider;
    private readonly CardActionPolicy _policy;
    private readonly CardActionService _service;
    
    public CardActionIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _rulesProvider = new TestCardActionRulesProvider();
        _policy = new CardActionPolicy(_rulesProvider.GetAllRules());
        _service = new CardActionService(_policy, _rulesProvider.GetAllActionNames());
    }
    
    [Fact]
    public void GetAllowedActions_ForActiveDebitCard_ShouldReturnCorrectActions()
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
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty debetowej w statusie Active z ustawionym PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
    }
    
    [Fact]
    public void GetAllowedActions_ForBlockedCreditCard_ShouldReturnCorrectActions()
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
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty kredytowej w statusie Blocked z ustawionym PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
    }
    
    [Fact]
    public void GetAllowedActions_ForOrderedPrepaidCardWithoutPin_ShouldReturnCorrectActions()
    {
        // Act
        var result = _service.GetAllowedActions(CardType.Prepaid, CardStatus.Ordered, false);
        
        // Assert
        result.ShouldContain(a => a.Name == "ACTION7"); // ACTION7 jest dozwolone dla kart bez PIN
        result.ShouldNotContain(a => a.Name == "ACTION6"); // ACTION6 jest niedozwolone dla kart bez PIN
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty prepaid w statusie Ordered bez ustawionego PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
    }
    
    [Fact]
    public void GetAllowedActions_ForOrderedPrepaidCardWithPin_ShouldReturnCorrectActions()
    {
        // Act
        var result = _service.GetAllowedActions(CardType.Prepaid, CardStatus.Ordered, true);
        
        // Assert
        result.ShouldContain(a => a.Name == "ACTION6"); // ACTION6 jest dozwolone dla kart z PIN
        result.ShouldNotContain(a => a.Name == "ACTION7"); // ACTION7 jest niedozwolone dla kart z PIN
        
        // Wyświetlenie wszystkich dozwolonych akcji dla lepszej przejrzystości
        _testOutputHelper.WriteLine($"Dozwolone akcje dla karty prepaid w statusie Ordered z ustawionym PIN:");
        foreach (var action in result)
        {
            _testOutputHelper.WriteLine($"- {action.Name}");
        }
    }
    
    [Fact]
    public void GetAllowedActions_ForAllCardTypesAndStatuses_ShouldAlwaysIncludeBasicActions()
    {
        // Arrange
        var basicActions = new[] { "ACTION3", "ACTION4", "ACTION9" };
        
        // Act & Assert
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            foreach (var cardStatus in Enum.GetValues<CardStatus>())
            {
                foreach (var isPinSet in new[] { true, false })
                {
                    var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);
                    var resultNames = result.Select(a => a.Name).ToList();
                    
                    foreach (var basicAction in basicActions)
                    {
                        resultNames.ShouldContain(basicAction, 
                            $"Akcja {basicAction} powinna być dozwolona dla karty {cardType} w statusie {cardStatus} z isPinSet={isPinSet}");
                    }
                }
            }
        }
    }
} 