using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Strategia dostępności dla akcji dostępnych dla kart o statusie Inactive lub Active
/// </summary>
public class InactiveAndActiveStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Akcja jest dostępna dla kart typu Prepaid, Debit lub Credit o statusie Inactive lub Active
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return IsAllowedCardType(cardType, CardType.Prepaid, CardType.Debit, CardType.Credit) &&
               IsAllowedCardStatus(cardStatus, CardStatus.Inactive, CardStatus.Active);
    }
} 