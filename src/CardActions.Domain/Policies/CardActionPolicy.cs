using System;
using System.Collections.Generic;
using System.Linq;
using CardActions.Domain.Models;
using CardActions.Domain.Services;

namespace CardActions.Domain.Policies;

/// <summary>
/// Implementacja polityki określającej, które akcje są dozwolone dla danej karty.
/// Klasa ta implementuje wzorzec Policy z DDD i odpowiada za egzekwowanie reguł biznesowych
/// dotyczących dozwolonych akcji dla kart o określonych parametrach.
/// </summary>
public class CardActionPolicy : ICardActionPolicy
{
    /// <summary>
    /// Kolekcja reguł używanych do określenia, które akcje są dozwolone.
    /// </summary>
    private readonly IReadOnlyCollection<CardActionRule> _rules;

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="CardActionPolicy"/>.
    /// </summary>
    /// <param name="rules">Kolekcja reguł używanych do określenia, które akcje są dozwolone</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy rules jest null</exception>
    public CardActionPolicy(IReadOnlyCollection<CardActionRule> rules)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    /// Sprawdza, czy dana akcja jest dozwolona dla karty o określonych parametrach.
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

/// <summary>
/// Reprezentuje regułę określającą, czy dana akcja jest dozwolona dla określonego typu i statusu karty.
/// Implementuje wzorzec Value Object z DDD.
/// </summary>
public class CardActionRule
{
    /// <summary>
    /// Nazwa akcji, której dotyczy reguła.
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    /// Typ karty, dla którego obowiązuje reguła.
    /// </summary>
    public CardType CardType { get; }

    /// <summary>
    /// Status karty, dla którego obowiązuje reguła.
    /// </summary>
    public CardStatus CardStatus { get; }

    /// <summary>
    /// Określa, czy akcja jest dozwolona dla podanego typu i statusu karty.
    /// </summary>
    public bool IsAllowed { get; }

    /// <summary>
    /// Określa, czy akcja wymaga ustawionego PIN-u.
    /// Null oznacza, że akcja nie zależy od ustawienia PIN-u.
    /// True oznacza, że akcja wymaga ustawionego PIN-u.
    /// False oznacza, że akcja wymaga nieustawionego PIN-u.
    /// </summary>
    public bool? RequiresPinSet { get; }

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="CardActionRule"/>.
    /// </summary>
    /// <param name="actionName">Nazwa akcji</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isAllowed">Czy akcja jest dozwolona</param>
    /// <param name="requiresPinSet">Czy akcja wymaga ustawionego PIN-u</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy actionName jest null</exception>
    public CardActionRule(string actionName, CardType cardType, CardStatus cardStatus, bool isAllowed, bool? requiresPinSet = null)
    {
        ActionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
        CardType = cardType;
        CardStatus = cardStatus;
        IsAllowed = isAllowed;
        RequiresPinSet = requiresPinSet;
    }
} 