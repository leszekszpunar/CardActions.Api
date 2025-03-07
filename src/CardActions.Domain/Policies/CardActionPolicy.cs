using System;
using System.Collections.Generic;
using System.Linq;
using CardActions.Domain.Models;
using CardActions.Domain.Services;

namespace CardActions.Domain.Policies;

/// <summary>
/// Implementacja polityki określającej, które akcje są dozwolone dla danej karty.
/// </summary>
public class CardActionPolicy : ICardActionPolicy
{
    private readonly IReadOnlyCollection<CardActionRule> _rules;

    public CardActionPolicy(IReadOnlyCollection<CardActionRule> rules)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    public bool IsActionAllowed(string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return false;

        var matchingRules = _rules
            .Where(r => r.ActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase) &&
                   r.CardType == cardType &&
                   r.CardStatus == cardStatus)
            .ToList();

        if (!matchingRules.Any())
            return false;

        // Filtruj reguły, które pasują do isPinSet
        var pinMatchingRules = matchingRules
            .Where(r => !r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet)
            .ToList();

        // Jeśli nie ma reguł pasujących do isPinSet, zwróć false
        if (!pinMatchingRules.Any())
            return false;

        // Jeśli którakolwiek z pasujących reguł ma IsAllowed=false, zwróć false
        if (pinMatchingRules.Any(r => !r.IsAllowed))
            return false;

        return true;
    }
}

/// <summary>
/// Reprezentuje regułę określającą, czy dana akcja jest dozwolona dla określonego typu i statusu karty.
/// </summary>
public class CardActionRule
{
    public string ActionName { get; }
    public CardType CardType { get; }
    public CardStatus CardStatus { get; }
    public bool IsAllowed { get; }
    public bool? RequiresPinSet { get; }

    public CardActionRule(string actionName, CardType cardType, CardStatus cardStatus, bool isAllowed, bool? requiresPinSet = null)
    {
        ActionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
        CardType = cardType;
        CardStatus = cardStatus;
        IsAllowed = isAllowed;
        RequiresPinSet = requiresPinSet;
    }
} 