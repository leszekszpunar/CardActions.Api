using CardActions.Domain.Enums;
using CardActions.Domain.Models;

namespace CardActions.Domain.Services.Interfaces;

/// <summary>
///     Definiuje serwis domenowy do zarządzania akcjami karty.
///     Interfejs ten jest częścią warstwy domenowej i definiuje kontrakt dla serwisu,
///     który dostarcza funkcjonalności związane z akcjami karty.
/// </summary>
public interface ICardActionService
{
    /// <summary>
    ///     Pobiera listę wszystkich dostępnych akcji dla karty.
    /// </summary>
    /// <returns>Lista wszystkich możliwych akcji</returns>
    IReadOnlyList<CardAction> GetAllActions();

    /// <summary>
    ///     Pobiera listę dozwolonych akcji dla karty o określonych parametrach.
    ///     Metoda synchroniczna (dla kompatybilności wstecznej).
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    List<CardAction> GetAllowedActions(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet);
    
    /// <summary>
    ///     Pobiera listę dozwolonych akcji dla karty o określonych parametrach.
    ///     Metoda asynchroniczna.
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    Task<List<CardAction>> GetAllowedActionsAsync(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet);
}