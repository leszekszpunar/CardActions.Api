using CardActions.Application.Services;
using CardActions.Domain.Enums;
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

        // Log created users and number of cards for debugging
        foreach (var user in _userCards.Keys)
            _logger.LogInformation("Created sample user '{UserId}' with {CardCount} cards",
                user, _userCards[user].Count);
    }

    /// <summary>
    ///     Pobiera szczegóły karty dla danego użytkownika i numeru karty.
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika</param>
    /// <param name="cardNumber">Numer karty</param>
    /// <returns>Szczegóły karty lub null, jeśli karta nie została znaleziona</returns>
    public async Task<CardDetails?> GetCardDetailsAsync(string userId, string cardNumber)
    {
        // At this point, we would typically make an HTTP call to an external service
        // to fetch the data. For this example we use generated sample data.
        await Task.Delay(1000);
        if (!_userCards.TryGetValue(userId, out var cards)
            || !cards.TryGetValue(cardNumber, out var cardDetails))
        {
            return null;
        }

        return cardDetails;
    }

    /// <summary>
    ///   Tworzy przykładowe dane kart dla użytkowników.
    /// </summary>
    /// <returns>Przykładowe dane kart dla użytkowników</returns>
    private static Dictionary<string, Dictionary<string, CardDetails>> CreateSampleUserCards()
    {
        var userCards = new Dictionary<string, Dictionary<string, CardDetails>>();
        for (var i = 1; i <= 3; i++)
        {
            var cards = new Dictionary<string, CardDetails>();
            var cardIndex = 1;
            foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
            {
                foreach (CardStatus cardStatus in Enum.GetValues(typeof(CardStatus)))
                {
                    var cardNumber = $"Card{i}{cardIndex}";
                    cards.Add(cardNumber,
                        new CardDetails(
                            cardNumber: cardNumber,
                            cardType: cardType,
                            cardStatus: cardStatus,
                            isPinSet: cardIndex % 2 == 0));
                    cardIndex++;
                }
            }

            var userId = $"User{i}";
            userCards.Add(userId, cards);
        }

        return userCards;
    }
}