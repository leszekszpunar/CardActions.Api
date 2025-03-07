using System.Collections.Generic;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using Shouldly;
using Xunit;

namespace CardActions.Unit.Tests.Policies;

public class CardActionPolicyTests
{
    private readonly IReadOnlyCollection<CardActionRule> _rules;
    private readonly CardActionPolicy _policy;

    public CardActionPolicyTests()
    {
        _rules = new List<CardActionRule>
        {
            // Reguły dla ACTION1
            new CardActionRule("ACTION1", CardType.Prepaid, CardStatus.Active, true),
            new CardActionRule("ACTION1", CardType.Debit, CardStatus.Active, true),
            new CardActionRule("ACTION1", CardType.Credit, CardStatus.Active, true),
            
            // Reguły dla ACTION2
            new CardActionRule("ACTION2", CardType.Prepaid, CardStatus.Active, true),
            new CardActionRule("ACTION2", CardType.Debit, CardStatus.Active, true),
            new CardActionRule("ACTION2", CardType.Credit, CardStatus.Active, true),
            new CardActionRule("ACTION2", CardType.Prepaid, CardStatus.Inactive, true),
            
            // Reguły dla ACTION3 (dostępne dla wszystkich)
            new CardActionRule("ACTION3", CardType.Prepaid, CardStatus.Ordered, true),
            new CardActionRule("ACTION3", CardType.Prepaid, CardStatus.Inactive, true),
            new CardActionRule("ACTION3", CardType.Prepaid, CardStatus.Active, true),
            new CardActionRule("ACTION3", CardType.Prepaid, CardStatus.Restricted, true),
            new CardActionRule("ACTION3", CardType.Prepaid, CardStatus.Blocked, true),
            new CardActionRule("ACTION3", CardType.Prepaid, CardStatus.Expired, true),
            new CardActionRule("ACTION3", CardType.Prepaid, CardStatus.Closed, true),
            
            // Reguły dla ACTION6 (zależne od PIN)
            new CardActionRule("ACTION6", CardType.Prepaid, CardStatus.Ordered, false, false),
            new CardActionRule("ACTION6", CardType.Prepaid, CardStatus.Ordered, true, true),
            new CardActionRule("ACTION6", CardType.Prepaid, CardStatus.Active, true),
            
            // Reguły dla ACTION7 (zależne od PIN)
            new CardActionRule("ACTION7", CardType.Prepaid, CardStatus.Ordered, true, false),
            new CardActionRule("ACTION7", CardType.Prepaid, CardStatus.Ordered, false, true),
            new CardActionRule("ACTION7", CardType.Prepaid, CardStatus.Active, true),
            
            // Reguły dla ACTION5 (tylko dla kart kredytowych)
            new CardActionRule("ACTION5", CardType.Credit, CardStatus.Active, true),
            new CardActionRule("ACTION5", CardType.Credit, CardStatus.Restricted, true),
            new CardActionRule("ACTION5", CardType.Credit, CardStatus.Blocked, true),
            new CardActionRule("ACTION5", CardType.Credit, CardStatus.Expired, true),
            new CardActionRule("ACTION5", CardType.Credit, CardStatus.Closed, true)
        };
        
        _policy = new CardActionPolicy(_rules);
    }

    [Theory]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Active, true, true)]
    [InlineData("ACTION1", CardType.Prepaid, CardStatus.Inactive, true, false)]
    [InlineData("ACTION2", CardType.Prepaid, CardStatus.Inactive, true, true)]
    [InlineData("ACTION3", CardType.Prepaid, CardStatus.Closed, true, true)]
    [InlineData("ACTION5", CardType.Credit, CardStatus.Active, true, true)]
    [InlineData("ACTION5", CardType.Prepaid, CardStatus.Active, true, false)]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, true, true)]
    [InlineData("ACTION6", CardType.Prepaid, CardStatus.Ordered, false, false)]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, false, true)]
    [InlineData("ACTION7", CardType.Prepaid, CardStatus.Ordered, true, false)]
    public void IsActionAllowed_ShouldReturnCorrectResult(
        string actionName, 
        CardType cardType, 
        CardStatus cardStatus, 
        bool isPinSet, 
        bool expectedResult)
    {
        // Act
        var result = _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet);
        
        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void IsActionAllowed_WithEmptyActionName_ShouldReturnFalse()
    {
        // Act
        var result = _policy.IsActionAllowed("", CardType.Prepaid, CardStatus.Active, true);
        
        // Assert
        result.ShouldBe(false);
    }

    [Fact]
    public void IsActionAllowed_WithNonExistentAction_ShouldReturnFalse()
    {
        // Act
        var result = _policy.IsActionAllowed("NON_EXISTENT_ACTION", CardType.Prepaid, CardStatus.Active, true);
        
        // Assert
        result.ShouldBe(false);
    }

    [Fact]
    public void IsActionAllowed_WithConflictingRules_ShouldReturnFalse()
    {
        // Arrange
        var conflictingRules = new List<CardActionRule>
        {
            new CardActionRule("ACTION1", CardType.Prepaid, CardStatus.Active, true),
            new CardActionRule("ACTION1", CardType.Prepaid, CardStatus.Active, false)
        };
        
        var policyWithConflictingRules = new CardActionPolicy(conflictingRules);
        
        // Act
        var result = policyWithConflictingRules.IsActionAllowed("ACTION1", CardType.Prepaid, CardStatus.Active, true);
        
        // Assert
        result.ShouldBe(false);
    }
} 