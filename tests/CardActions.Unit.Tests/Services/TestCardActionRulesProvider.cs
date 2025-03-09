using CardActions.Application.Services;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;

namespace CardActions.Unit.Tests.Services;

/// <summary>
///     Testowa implementacja ICardActionRulesProvider, która dostarcza reguły bezpośrednio z kodu.
/// </summary>
public class TestCardActionRulesProvider : ICardActionRulesProvider
{
    private readonly IReadOnlyList<string> _allActionNames;
    private readonly IReadOnlyCollection<CardActionRule> _rules;

    public TestCardActionRulesProvider()
    {
        _rules = PrepareTestRules();
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

    /// <summary>
    ///     Przygotowuje zestaw testowych reguł dla wszystkich akcji.
    /// </summary>
    private IReadOnlyCollection<CardActionRule> PrepareTestRules()
    {
        var rules = new List<CardActionRule>();

        // ACTION1 - tylko dla aktywnych kart
        foreach (var cardType in Enum.GetValues<CardType>())
            rules.Add(new CardActionRule("ACTION1", cardType, CardStatus.Active, true));

        // ACTION2 - dla aktywnych i nieaktywnych kart
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION2", cardType, CardStatus.Active, true));
            rules.Add(new CardActionRule("ACTION2", cardType, CardStatus.Inactive, true));
        }

        // ACTION3 - dla wszystkich typów kart i statusów
        foreach (var cardType in Enum.GetValues<CardType>())
        foreach (var cardStatus in Enum.GetValues<CardStatus>())
            rules.Add(new CardActionRule("ACTION3", cardType, cardStatus, true));

        // ACTION4 - dla wszystkich typów kart i statusów
        foreach (var cardType in Enum.GetValues<CardType>())
        foreach (var cardStatus in Enum.GetValues<CardStatus>())
            rules.Add(new CardActionRule("ACTION4", cardType, cardStatus, true));

        // ACTION5 - tylko dla kart kredytowych
        foreach (var cardStatus in Enum.GetValues<CardStatus>())
            rules.Add(new CardActionRule("ACTION5", CardType.Credit, cardStatus, true));

        // ACTION6 - zależne od PIN
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            // Dla statusów Ordered i Inactive - tylko gdy PIN jest ustawiony
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Ordered, true, true));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Inactive, true, true));

            // Dla statusu Active - zawsze dozwolone
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Active, true));

            // Dla statusu Blocked - tylko gdy PIN jest ustawiony
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Blocked, true, true));

            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Closed, false));
        }

        // ACTION7 - zależne od PIN
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            // Dla statusów Ordered i Inactive - tylko gdy PIN NIE jest ustawiony
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Ordered, true, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Inactive, true, false));

            // Dla statusu Active - zawsze dozwolone
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Active, true));

            // Dla statusu Blocked - tylko gdy PIN jest ustawiony
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Blocked, true, true));

            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Closed, false));
        }

        // ACTION8 - dla Ordered, Inactive, Active i Blocked
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Active, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Blocked, true));

            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Closed, false));
        }

        // ACTION9 - dla wszystkich typów kart i statusów
        foreach (var cardType in Enum.GetValues<CardType>())
        foreach (var cardStatus in Enum.GetValues<CardStatus>())
            rules.Add(new CardActionRule("ACTION9", cardType, cardStatus, true));

        // ACTION10 - dla Ordered, Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Active, true));

            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Closed, false));
        }

        // ACTION11 - dla Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Active, true));

            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Ordered, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Closed, false));
        }

        // ACTION12 - dla Ordered, Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Active, true));

            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Closed, false));
        }

        // ACTION13 - dla Ordered, Inactive, Active
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Active, true));

            // Dla pozostałych statusów - niedozwolone
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Closed, false));
        }

        return rules;
    }
}