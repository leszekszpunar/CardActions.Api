using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 11 dla karty
/// </summary>
public class Action11CardAction : CardAction
{
    private const string ActionName = "ACTION11";
    private const string ActionDisplayName = "Wydaj Duplikat";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action11CardAction"/>.
    /// </summary>
    public Action11CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION11 ma następujące reguły dostępności:
        // - Dla kart Prepaid, Debit, Credit - dostępna w statusach INACTIVE, ACTIVE
        
        if (!(cardType == Enums.CardType.Prepaid || cardType == Enums.CardType.Debit || cardType == Enums.CardType.Credit))
            return false;
            
        if (cardStatus == Enums.CardStatus.Inactive || cardStatus == Enums.CardStatus.Active)
            IsAvailable = true;
        else
            IsAvailable = false;
        
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        if (!IsAvailable)
        {
            return CardActionResult.Failure("Akcja ACTION11 nie jest dostępna");
        }
        
        // Implementacja dla ACTION11
        await Task.Delay(50); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION11");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 