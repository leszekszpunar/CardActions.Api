using CardActions.Domain.Models;
using CardActions.Domain.Policies.Interfaces;

namespace CardActions.Domain.Policies;

/// <summary>
///     Implementacja polityki określającej, które akcje są dozwolone dla danej karty.
///     Klasa ta implementuje wzorzec Policy z DDD i odpowiada za egzekwowanie reguł biznesowych
///     dotyczących dozwolonych akcji dla kart o określonych parametrach.
///     Wzorce projektowe:
///     - Policy Pattern: Ta klasa jest przykładem wzorca Policy, gdzie reguły biznesowe są
///     enkapsulowane w osobnej klasie. Dzięki temu reguły można łatwo modyfikować i testować
///     niezależnie od reszty kodu.
///     - Strategy Pattern: Polityka działa jako strategia określania dozwolonych akcji,
///     pozwalając na łatwą podmianę w przyszłości (np. inna implementacja ICardActionPolicy).
///     - Dependency Injection: Zależność od reguł jest wstrzykiwana przez konstruktor,
///     co ułatwia testowanie i zwiększa elastyczność.
///     Zalety:
///     - Odseparowanie reguł biznesowych od innych warstw aplikacji
///     - Jednolite miejsce do definiowania i egzekwowania polityk
///     - Możliwość łatwej zmiany polityki w przyszłości
/// </summary>
public class CardActionPolicy : ICardActionPolicy
{
    /// <summary>
    ///     Kolekcja reguł używanych do określenia, które akcje są dozwolone.
    /// </summary>
    private readonly IReadOnlyCollection<CardActionRule> _rules;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardActionPolicy" />.
    /// </summary>
    /// <param name="rules">Kolekcja reguł używanych do określenia, które akcje są dozwolone</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy rules jest null</exception>
    public CardActionPolicy(IReadOnlyCollection<CardActionRule> rules)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    ///     Sprawdza, czy dana akcja jest dozwolona dla karty o określonych parametrach.
    /// </summary>
    /// <param name="actionName">Nazwa akcji do sprawdzenia</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>True, jeśli akcja jest dozwolona; w przeciwnym razie false</returns>
    public bool IsActionAllowed(string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Sprawdź, czy nazwa akcji jest poprawna
        if (string.IsNullOrWhiteSpace(actionName))
            return false;

        // Znajdź reguły pasujące do podanych parametrów
        var matchingRules = _rules
            .Where(r => r.ActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase) &&
                        r.CardType == cardType &&
                        r.CardStatus == cardStatus)
            .ToList();

        // Jeśli nie ma pasujących reguł, akcja jest niedozwolona
        if (!matchingRules.Any())
            return false;

        // Filtruj reguły, które pasują do isPinSet
        var pinMatchingRules = matchingRules
            .Where(r => !r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet)
            .ToList();

        // Jeśli nie ma reguł pasujących do isPinSet, akcja jest niedozwolona
        if (!pinMatchingRules.Any())
            return false;

        // Jeśli którakolwiek z pasujących reguł ma IsAllowed=false, akcja jest niedozwolona
        if (pinMatchingRules.Any(r => !r.IsAllowed))
            return false;

        // Wszystkie warunki są spełnione, akcja jest dozwolona
        return true;
    }
}