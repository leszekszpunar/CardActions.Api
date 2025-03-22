using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Strategia dostępności dla akcji dostępnych tylko dla standardowych typów kart
/// </summary>
public class StandardCardTypeStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Akcja jest dostępna tylko dla kart typu Prepaid, Debit lub Credit
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return IsAllowedCardType(cardType, CardType.Prepaid, CardType.Debit, CardType.Credit);
    }
} 