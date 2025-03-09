using CardActions.Application.Services;
using CardActions.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CardActions.Infrastructure.Services;

/// <summary>
///     Implementacja serwisu do pobierania informacji o kartach płatniczych.
/// </summary>
internal class CardService : ICardService
{
    private readonly ILogger<CardService> _logger;
    private readonly Dictionary<string, Dictionary<string, CardDetails>> _userCards;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardService" />.
    /// </summary>
    /// <param name="logger">Logger</param>
    public CardService(ILogger<CardService> logger)
    {
        _logger = logger;
        _userCards = CreateSampleUserCards();
    }

    /// <summary>
    ///     Pobiera szczegóły karty dla danego użytkownika i numeru karty.
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika</param>
    /// <param name="cardNumber">Numer karty</param>
    /// <returns>Szczegóły karty lub null, jeśli karta nie została znaleziona</returns>
    public async Task<CardDetails?> GetCardDetails(string userId, string cardNumber)
    {
        _logger.LogInformation("Getting card details for user '{UserId}' and card '{CardNumber}'", userId, cardNumber);

        // At this point, we would typically make an HTTP call to an external service
        // to fetch the data. For this example we use generated sample data.
        await Task.Delay(1000);

        if (!_userCards.TryGetValue(userId, out var cards))
        {
            _logger.LogWarning("User '{UserId}' not found", userId);
            return null;
        }

        if (!cards.TryGetValue(cardNumber, out var cardDetails))
        {
            _logger.LogWarning("Card '{CardNumber}' not found for user '{UserId}'", cardNumber, userId);
            return null;
        }

        _logger.LogInformation("Card '{CardNumber}' found for user '{UserId}'", cardNumber, userId);
        return cardDetails;
    }

    private static Dictionary<string, Dictionary<string, CardDetails>> CreateSampleUserCards()
    {
        var userCards = new Dictionary<string, Dictionary<string, CardDetails>>();
        for (var i = 1; i <= 3; i++)
        {
            var cards = new Dictionary<string, CardDetails>();
            var cardIndex = 1;
            foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
            foreach (CardStatus cardStatus in Enum.GetValues(typeof(CardStatus)))
            {
                var cardNumber = $"Card{i}{cardIndex}";
                cards.Add(cardNumber,
                    new CardDetails(
                        cardNumber,
                        cardType,
                        cardStatus,
                        cardIndex % 2 == 0));
                cardIndex++;
            }

            var userId = $"User{i}";
            userCards.Add(userId, cards);
        }

        return userCards;
    }
}