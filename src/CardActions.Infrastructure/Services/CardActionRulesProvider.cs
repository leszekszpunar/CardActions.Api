using CardActions.Application.Services;
using CardActions.Domain.Policies;
using CardActions.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CardActions.Infrastructure.Services;

/// <summary>
///     Implementacja dostawcy reguł akcji karty.
///     Proces biznesowy:
///     1. Inicjalizacja
///     - Wczytuje reguły z zewnętrznego źródła (np. pliku CSV)
///     - Tworzy listę wszystkich dostępnych akcji
///     2. Zarządzanie regułami
///     - Przechowuje reguły w pamięci
///     - Udostępnia metody do pobierania wszystkich reguł
///     - Udostępnia metody do pobierania nazw wszystkich akcji
///     3. Monitorowanie
///     - Loguje informacje o załadowanych regułach
///     - Ułatwia debugowanie i monitorowanie systemu
///     Wzorce projektowe:
///     - Repository Pattern: Ta klasa działa jako repozytorium reguł akcji karty,
///     oddzielając logikę dostępu do danych od logiki biznesowej.
///     - Dependency Injection: Loader reguł jest wstrzykiwany przez konstruktor, co ułatwia testowanie.
///     Zalety:
///     - Odseparowanie dostępu do danych od logiki biznesowej
///     - Elastyczność źródła danych (można łatwo zmienić na inny format pliku lub bazę danych)
///     - Centralne miejsce do zarządzania regułami
///     - Obsługa błędów i logowanie
///     Zasady SOLID:
///     - Single Responsibility: Klasa ma jedno zadanie - dostarczanie reguł
///     - Open/Closed: Można rozszerzyć funkcjonalność bez modyfikacji istniejącego kodu
///     - Liskov Substitution: Implementuje interfejs ICardActionRulesProvider
///     - Interface Segregation: Interfejs ICardActionRulesProvider jest mały i spójny
///     - Dependency Inversion: Zależność od loadera reguł jest wstrzykiwana
/// </summary>
internal sealed class CardActionRulesProvider : ICardActionRulesProvider
{
    /// <summary>
    ///     Lista wszystkich nazw akcji karty.
    /// </summary>
    private readonly IReadOnlyList<string> _allActionNames;

    /// <summary>
    ///     Logger używany do logowania informacji, ostrzeżeń i błędów.
    /// </summary>
    private readonly ILogger<CardActionRulesProvider> _logger;

    /// <summary>
    ///     Kolekcja reguł akcji karty.
    /// </summary>
    private readonly IReadOnlyCollection<CardActionRule> _rules;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardActionRulesProvider" />.
    /// </summary>
    /// <param name="rulesLoader">Loader reguł akcji karty</param>
    /// <param name="logger">Logger używany do logowania</param>
    public CardActionRulesProvider(ICardActionRulesLoader rulesLoader, ILogger<CardActionRulesProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (rulesLoader == null) throw new ArgumentNullException(nameof(rulesLoader));

        _rules = rulesLoader.LoadRules();
        _allActionNames = _rules
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        PrintLoadedRules();
    }

    /// <summary>
    ///     Pobiera wszystkie reguły akcji karty.
    /// </summary>
    /// <returns>Kolekcja reguł akcji karty</returns>
    public IReadOnlyCollection<CardActionRule> GetAllRules()
    {
        return _rules;
    }

    /// <summary>
    ///     Pobiera wszystkie nazwy akcji karty.
    /// </summary>
    /// <returns>Lista nazw akcji karty</returns>
    public IReadOnlyList<string> GetAllActionNames()
    {
        return _allActionNames;
    }

    /// <summary>
    ///     Wyświetla załadowane reguły w logach (tylko w trybie Debug).
    /// </summary>
    private void PrintLoadedRules()
    {
        foreach (var rule in _rules)
            _logger.LogDebug(
                $"Action: {rule.ActionName}, CardType: {rule.CardType}, CardStatus: {rule.CardStatus}, Allowed: {rule.IsAllowed}, RequiresPin: {rule.RequiresPinSet}");
    }
}