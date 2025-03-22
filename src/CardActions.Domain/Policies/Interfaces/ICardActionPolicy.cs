using CardActions.Domain.Models;
using CardActions.Domain.Enums;

namespace CardActions.Domain.Policies.Interfaces;

/// <summary>
///     Definiuje politykę określającą, które akcje są dozwolone dla danej karty.
///     Interfejs ten jest częścią warstwy domenowej i definiuje kontrakt dla polityki,
///     która egzekwuje reguły biznesowe dotyczące dozwolonych akcji dla kart.
/// </summary>
public interface ICardActionPolicy
{
    /// <summary>
    ///     Sprawdza, czy dana akcja jest dozwolona dla karty o określonych parametrach.
    /// </summary>
    /// <param name="actionName">Nazwa akcji do sprawdzenia</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>True, jeśli akcja jest dozwolona; w przeciwnym razie false</returns>
    bool IsActionAllowed(string actionName, Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet);
}