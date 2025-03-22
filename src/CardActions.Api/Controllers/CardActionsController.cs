using CardActions.Api.Controllers;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NSwag.Annotations;

namespace CardActions.Api.Controllers;

/// <summary>
///     Kontroler obsługujący akcje dla kart płatniczych
/// </summary>
[Route("api/users")]
[Produces("application/json")]
[EnableRateLimiting("api")]
[OpenApiTag("Akcje kart płatniczych", Description = "Zarządzanie akcjami na kartach płatniczych")]
[OpenApiController("Zarządzanie akcjami na kartach płatniczych")]
public class CardActionsController : BaseApiController
{
    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardActionsController" />.
    /// </summary>
    /// <param name="mediator">Mediator do obsługi zapytań i komend.</param>
    public CardActionsController(IMediator mediator)
        : base(mediator)
    {
    }

    /// <summary>
    ///     Pobiera dozwolone akcje dla karty użytkownika
    /// </summary>
    /// <param name="userId">Identyfikator użytkownika</param>
    /// <param name="cardNumber">Numer karty</param>
    /// <returns>Lista dozwolonych akcji</returns>
    /// <response code="200">Zwraca dozwolone akcje dla karty</response>
    /// <response code="400">Błąd walidacji parametrów wejściowych</response>
    /// <response code="404">Karta nie została znaleziona</response>
    /// <response code="500">Błąd wewnętrzny serwera</response>
    [HttpGet("{userId}/cards/{cardNumber}/actions")]
    [OpenApiOperation("get-allowed-actions", "Pobiera dozwolone akcje dla karty",
        "Zwraca listę akcji, które użytkownik może wykonać na danej karcie płatniczej")]
    [ProducesResponseType(typeof(GetAllowedCardActionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetAllowedCardActionsResponse>> GetAllowedActions(
        [FromRoute] string userId,
        [FromRoute] string cardNumber)
    {
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        return await HandleQuery(query);
    }
}