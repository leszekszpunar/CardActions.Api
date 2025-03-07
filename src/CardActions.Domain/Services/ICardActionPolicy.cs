using CardActions.Domain.Models;

namespace CardActions.Domain.Services;

/// <summary>
/// Definiuje politykę określającą, które akcje są dozwolone dla danej karty.
/// </summary>
public interface ICardActionPolicy
{
    /// <summary>
    /// Sprawdza, czy dana akcja jest dozwolona dla karty o określonych parametrach.
    /// </summary>
    /// <param name="actionName">Nazwa akcji do sprawdzenia</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>True, jeśli akcja jest dozwolona; w przeciwnym razie false</returns>
    bool IsActionAllowed(string actionName, CardType cardType, CardStatus cardStatus, bool isPinSet);
} 