namespace CardActions.Domain.Models;

/// <summary>
///     Reprezentuje statusy kart płatniczych.
/// </summary>
public enum CardStatus
{
    /// <summary>
    ///     Karta zamówiona - karta została zamówiona, ale jeszcze nie została aktywowana.
    /// </summary>
    Ordered,

    /// <summary>
    ///     Karta nieaktywna - karta została dostarczona, ale nie została jeszcze aktywowana.
    /// </summary>
    Inactive,

    /// <summary>
    ///     Karta aktywna - karta jest aktywna i może być używana do płatności.
    /// </summary>
    Active,

    /// <summary>
    ///     Karta z ograniczeniami - karta jest aktywna, ale ma ograniczenia w użytkowaniu.
    /// </summary>
    Restricted,

    /// <summary>
    ///     Karta zablokowana - karta została tymczasowo zablokowana.
    /// </summary>
    Blocked,

    /// <summary>
    ///     Karta wygasła - karta przekroczyła datę ważności.
    /// </summary>
    Expired,

    /// <summary>
    ///     Karta zamknięta - karta została trwale zamknięta.
    /// </summary>
    Closed
}