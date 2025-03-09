using CardActions.Application.Common.Exceptions;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Services;
using CardActions.Domain.Services.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;

/// <summary>
///     Obsługuje zapytanie o dozwolone akcje dla danej karty użytkownika.
///     Proces biznesowy:
///     1. Walidacja danych wejściowych
///        - Sprawdza poprawność identyfikatora użytkownika i numeru karty
///        - Jeśli dane są niepoprawne, przerywa proces
///     2. Pobranie szczegółów karty
///        - Pobiera informacje o karcie użytkownika
///        - Sprawdza czy karta istnieje
///        - Jeśli karta nie istnieje, zwraca błąd z odpowiednim komunikatem
///     3. Określenie dozwolonych akcji
///        - Na podstawie typu karty, statusu i ustawienia PIN-u
///        - Pobiera listę dozwolonych akcji
///        - Zwraca nazwy dozwolonych akcji użytkownikowi
/// </summary>
internal sealed class GetAllowedCardActionsQueryHandler
    : IRequestHandler<GetAllowedCardActionsQuery, GetAllowedCardActionsResponse>
{
    private readonly ICardActionService _cardActionService;
    private readonly ICardService _cardService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<GetAllowedCardActionsQueryHandler> _logger;
    private readonly IValidator<GetAllowedCardActionsQuery> _validator;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="GetAllowedCardActionsQueryHandler"/>.
    /// </summary>
    /// <param name="cardService">Serwis do pobierania informacji o kartach</param>
    /// <param name="cardActionService">Serwis do pobierania dozwolonych akcji dla kart</param>
    /// <param name="validator">Walidator zapytania</param>
    /// <param name="localizationService">Serwis do lokalizacji komunikatów</param>
    /// <param name="logger">Logger</param>
    public GetAllowedCardActionsQueryHandler(
        ICardService cardService,
        ICardActionService cardActionService,
        IValidator<GetAllowedCardActionsQuery> validator,
        ILocalizationService localizationService,
        ILogger<GetAllowedCardActionsQueryHandler> logger)
    {
        _cardService = cardService;
        _cardActionService = cardActionService;
        _validator = validator;
        _localizationService = localizationService;
        _logger = logger;
    }

    /// <summary>
    ///     Obsługuje zapytanie o dozwolone akcje dla karty użytkownika.
    ///     Proces biznesowy:
    ///     1. Walidacja - sprawdza poprawność danych wejściowych
    ///     2. Pobranie karty - weryfikuje istnienie karty i pobiera jej szczegóły
    ///     3. Określenie akcji - na podstawie parametrów karty ustala dozwolone akcje
    /// </summary>
    /// <param name="request">Zapytanie zawierające identyfikator użytkownika i numer karty</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Odpowiedź zawierająca listę dozwolonych akcji dla karty</returns>
    /// <exception cref="ValidationException">Rzucany, gdy zapytanie nie przejdzie walidacji</exception>
    /// <exception cref="NotFoundException">Rzucany, gdy karta nie zostanie znaleziona</exception>
    public async Task<GetAllowedCardActionsResponse> Handle(
        GetAllowedCardActionsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request for user '{UserId}' and card '{CardNumber}'", request.UserId,
            request.CardNumber);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for user '{UserId}' and card '{CardNumber}'. Errors: {Errors}",
                request.UserId, request.CardNumber,
                string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

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

        _logger.LogInformation(
            "Card '{CardNumber}' found for user '{UserId}'. Type: {CardType}, Status: {CardStatus}, Has PIN: {IsPinSet}",
            cardDetails.CardNumber, request.UserId, cardDetails.CardType, cardDetails.CardStatus, cardDetails.IsPinSet);

        var allowedActions = _cardActionService.GetAllowedActions(
            cardDetails.CardType,
            cardDetails.CardStatus,
            cardDetails.IsPinSet);

        _logger.LogInformation("Allowed actions for card '{CardNumber}': {AllowedActions}",
            cardDetails.CardNumber, string.Join(", ", allowedActions.Select(a => a.Name)));

        return new GetAllowedCardActionsResponse(allowedActions.Select(a => a.Name).ToList());
    }
}