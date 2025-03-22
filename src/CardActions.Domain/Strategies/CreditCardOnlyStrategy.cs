using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Strategia dostępności dla akcji dostępnych tylko dla kart kredytowych
/// </summary>
public class CreditCardOnlyStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Akcja jest dostępna tylko dla kart typu Credit
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return cardType == CardType.Credit;
    }
} 