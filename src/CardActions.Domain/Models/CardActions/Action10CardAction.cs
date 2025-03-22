using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 10 dla karty
/// </summary>
public class Action10CardAction : CardAction
{
    private const string ActionName = "ACTION10";
    private const string ActionDisplayName = "Zastrzeż Kartę";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action10CardAction"/>.
    /// </summary>
    public Action10CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION10 ma następujące reguły dostępności:
        // - Dla kart Prepaid, Debit, Credit - dostępna zawsze
        // - Dla statusów ORDERED, INACTIVE, ACTIVE - zawsze TAK
        // - Dla pozostałych statusów - NIE
        
        bool isAllowedCardType = cardType == Enums.CardType.Prepaid || 
                                cardType == Enums.CardType.Debit || 
                                cardType == Enums.CardType.Credit;
        
        if (!isAllowedCardType)
            return false;
            
        switch (cardStatus)
        {
            case Enums.CardStatus.Ordered:
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
            return CardActionResult.Failure("Akcja ACTION10 nie jest dostępna");
        }
        
        // Implementacja dla ACTION10
        await Task.Delay(40); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION10");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 