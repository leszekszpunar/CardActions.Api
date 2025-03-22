using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 2 dla karty
/// </summary>
public class Action2CardAction : CardAction
{
    private const string ActionName = "ACTION2";
    private const string ActionDisplayName = "Pierwsza Aktywacja";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action2CardAction"/>.
    /// </summary>
    public Action2CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION2 jest dostępna dla kart PREPAID, DEBIT i CREDIT w statusie INACTIVE
        IsAvailable = (cardType == Enums.CardType.Prepaid || 
                      cardType == Enums.CardType.Debit || 
                      cardType == Enums.CardType.Credit) &&
                      cardStatus == Enums.CardStatus.Inactive;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        if (!IsAvailable)
        {
            return CardActionResult.Failure("Akcja ACTION2 nie jest dostępna");
        }
        
        // Implementacja dla ACTION2
        await Task.Delay(35); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION2");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 