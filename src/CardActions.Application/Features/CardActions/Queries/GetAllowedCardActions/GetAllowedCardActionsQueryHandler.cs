using System.ComponentModel.DataAnnotations;
using CardActions.Application.Common.Base;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Common.Models;
using CardActions.Application.Services;
using CardActions.Domain.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;

/// <summary>
///     Obsługuje zapytanie o dozwolone akcje dla danej karty użytkownika.
///     Proces biznesowy:
///     1. Walidacja danych wejściowych
///     2. Pobranie szczegółów karty
///     3. Określenie dozwolonych akcji na podstawie parametrów karty
/// </summary>
internal sealed class GetAllowedCardActionsQueryHandler
    : CardActionHandlerBase, IRequestHandler<GetAllowedCardActionsQuery, Result<GetAllowedCardActionsResponse>>
{
    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="GetAllowedCardActionsQueryHandler" />.
    /// </summary>
    /// <param name="cardService">Serwis do pobierania informacji o kartach</param>
    /// <param name="cardActionService">Serwis do pobierania dozwolonych akcji dla kart</param>
    /// <param name="localizationService">Serwis do lokalizacji komunikatów</param>
    /// <param name="logger">Logger</param>
    public GetAllowedCardActionsQueryHandler(
        ICardService cardService,
        ICardActionService cardActionService,
        ILocalizationService localizationService,
        ILogger<GetAllowedCardActionsQueryHandler> logger)
        : base(cardService, cardActionService, localizationService, logger)
    {
    }

    /// <summary>
    ///     Obsługuje zapytanie o dozwolone akcje dla karty użytkownika.
    /// </summary>
    /// <param name="request">Zapytanie zawierające identyfikator użytkownika i numer karty</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Odpowiedź zawierająca listę dozwolonych akcji dla karty</returns>
    public async Task<Result<GetAllowedCardActionsResponse>> Handle(
        GetAllowedCardActionsQuery request,
        CancellationToken cancellationToken)
    {
        // Walidacja danych wejściowych
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);
        
        if (!isValid)
        {
            var errors = new Dictionary<string, List<string>>();
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    if (!errors.ContainsKey(memberName))
                        errors[memberName] = new List<string>();
                    
                    errors[memberName].Add(validationResult.ErrorMessage ?? "Validation error");
                }
            }
            
            Logger.LogWarning("Validation failed for request: {Errors}", 
                string.Join(", ", errors.SelectMany(e => e.Value)));
                
            return Result<GetAllowedCardActionsResponse>.ValidationFailure(errors);
        }
        
        Logger.LogInformation("Handling request for user '{UserId}' and card '{CardNumber}'", request.UserId,
            request.CardNumber);

        // Pobierz dane karty
        var cardDetails = await CardService.GetCardDetailsAsync(request.UserId, request.CardNumber);
        
        // Sprawdź, czy karta istnieje
        if (cardDetails == null)
        {
            var errorDetail = LocalizationService.GetString("Error.CardNotFound.Detail", request.CardNumber, request.UserId);
            
            Logger.LogWarning("Card not found: {CardNumber} for user {UserId}", request.CardNumber, request.UserId);
            
            return Result<GetAllowedCardActionsResponse>.NotFound(errorDetail);
        }
        
        // Pobierz dozwolone akcje na podstawie parametrów karty
        var allowedActions = await CardActionService.GetAllowedActionsAsync(
            cardDetails.CardType,
            cardDetails.CardStatus,
            cardDetails.IsPinSet);

        if (allowedActions == null || !allowedActions.Any())
        {
            Logger.LogWarning("Nie znaleziono dozwolonych akcji dla karty '{CardNumber}'", cardDetails.CardNumber);
            return Result<GetAllowedCardActionsResponse>.Success(new GetAllowedCardActionsResponse(new List<string>()));
        }

        var allowedActionNames = allowedActions.Select(a => a.Name).ToList();

        Logger.LogInformation("Dozwolone akcje dla karty '{CardNumber}': {AllowedActions}",
            cardDetails.CardNumber, string.Join(", ", allowedActionNames));

        return Result<GetAllowedCardActionsResponse>.Success(new GetAllowedCardActionsResponse(allowedActionNames));
    }
}