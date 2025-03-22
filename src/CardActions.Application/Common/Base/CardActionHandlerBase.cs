using CardActions.Application.Common.Exceptions;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Services;
using CardActions.Domain.Models;
using CardActions.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CardActions.Application.Common.Base;

/// <summary>
/// Klasa bazowa dla handlerów obsługujących operacje na kartach
/// </summary>
public abstract class CardActionHandlerBase
{
    protected readonly ICardService CardService;
    protected readonly ICardActionService CardActionService;
    protected readonly ILocalizationService LocalizationService;
    protected readonly ILogger Logger;

    protected CardActionHandlerBase(
        ICardService cardService,
        ICardActionService cardActionService,
        ILocalizationService localizationService,
        ILogger logger)
    {
        CardService = cardService;
        CardActionService = cardActionService;
        LocalizationService = localizationService;
        Logger = logger;
    }

    /// <summary>
    /// Pobiera szczegóły karty i loguje jej istnienie
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika</param>
    /// <param name="cardNumber">Numer karty</param>
    /// <returns>Szczegóły karty lub null jeśli karta nie istnieje</returns>
    protected async Task<CardDetails?> GetCardDetailsAsync(string userId, string cardNumber)
    {
        var cardDetails = await CardService.GetCardDetailsAsync(userId, cardNumber);

        if (cardDetails is null)
        {
            Logger.LogWarning("Card '{CardNumber}' not found for user '{UserId}'", cardNumber, userId);
            return null;
        }

        Logger.LogInformation(
            "Card '{CardNumber}' found for user '{UserId}'. Type: {CardType}, Status: {CardStatus}, HasPin: {IsPinSet}",
            cardDetails.CardNumber, userId, cardDetails.CardType, cardDetails.CardStatus, cardDetails.IsPinSet);

        return cardDetails;
    }
} 