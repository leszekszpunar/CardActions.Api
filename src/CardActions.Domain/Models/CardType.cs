namespace CardActions.Domain.Models;

/// <summary>
///   Reprezentuje typy kart płatniczych.
/// </summary>
public enum CardType
{
    /// <summary>
    ///   Karta przedpłacona.
    /// </summary>
    Prepaid,
    /// <summary>
    ///  Karta debetowa.
    /// </summary>
    Debit,
    /// <summary>
    ///  Karta kredytowa.
    /// </summary>
    Credit
}