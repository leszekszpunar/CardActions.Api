namespace CardActions.Domain.Models;

/// <summary>
///     Reprezentuje szczegóły karty płatniczej jako encję domenową, zawierającą podstawowe
///     informacje o karcie, takie jak numer, typ, status oraz informację o PIN.
///     Wzorce projektowe:
///     - Entity Pattern: Ta klasa reprezentuje encję biznesową z tożsamością określoną przez CardNumber.
///     Encje, w przeciwieństwie do obiektów wartościowych, mają swoją tożsamość niezależną od wartości.
///     - Encapsulation: Właściwości mają publiczne gettery, ale prywatne settery, co zapewnia
///     kontrolowaną modyfikację stanu encji.
///     - Validation: Konstruktor zawiera logikę walidacji, zapewniając integralność danych.
///     Zalety:
///     - Jasna reprezentacja encji biznesowej w modelu domenowym
///     - Kontrolowana modyfikacja stanu
///     - Centralizacja logiki walidacji w konstruktorze
/// </summary>
public class CardDetails
{
    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardDetails" />.
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

    /// <summary>
    ///     Numer karty, który jednoznacznie ją identyfikuje.
    /// </summary>
    public string CardNumber { get; private set; }

    /// <summary>
    ///     Typ karty (Prepaid, Debit, Credit).
    /// </summary>
    public CardType CardType { get; private set; }

    /// <summary>
    ///     Status karty (Ordered, Inactive, Active, Restricted, Blocked, Expired, Closed).
    /// </summary>
    public CardStatus CardStatus { get; private set; }

    /// <summary>
    ///     Określa, czy PIN jest ustawiony dla tej karty.
    /// </summary>
    public bool IsPinSet { get; private set; }
}