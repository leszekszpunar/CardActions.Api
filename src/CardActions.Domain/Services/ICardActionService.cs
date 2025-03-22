using CardActions.Domain.Enums;
using CardActions.Domain.Models;

namespace CardActions.Domain.Services;

/// <summary>
///     Interfejs dla serwisu akcji kart
/// </summary>
public interface ICardActionService
{
    /// <summary>
    ///     Pobiera listę wszystkich dostępnych akcji dla karty
    /// </summary>
    /// <returns>Lista wszystkich akcji</returns>
    IReadOnlyList<CardAction> GetAllActions();
    
    /// <summary>
    ///     Pobiera listę dozwolonych akcji dla karty o podanych parametrach (asynchroniczne)
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    Task<List<CardAction>> GetAllowedActionsAsync(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet);
    
    /// <summary>
    ///     Pobiera listę dozwolonych akcji dla karty o podanych parametrach (synchroniczne)
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    List<CardAction> GetAllowedActions(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet);
    
    /// <summary>
    ///     Wykonuje akcję o podanej nazwie dla karty o podanych parametrach
    /// </summary>
    /// <param name="actionName">Nazwa akcji</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Rezultat wykonania akcji</returns>
    Task<CardActionResult> ExecuteActionAsync(string actionName, Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet);
} 