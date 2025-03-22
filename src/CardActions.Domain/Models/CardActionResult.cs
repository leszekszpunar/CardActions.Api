namespace CardActions.Domain.Models;

/// <summary>
///     Reprezentuje wynik wykonania akcji na karcie
/// </summary>
public class CardActionResult
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
    ///     Ewentualne dane wynikowe, związane z akcją
    /// </summary>
    public object Data { get; }

    /// <summary>
    ///     Konstruktor wyniku akcji
    /// </summary>
    /// <param name="success">Czy operacja zakończyła się sukcesem</param>
    /// <param name="message">Komunikat</param>
    /// <param name="data">Opcjonalne dane</param>
    private CardActionResult(bool success, string message, object? data = null)
    {
        Success = success;
        Message = message ?? string.Empty;
        Data = data;
    }

    /// <summary>
    ///     Tworzy wynik sukcesu
    /// </summary>
    /// <param name="message">Komunikat sukcesu</param>
    /// <param name="data">Opcjonalne dane</param>
    /// <returns>Wynik sukcesu</returns>
    public static CardActionResult SuccessResult(string message = "Operacja zakończona sukcesem", object? data = null)
    {
        return new CardActionResult(true, message, data);
    }

    /// <summary>
    ///     Tworzy wynik niepowodzenia
    /// </summary>
    /// <param name="message">Komunikat błędu</param>
    /// <param name="data">Opcjonalne dane</param>
    /// <returns>Wynik niepowodzenia</returns>
    public static CardActionResult Failure(string message = "Operacja zakończona niepowodzeniem", object? data = null)
    {
        return new CardActionResult(false, message, data);
    }

    /// <summary>
    ///     Tworzy wynik dla niedostępnej akcji
    /// </summary>
    /// <returns>Wynik niedostępnej akcji</returns>
    public static CardActionResult ActionNotAvailable()
    {
        return Failure("Akcja jest niedostępna dla tej karty");
    }
} 