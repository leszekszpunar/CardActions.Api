using CardActions.Domain.Enums;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Strategies;

/// <summary>
/// Strategia dostępności dla akcji, które są zawsze dostępne niezależnie od parametrów karty
/// </summary>
public class AlwaysAvailableStrategy : BaseCardActionAvailabilityStrategy
{
    /// <summary>
    /// Akcja jest zawsze dostępna niezależnie od parametrów karty
    /// </summary>
    public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        return true;
    }
} 