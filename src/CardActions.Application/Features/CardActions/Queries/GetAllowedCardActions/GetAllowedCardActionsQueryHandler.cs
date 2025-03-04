using CardActions.Application.Services;
using CardActions.Application.Common.Exceptions;
using CardActions.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;

/// <summary>
/// Obsługuje zapytanie o dozwolone akcje dla danej karty użytkownika.
/// </summary>
internal sealed class GetAllowedCardActionsQueryHandler 
    : IRequestHandler<GetAllowedCardActionsQuery, GetAllowedCardActionsResponse>
{
    private readonly ICardService _cardService;
    private readonly ICardActionRulesProvider _rulesProvider;
    private readonly IValidator<GetAllowedCardActionsQuery> _validator;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<GetAllowedCardActionsQueryHandler> _logger;

    public GetAllowedCardActionsQueryHandler(
        ICardService cardService,
        ICardActionRulesProvider rulesProvider,
        IValidator<GetAllowedCardActionsQuery> validator,
        ILocalizationService localizationService,
        ILogger<GetAllowedCardActionsQueryHandler> logger)
    {
        _cardService = cardService;
        _rulesProvider = rulesProvider;
        _validator = validator;
        _localizationService = localizationService;
        _logger = logger;
    }

    public async Task<GetAllowedCardActionsResponse> Handle(
        GetAllowedCardActionsQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request for user '{UserId}' and card '{CardNumber}'", request.UserId, request.CardNumber);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for user '{UserId}' and card '{CardNumber}'. Errors: {Errors}",
                request.UserId, request.CardNumber, string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            throw new ValidationException(validationResult.Errors);
        }

        var cardDetails = await _cardService.GetCardDetails(request.UserId, request.CardNumber);
        
        if (cardDetails is null)
        {
            _logger.LogWarning("Card '{CardNumber}' not found for user '{UserId}'", request.CardNumber, request.UserId);
            
            throw new NotFoundException(
                _localizationService.GetString("Error.CardNotFound.Title"),
                _localizationService.GetString("Error.CardNotFound.Detail", request.CardNumber, request.UserId)
            );
        }

        _logger.LogInformation("Card '{CardNumber}' found for user '{UserId}'. Type: {CardType}, Status: {CardStatus}, Has PIN: {IsPinSet}",
            cardDetails.CardNumber, request.UserId, cardDetails.CardType, cardDetails.CardStatus, cardDetails.IsPinSet);

        var allowedActions = _rulesProvider.GetAllowedActions(
            cardDetails.CardType,
            cardDetails.CardStatus,
            cardDetails.IsPinSet);

        _logger.LogInformation("Allowed actions for card '{CardNumber}': {AllowedActions}", 
            cardDetails.CardNumber, string.Join(", ", allowedActions));

        return new GetAllowedCardActionsResponse(allowedActions);
    }
}
