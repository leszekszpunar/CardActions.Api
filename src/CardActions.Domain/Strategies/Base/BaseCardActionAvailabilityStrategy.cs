using CardActions.Domain.Enums;

namespace CardActions.Domain.Strategies.Base;

/// <summary>
/// Bazowa abstrakcyjna klasa dla strategii dostępności akcji karty
/// </summary>
public abstract class BaseCardActionAvailabilityStrategy : ICardActionAvailabilityStrategy
{
    /// <summary>
    /// Sprawdza czy karta jest jednego z dozwolonych typów
    /// </summary>
    /// <param name="cardType">Typ karty do sprawdzenia</param>
    /// <param name="allowedCardTypes">Dozwolone typy kart</param>
    /// <returns>Czy karta jest dozwolonego typu</returns>
    protected bool IsAllowedCardType(CardType cardType, params CardType[] allowedCardTypes)
    {
        return allowedCardTypes.Contains(cardType);
    }

    /// <summary>
    /// Sprawdza czy status karty jest jednym z dozwolonych statusów
    /// </summary>
    /// <param name="cardStatus">Status karty do sprawdzenia</param>
    /// <param name="allowedCardStatuses">Dozwolone statusy karty</param>
    /// <returns>Czy status karty jest dozwolony</returns>
    protected bool IsAllowedCardStatus(CardStatus cardStatus, params CardStatus[] allowedCardStatuses)
    {
        return allowedCardStatuses.Contains(cardStatus);
    }

    /// <summary>
    /// Sprawdza czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Czy akcja jest dostępna</returns>
    public abstract bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet);
} 