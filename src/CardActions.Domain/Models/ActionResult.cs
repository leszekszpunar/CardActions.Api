using System;
using System.Threading.Tasks;

namespace CardActions.Domain.Models;

/// <summary>
///     Reprezentuje wynik wykonania akcji (dla zachowania kompatybilności)
/// </summary>
public class ActionResult
{
    /// <summary>
    ///     Określa, czy akcja zakończyła się sukcesem
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    ///     Komunikat związany z wynikiem akcji
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Konstruktor wyniku akcji
    /// </summary>
    /// <param name="success">Czy operacja zakończyła się sukcesem</param>
    /// <param name="message">Komunikat</param>
    private ActionResult(bool success, string message)
    {
        Success = success;
        Message = message ?? string.Empty;
    }

    /// <summary>
    ///     Tworzy wynik sukcesu
    /// </summary>
    /// <param name="message">Komunikat sukcesu</param>
    /// <returns>Wynik sukcesu</returns>
    private static ActionResult SuccessResult(string message = "Operacja zakończona sukcesem")
    {
        return new ActionResult(true, message);
    }

    /// <summary>
    ///     Tworzy wynik niepowodzenia
    /// </summary>
    /// <param name="message">Komunikat błędu</param>
    /// <returns>Wynik niepowodzenia</returns>
    public static ActionResult Failure(string message = "Operacja zakończona niepowodzeniem")
    {
        return new ActionResult(false, message);
    }
    
    /// <summary>
    ///     Konwertuje ActionResult na CardActionResult
    /// </summary>
    /// <returns>CardActionResult</returns>
    public CardActionResult ToCardActionResult()
    {
        return Success 
            ? CardActionResult.SuccessResult(Message) 
            : CardActionResult.Failure(Message);
    }
    
    /// <summary>
    ///     Tworzy ActionResult na podstawie CardActionResult
    /// </summary>
    /// <param name="result">CardActionResult</param>
    /// <returns>ActionResult</returns>
    public static ActionResult FromCardActionResult(CardActionResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));
            
        return result.Success 
            ? SuccessResult(result.Message) 
            : Failure(result.Message);
    }
} 