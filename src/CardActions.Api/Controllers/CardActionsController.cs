
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using CardActions.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NSwag.Annotations;

namespace CardActions.Api.Controllers;

/// <summary>
///     Kontroler obsługujący akcje dla kart płatniczych
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[EnableRateLimiting("api")]
[OpenApiTag("Akcje kart płatniczych", Description = "Zarządzanie akcjami na kartach płatniczych")]
[OpenApiController("Zarządzanie akcjami na kartach płatniczych")]
public class CardActionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<GetAllowedCardActionsQuery> _validator;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardActionsController" />.
    /// </summary>
    /// <param name="mediator">Mediator do obsługi zapytań i komend.</param>
    /// <param name="validator"></param>
    public CardActionsController(IMediator mediator, IValidator<GetAllowedCardActionsQuery> validator)
    {
        _mediator = mediator;
        _validator = validator;
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
        
        var validationResult = await _validator.ValidateAsync(query);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}