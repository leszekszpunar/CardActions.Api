using System;
using System.Collections.Generic;
using System.Linq;
using CardActions.Domain.Models;

namespace CardActions.Domain.Services;

/// <summary>
/// Implementacja serwisu domenowego do zarządzania akcjami karty.
/// Klasa ta implementuje wzorzec Domain Service z DDD i odpowiada za dostarczanie
/// funkcjonalności związanych z akcjami karty, które nie pasują do żadnej konkretnej encji.
/// </summary>
public class CardActionService : ICardActionService
{
    /// <summary>
    /// Polityka określająca, które akcje są dozwolone dla danej karty.
    /// </summary>
    private readonly ICardActionPolicy _policy;

    /// <summary>
    /// Lista wszystkich dostępnych nazw akcji.
    /// </summary>
    private readonly IReadOnlyList<string> _allActionNames;

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="CardActionService"/>.
    /// </summary>
    /// <param name="policy">Polityka określająca, które akcje są dozwolone</param>
    /// <param name="allActionNames">Lista wszystkich dostępnych nazw akcji</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy policy lub allActionNames jest null</exception>
    public CardActionService(ICardActionPolicy policy, IReadOnlyList<string> allActionNames)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _allActionNames = allActionNames ?? throw new ArgumentNullException(nameof(allActionNames));
    }

    /// <summary>
    /// Pobiera listę wszystkich dostępnych akcji dla karty.
    /// </summary>
    /// <returns>Lista wszystkich możliwych akcji</returns>
    public IReadOnlyList<CardAction> GetAllActions()
    {
        return _allActionNames
            .Select(CardAction.Create)
            .ToList();
    }

    /// <summary>
    /// Pobiera listę dozwolonych akcji dla karty o określonych parametrach.
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    public IReadOnlyList<CardAction> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return _allActionNames
            .Where(actionName => _policy.IsActionAllowed(actionName, cardType, cardStatus, isPinSet))
            .Select(CardAction.Create)
            .ToList();
    }
} 