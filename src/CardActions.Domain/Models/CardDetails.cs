using System;

namespace CardActions.Domain.Models;

/// <summary>
/// Reprezentuje szczegóły karty płatniczej jako encję domenową.
/// </summary>
public class CardDetails
{
    public string CardNumber { get; private set; }
    public CardType CardType { get; private set; }
    public CardStatus CardStatus { get; private set; }
    public bool IsPinSet { get; private set; }

    public CardDetails(string cardNumber, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        CardNumber = cardNumber ?? throw new ArgumentNullException(nameof(cardNumber));
        CardType = cardType;
        CardStatus = cardStatus;
        IsPinSet = isPinSet;
    }
} 