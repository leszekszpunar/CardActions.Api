using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;

namespace CardActions.Domain.Models.CardActions;

/// <summary>
/// Akcja tymczasowego zablokowania karty
/// </summary>
public class TemporaryBlockCardAction : CardAction
{
    private const string ActionName = "TEMPORARY_BLOCK";
    private const string ActionDisplayName = "Tymczasowa Blokada Karty";
    
    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="TemporaryBlockCardAction"/>.
    /// </summary>
    public TemporaryBlockCardAction() : base(ActionName, ActionDisplayName, CardActionAvailabilityStrategyFactory.GetStrategy(ActionName))
    {
        IsAvailable = false;
    }
    
    /// <summary>
    /// Sprawdza, czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    public override bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // Tymczasowa blokada jest dostępna tylko dla aktywnych kart
        IsAvailable = cardStatus == Enums.CardStatus.Active;
        return IsAvailable;
    }
    
    /// <summary>
    /// Wykonuje akcję
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync()
    {
        if (!IsAvailable)
        {
            return CardActionResult.Failure("Tymczasowa blokada nie jest dostępna dla tej karty");
        }
        
        // Implementacja tymczasowej blokady
        await Task.Delay(40); // Symulacja operacji asynchronicznej
        
        return CardActionResult.SuccessResult("Karta została tymczasowo zablokowana");
    }
    
    /// <summary>
    /// Wykonuje akcję - metoda dla kompatybilności wstecznej
    /// </summary>
    public override async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        return await ExecuteAsync();
    }
} 