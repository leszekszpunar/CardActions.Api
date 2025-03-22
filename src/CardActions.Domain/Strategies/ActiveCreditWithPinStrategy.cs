using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Strategia dostępności dla akcji dostępnych tylko dla aktywnych kart kredytowych z ustawionym PIN-em
/// </summary>
public class ActiveCreditWithPinStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Akcja jest dostępna dla kart typu Credit o statusie Active z ustawionym PIN-em
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return cardType == CardType.Credit && 
               cardStatus == CardStatus.Active && 
               isPinSet;
    }
} 