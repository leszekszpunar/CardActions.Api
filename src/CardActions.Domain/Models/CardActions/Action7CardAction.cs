using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 7 dla karty
/// </summary>
public class Action7CardAction : CardAction
{
    private const string ActionName = "ACTION7";
    private const string ActionDisplayName = "Ustaw Pierwszy PIN";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action7CardAction"/>.
    /// </summary>
    public Action7CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION7 ma następujące reguły dostępności według tabeli:
        // - Dla kart Prepaid, Debit, Credit - dostępna w określonych warunkach
        // - Dla statusów ORDERED, INACTIVE, ACTIVE - dostępna tylko jeśli brak pinu
        // - Dla RESTRICTED - NIE
        // - Dla BLOCKED - dostępna tylko z pinem
        // - Dla EXPIRED, CLOSED - NIE
        
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
                IsAvailable = !isPinSet; // Tylko gdy nie ma pinu
                break;
            case Enums.CardStatus.Restricted:
                IsAvailable = false;
                break;
            case Enums.CardStatus.Blocked:
                IsAvailable = isPinSet; // Tylko gdy pin jest nadany
                break;
            case Enums.CardStatus.Expired:
            case Enums.CardStatus.Closed:
                IsAvailable = false;
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
            return CardActionResult.Failure("Akcja ACTION7 nie jest dostępna");
        }
        
        // Implementacja dla ACTION7
        await Task.Delay(45); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION7");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 