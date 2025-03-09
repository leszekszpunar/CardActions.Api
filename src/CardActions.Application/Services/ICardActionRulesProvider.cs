using CardActions.Domain.Policies;

namespace CardActions.Application.Services;

/// <summary>
///     Definiuje dostawcę reguł akcji karty.
///     Interfejs ten jest częścią warstwy aplikacji i definiuje kontrakt dla dostawcy,
///     który dostarcza reguły akcji karty do warstwy domenowej.
/// </summary>
public interface ICardActionRulesProvider
{
    /// <summary>
    ///     Pobiera wszystkie reguły akcji karty.
    /// </summary>
    /// <returns>Kolekcja reguł akcji karty</returns>
    IReadOnlyCollection<CardActionRule> GetAllRules();

    /// <summary>
    ///     Pobiera wszystkie nazwy akcji karty.
    /// </summary>
    /// <returns>Lista nazw akcji karty</returns>
    IReadOnlyList<string> GetAllActionNames();
}