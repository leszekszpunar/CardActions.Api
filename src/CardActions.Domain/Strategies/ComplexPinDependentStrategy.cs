using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Złożona strategia dla akcji z kompleksowymi regułami dostępności zależnymi od PIN-u
/// </summary>
public class ComplexPinDependentStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Sprawdza złożone reguły dostępności dla ACTION6:
    /// - Dla kart Prepaid, Debit, Credit - dostępna zawsze
    /// - Dla statusów ORDERED, INACTIVE, ACTIVE - dostępna tylko jeśli NIE ma ustawionego PIN-u
    /// - Dla RESTRICTED - NIE
    /// - Dla BLOCKED - dostępna tylko jeśli PIN jest nadany
    /// - Dla EXPIRED, CLOSED - NIE
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        if (!IsAllowedCardType(cardType, CardType.Prepaid, CardType.Debit, CardType.Credit))
            return false;
            
        switch (cardStatus)
        {
            case CardStatus.Ordered:
            case CardStatus.Inactive:
            case CardStatus.Active:
                return !isPinSet; // Tylko jeśli NIE ma PIN-u
            case CardStatus.Restricted:
                return false;
            case CardStatus.Blocked:
                return isPinSet; // Tylko jeśli PIN jest nadany
            case CardStatus.Expired:
            case CardStatus.Closed:
                return false;
            default:
                return false;
        }
    }
} 