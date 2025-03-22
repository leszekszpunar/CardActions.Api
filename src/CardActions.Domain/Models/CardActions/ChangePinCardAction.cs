using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja zmiany PIN-u dla karty
/// </summary>
public class ChangePinCardAction : CardAction
{
    private const string ActionName = "CHANGE_PIN";
    private const string ActionDisplayName = "Zmiana Kodu PIN";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="ChangePinCardAction"/>.
    /// </summary>
    public ChangePinCardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // Zmiana PIN-u jest dostępna tylko dla kart debetowych w statusie aktywnym
        IsAvailable = cardType == Enums.CardType.Debit && cardStatus == Enums.CardStatus.Active;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        if (!IsAvailable)
        {
            return CardActionResult.Failure("Akcja zmiany PIN-u nie jest dostępna");
        }
        
        // Implementacja dla zmiany PIN-u
        await Task.Delay(50); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("PIN został zmieniony");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 