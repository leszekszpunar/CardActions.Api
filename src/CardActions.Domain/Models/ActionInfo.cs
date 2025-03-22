namespace CardActions.Domain.Models;

/// <summary>
/// Informacje o akcji
/// </summary>
public class ActionInfo
{
    /// <summary>
    /// Nazwa akcji
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Nazwa wyświetlana akcji
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// Czy akcja jest dostępna
    /// </summary>
    public bool IsAvailable { get; set; }
} 