using System.Collections.Generic;
using CardActions.Domain.Models;

namespace CardActions.Domain.Services;

/// <summary>
/// Definiuje serwis domenowy do zarządzania akcjami karty.
/// Interfejs ten jest częścią warstwy domenowej i definiuje kontrakt dla serwisu,
/// który dostarcza funkcjonalności związane z akcjami karty.
/// </summary>
public interface ICardActionService
{
    /// <summary>
    /// Pobiera listę wszystkich dostępnych akcji dla karty.
    /// </summary>
    /// <returns>Lista wszystkich możliwych akcji</returns>
    IReadOnlyList<CardAction> GetAllActions();

    /// <summary>
    /// Pobiera listę dozwolonych akcji dla karty o określonych parametrach.
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    IReadOnlyList<CardAction> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet);
} 