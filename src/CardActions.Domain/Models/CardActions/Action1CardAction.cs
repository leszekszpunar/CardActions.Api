using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 1 dla karty - dostępna dla wszystkich aktywnych kart
/// </summary>
public class Action1CardAction : CardAction
{
    private const string ActionName = "ACTION1";
    private const string ActionDisplayName = "Aktywacja Karty";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action1CardAction"/>.
    /// </summary>
    public Action1CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION1 jest dostępna dla kart PREPAID, DEBIT i CREDIT tylko w statusie ACTIVE
        IsAvailable = (cardType == Enums.CardType.Prepaid || 
                      cardType == Enums.CardType.Debit || 
                      cardType == Enums.CardType.Credit) &&
                      cardStatus == Enums.CardStatus.Active;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        if (!IsAvailable)
        {
            return CardActionResult.Failure("Akcja ACTION1 nie jest dostępna");
        }
        
        // Implementacja dla ACTION1
        await Task.Delay(30); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION1");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 