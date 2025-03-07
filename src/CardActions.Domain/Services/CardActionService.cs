using System;
using System.Collections.Generic;
using System.Linq;
using CardActions.Domain.Models;

namespace CardActions.Domain.Services;

/// <summary>
/// Implementacja serwisu domenowego do zarządzania akcjami karty.
/// </summary>
public class CardActionService : ICardActionService
{
    private readonly ICardActionPolicy _policy;
    private readonly IReadOnlyList<string> _allActionNames;

    public CardActionService(ICardActionPolicy policy, IReadOnlyList<string> allActionNames)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _allActionNames = allActionNames ?? throw new ArgumentNullException(nameof(allActionNames));
    }

    public IReadOnlyList<CardAction> GetAllActions()
    {
        return _allActionNames
            .Select(CardAction.Create)
            .ToList();
    }

    public IReadOnlyList<CardAction> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return _allActionNames
            .Where(actionName => _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet))
            .Select(CardAction.Create)
            .ToList();
    }
} 