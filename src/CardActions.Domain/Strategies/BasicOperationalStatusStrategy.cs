using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Strategia dostępności dla akcji dostępnych w podstawowych stanach operacyjnych karty
/// </summary>
public class BasicOperationalStatusStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Akcja jest dostępna dla kart typu Prepaid, Debit lub Credit o statusie Ordered, Inactive lub Active
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return IsAllowedCardType(cardType, CardType.Prepaid, CardType.Debit, CardType.Credit) &&
               IsAllowedCardStatus(cardStatus, CardStatus.Ordered, CardStatus.Inactive, CardStatus.Active);
    }
} 