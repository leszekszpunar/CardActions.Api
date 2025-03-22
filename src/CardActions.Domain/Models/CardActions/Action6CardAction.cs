using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 6 dla karty
/// </summary>
public class Action6CardAction : CardAction
{
    private const string ActionName = "ACTION6";
    private const string ActionDisplayName = "Zmiana Kodu PIN";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action6CardAction"/>.
    /// </summary>
    public Action6CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION6 ma następujące reguły dostępności według tabeli:
        // - Dla kart Prepaid, Debit, Credit - dostępna w określonych warunkach
        // - Dla statusów ORDERED, INACTIVE, ACTIVE - dostępna tylko jeśli NIE ma ustawionego PIN-u
        // - Dla RESTRICTED - NIE
        // - Dla BLOCKED - dostępna tylko jeśli PIN jest nadany
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
                IsAvailable = !isPinSet; // Tylko jeśli NIE ma PIN-u
                break;
            case Enums.CardStatus.Restricted:
                IsAvailable = false;
                break;
            case Enums.CardStatus.Blocked:
                IsAvailable = isPinSet; // Tylko jeśli PIN jest nadany
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
            return CardActionResult.Failure("Akcja ACTION6 nie jest dostępna");
        }
        
        // Implementacja dla ACTION6
        await Task.Delay(25); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION6");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 