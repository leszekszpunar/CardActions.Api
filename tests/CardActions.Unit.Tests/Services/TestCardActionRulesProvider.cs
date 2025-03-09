using CardActions.Application.Services;
using CardActions.Domain.Policies;
using CardActions.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardActions.Unit.Tests.Services;

/// <summary>
///     Testowa implementacja ICardActionRulesProvider, która dostarcza reguły bezpośrednio z pliku CSV.
/// </summary>
public class TestCardActionRulesProvider : ICardActionRulesProvider
{
    private readonly IReadOnlyList<string> _allActionNames;
    private readonly IReadOnlyCollection<CardActionRule> _rules;

    public TestCardActionRulesProvider()
    {
        // Ścieżka do pliku CSV
        var csvPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv"));

        // Wczytanie reguł z pliku CSV
        var logger = new Mock<ILogger<CsvCardActionRulesLoader>>().Object;
        var loader = new CsvCardActionRulesLoader(csvPath, logger);
        _rules = loader.LoadRules();

        _allActionNames = _rules
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();
    }

    public IReadOnlyCollection<CardActionRule> GetAllRules()
    {
        return _rules;
    }

    public IReadOnlyList<string> GetAllActionNames()
    {
        return _allActionNames;
    }
}