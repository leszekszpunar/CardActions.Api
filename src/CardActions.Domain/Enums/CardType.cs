namespace CardActions.Domain.Enums;

/// <summary>
/// Typ karty
/// </summary>
public enum CardType
{
    /// <summary>
    /// Nieznany typ karty
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Karta przedpłacona
    /// </summary>
    Prepaid = 1,
    
    /// <summary>
    /// Karta debetowa
    /// </summary>
    Debit = 2,
    
    /// <summary>
    /// Karta kredytowa
    /// </summary>
    Credit = 3,
    
    /// <summary>
    /// Karta wirtualna
    /// </summary>
    Virtual = 4
} 