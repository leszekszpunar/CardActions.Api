using System.Threading.Tasks;
using CardActions.Application.Features.Version.Queries.GetVersionInfo;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace CardActions.Api.Controllers;

/// <summary>
/// Kontroler zwracający informacje o wersji aplikacji
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="VersionController"/>
    /// </summary>
    /// <param name="mediator">Mediator</param>
    /// <param name="environment">Środowisko hostingu</param>
    public VersionController(IMediator mediator, IHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    /// <summary>
    /// Pobiera szczegółowe informacje o wersji aplikacji
    /// </summary>
    /// <returns>Obiekt zawierający informacje o wersji</returns>
    [HttpGet]
    [ProducesResponseType(typeof(VersionInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        // Określ, czy zwracać szczegółowe informacje w zależności od środowiska
        var includeDetails = _environment.IsDevelopment() || 
                            _environment.EnvironmentName.ToLower().Contains("test");
        
        // Użyj konstruktora z parametrem
        var query = new GetVersionInfoQuery(includeDetails);
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }
} 