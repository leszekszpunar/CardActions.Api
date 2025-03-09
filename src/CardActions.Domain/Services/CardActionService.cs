using System;
using System.Collections.Generic;
using System.Linq;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;

namespace CardActions.Domain.Services;

/// <summary>
/// Serwis domenowy dostarczający funkcjonalności związane z akcjami karty.
/// Klasa ta odpowiada za pobieranie dozwolonych akcji dla karty o określonych parametrach.
/// 
/// Wzorce projektowe:
/// - Service Layer: Ta klasa implementuje wzorzec Service Layer, działając jako
///   fasada dla operacji domenowych związanych z akcjami karty.
/// - Strategy Pattern: Serwis korzysta z polityki jako strategii do określania,
///   które akcje są dozwolone, co umożliwia łatwą zmianę zachowania.
/// - Dependency Injection: Zależności od polityki i listy wszystkich akcji są wstrzykiwane
///   przez konstruktor, co zwiększa elastyczność i ułatwia testowanie.
/// 
/// SOLID:
/// - Single Responsibility Principle: Klasa ma jedno zadanie - dostarczanie akcji karty
/// - Open/Closed Principle: Można rozszerzyć zachowanie serwisu bez modyfikacji jego kodu
///   poprzez dostarczenie innej implementacji ICardActionPolicy
/// - Dependency Inversion: Serwis zależy od abstrakcji (interfejsów), a nie od konkretnych implementacji
/// 
/// Zalety:
/// - Prosty interfejs dla klientów
/// - Możliwość wymiany polityki bez zmiany serwisu
/// - Łatwość testowania dzięki wstrzykiwaniu zależności
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