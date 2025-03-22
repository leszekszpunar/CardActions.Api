namespace CardActions.Domain.Enums;

/// <summary>
/// Status karty
/// </summary>
public enum CardStatus
{
    /// <summary>
    /// Karta zamówiona
    /// </summary>
    Ordered = 0,
    
    /// <summary>
    /// Karta nieaktywna
    /// </summary>
    Inactive = 1,
    
    /// <summary>
    /// Karta aktywna
    /// </summary>
    Active = 2,
    
    /// <summary>
    /// Karta ograniczona
    /// </summary>
    Restricted = 3,
    
    /// <summary>
    /// Karta zablokowana
    /// </summary>
    Blocked = 4,
    
    /// <summary>
    /// Karta wygasła
    /// </summary>
    Expired = 5,
    
    /// <summary>
    /// Karta zamknięta
    /// </summary>
    Closed = 6
} 