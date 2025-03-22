using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 9 dla karty - zawsze dostępna
/// </summary>
public class Action9CardAction : CardAction
{
    private const string ActionName = "ACTION9";
    private const string ActionDisplayName = "Szczegóły Karty";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action9CardAction"/>.
    /// </summary>
    public Action9CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = true;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION9 jest zawsze dostępna dla wszystkich kart i wszystkich statusów
        IsAvailable = true;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        // Implementacja dla ACTION9
        await Task.Delay(30); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION9");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 