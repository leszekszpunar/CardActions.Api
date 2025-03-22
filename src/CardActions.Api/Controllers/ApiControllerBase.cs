using CardActions.Api.Extensions;
using CardActions.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CardActions.Api.Controllers;

/// <summary>
/// Bazowy kontroler API obsługujący wzorzec Result
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Mediator do obsługi zapytań i komend
    /// </summary>
    protected readonly IMediator Mediator;

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="BaseApiController"/>.
    /// </summary>
    /// <param name="mediator">Mediator do obsługi zapytań i komend</param>
    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Obsługuje odpowiedź z wzorcem Result
    /// </summary>
    /// <typeparam name="T">Typ danych w Result</typeparam>
    /// <param name="result">Obiekt Result</param>
    /// <returns>ActionResult odpowiadający stanowi Result</returns>
    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        return result.ToActionResult(this);
    }
    
    /// <summary>
    /// Wysyła zapytanie przez mediator i obsługuje rezultat
    /// </summary>
    /// <typeparam name="TResponse">Typ odpowiedzi</typeparam>
    /// <param name="query">Zapytanie do wysłania</param>
    /// <returns>ActionResult odpowiadający stanowi Result</returns>
    protected async Task<ActionResult<TResponse>> HandleQuery<TResponse>(IRequest<Result<TResponse>> query)
    {
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
} 