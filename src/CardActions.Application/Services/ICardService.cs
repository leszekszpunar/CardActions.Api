using CardActions.Domain.Models;

namespace CardActions.Application.Services;

/// <summary>
/// Definiuje serwis do pobierania informacji o kartach płatniczych.
/// </summary>
public interface ICardService
{
    /// <summary>
    /// Pobiera szczegóły karty dla danego użytkownika i numeru karty.
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika</param>
    /// <param name="cardNumber">Numer karty</param>
    /// <returns>Szczegóły karty lub null, jeśli karta nie została znaleziona</returns>
    Task<CardDetails?> GetCardDetails(string userId, string cardNumber);
} 