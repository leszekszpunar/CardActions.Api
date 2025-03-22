using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 4 dla karty - zawsze dostępna
/// </summary>
public class Action4CardAction : CardAction
{
    private const string ActionName = "ACTION4";
    private const string ActionDisplayName = "Sprawdzenie Limitu";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action4CardAction"/>.
    /// </summary>
    public Action4CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = true;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION4 jest zawsze dostępna dla wszystkich kart i wszystkich statusów
        IsAvailable = true;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        // Implementacja dla ACTION4
        await Task.Delay(40); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION4");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
}