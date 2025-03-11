using MediatR;

namespace CardActions.Application.Features.Version.Queries.GetVersionInfo;

/// <summary>
/// Query do pobierania informacji o wersji aplikacji
/// </summary>
public class GetVersionInfoQuery : IRequest<VersionInfoDto>
{
    /// <summary>
    /// Określa, czy zwracać szczegółowe informacje o wersji
    /// </summary>
    /// <remarks>
    /// Domyślnie ustawione na true
    /// </remarks>
    public bool IncludeDetailedInfo { get; private set; } = true;
    
    /// <summary>
    /// Tworzy nową instancję zapytania z domyślnymi wartościami
    /// </summary>
    public GetVersionInfoQuery()
    {
    }
    
    /// <summary>
    /// Tworzy nową instancję zapytania z określoną wartością includeDetailedInfo
    /// </summary>
    /// <param name="includeDetailedInfo">Czy zwracać szczegółowe informacje</param>
    public GetVersionInfoQuery(bool includeDetailedInfo)
    {
        IncludeDetailedInfo = includeDetailedInfo;
    }
} 