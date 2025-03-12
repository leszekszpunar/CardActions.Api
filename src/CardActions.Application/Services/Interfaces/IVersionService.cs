using System.Reflection;
using CardActions.Application.Features.Version.Queries.GetVersionInfo;

namespace CardActions.Application.Services.Interfaces;

/// <summary>
///     Interfejs serwisu do pobierania informacji o wersji aplikacji
/// </summary>
public interface IVersionService
{
    /// <summary>
    ///     Pobiera informacje o wersji z podanego assembly
    /// </summary>
    /// <param name="assembly">Assembly, z którego pobierane są informacje o wersji</param>
    /// <param name="includeDetailedInfo">Określa, czy zwracać szczegółowe informacje techniczne</param>
    /// <returns>DTO z informacjami o wersji</returns>
    VersionInfoDto GetVersionInfo(Assembly assembly, bool includeDetailedInfo = true);
}