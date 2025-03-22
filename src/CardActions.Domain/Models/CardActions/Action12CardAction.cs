using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 12 dla karty
/// </summary>
public class Action12CardAction : CardAction
{
    private const string ActionName = "ACTION12";
    private const string ActionDisplayName = "Zmień Dane Karty";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action12CardAction"/>.
    /// </summary>
    public Action12CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION12 ma następujące reguły dostępności:
        // - Dla kart Prepaid, Debit, Credit - dostępna zawsze
        // - Dla statusów INACTIVE, ACTIVE - zawsze TAK
        // - Dla pozostałych statusów - NIE
        
        bool isAllowedCardType = cardType == Enums.CardType.Prepaid || 
                                cardType == Enums.CardType.Debit || 
                                cardType == Enums.CardType.Credit;
        
        if (!isAllowedCardType)
            return false;
            
        switch (cardStatus)
        {
            case Enums.CardStatus.Inactive:
            case Enums.CardStatus.Active:
                IsAvailable = true;
                break;
            default:
                IsAvailable = false;
                break;
        }
        
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        if (!IsAvailable)
        {
            return CardActionResult.Failure("Akcja ACTION12 nie jest dostępna");
        }
        
        // Implementacja dla ACTION12
        await Task.Delay(45); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION12");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 