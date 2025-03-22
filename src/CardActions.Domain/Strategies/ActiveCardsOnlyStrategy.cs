using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Strategia dostępności dla akcji dostępnych tylko dla aktywnych kart standardowych typów
/// </summary>
public class ActiveCardsOnlyStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Akcja jest dostępna dla kart typu Prepaid, Debit lub Credit o statusie Active
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return IsAllowedCardType(cardType, CardType.Prepaid, CardType.Debit, CardType.Credit) &&
               cardStatus == CardStatus.Active;
    }
} 