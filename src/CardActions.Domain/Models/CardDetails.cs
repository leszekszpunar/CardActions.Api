using System;

namespace CardActions.Domain.Models;

/// <summary>
/// Reprezentuje szczegóły karty płatniczej jako encję domenową.
/// Klasa ta zawiera podstawowe informacje o karcie, takie jak numer, typ, status i informację o PIN-ie.
/// </summary>
public class CardDetails
{
    /// <summary>
    /// Numer karty, który jednoznacznie ją identyfikuje.
    /// </summary>
    public string CardNumber { get; private set; }

    /// <summary>
    /// Typ karty (Prepaid, Debit, Credit).
    /// </summary>
    public CardType CardType { get; private set; }

    /// <summary>
    /// Status karty (Ordered, Inactive, Active, Restricted, Blocked, Expired, Closed).
    /// </summary>
    public CardStatus CardStatus { get; private set; }

    /// <summary>
    /// Określa, czy PIN jest ustawiony dla tej karty.
    /// </summary>
    public bool IsPinSet { get; private set; }

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="CardDetails"/>.
    /// </summary>
    /// <param name="cardNumber">Numer karty</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy cardNumber jest null</exception>
    public CardDetails(string cardNumber, CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        CardNumber = cardNumber ?? throw new ArgumentNullException(nameof(cardNumber));
        CardType = cardType;
        CardStatus = cardStatus;
        IsPinSet = isPinSet;
    }
} 