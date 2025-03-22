using CardActions.Domain.Enums;

namespace CardActions.Domain.Strategies.Base;

/// <summary>
/// Interfejs strategii określającej dostępność akcji karty
/// </summary>
public interface ICardActionAvailabilityStrategy
{
    /// <summary>
    /// Sprawdza czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Czy akcja jest dostępna</returns>
    bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet);
} 