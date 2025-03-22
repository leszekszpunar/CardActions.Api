using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja 5 dla karty - dostępna tylko dla kart kredytowych
/// </summary>
public class Action5CardAction : CardAction
{
    private const string ActionName = "ACTION5";
    private const string ActionDisplayName = "Zmiana Limitu Kredytowego";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Action5CardAction"/>.
    /// </summary>
    public Action5CardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // ACTION5 jest dostępna tylko dla kart kredytowych, niezależnie od statusu
        IsAvailable = cardType == Enums.CardType.Credit;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        if (!IsAvailable)
        {
            return CardActionResult.Failure("Akcja ACTION5 nie jest dostępna");
        }
        
        // Implementacja dla ACTION5
        await Task.Delay(30); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Wykonano akcję ACTION5");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 