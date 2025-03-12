using System.Reflection;
using CardActions.Application.Services.Interfaces;
using MediatR;

namespace CardActions.Application.Features.Version.Queries.GetVersionInfo;

/// <summary>
///     Handler zapytania o informacje o wersji aplikacji
/// </summary>
internal class GetVersionInfoQueryHandler : IRequestHandler<GetVersionInfoQuery, VersionInfoDto>
{
    private readonly IVersionService _versionService;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="GetVersionInfoQueryHandler" />
    /// </summary>
    /// <param name="versionService">Serwis wersji</param>
    public GetVersionInfoQueryHandler(IVersionService versionService)
    {
        _versionService = versionService;
    }

    /// <inheritdoc />
    public Task<VersionInfoDto> Handle(GetVersionInfoQuery request, CancellationToken cancellationToken)
    {
        // Pobieramy informacje z bieżącego assembly
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var versionInfo = _versionService.GetVersionInfo(assembly, request.IncludeDetailedInfo);

        return Task.FromResult(versionInfo);
    }
}