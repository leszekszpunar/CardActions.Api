using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 3 dla karty - zawsze dostępna
/// </summary>
public class Action3CardAction : CardAction
{
    private const string ActionName = "ACTION3";
    private const string ActionDisplayName = "Sprawdzenie Statusu";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action3CardAction"/>.
    /// </summary>
    public Action3CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = true;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION3 jest zawsze dostępna dla wszystkich kart i wszystkich statusów
        IsAvailable = true;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        // Implementacja dla ACTION3
        await Task.Delay(40); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION3");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 