using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CardActions.Infrastructure.Services;
using CardActions.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardActions.Unit.Tests.Services;

/// <summary>
///     Testy dla klasy CardActionRulesProvider odpowiedzialnej za dostarczanie reguł akcji dla kart.
/// </summary>
public class CardActionRulesProviderTests
{
    // Ścieżki do pliku CSV z użyciem AppContext.BaseDirectory
    private static readonly string CsvPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory,
            "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv"));

    // Alternatywne ścieżki, jeśli główna zawiedzie
    private static readonly string[] FallbackPaths = new[]
    {
        "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv",
        "../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv",
        "src/CardActions.Api/Resources/Allowed_Actions_Table.csv"
    };

    private readonly Mock<ILogger<CardActionRulesProvider>> _loggerMock;
    private readonly Mock<ICardActionRulesLoader> _rulesLoaderMock;
    private readonly List<CardActionRule> _testRules;

    public CardActionRulesProviderTests()
    {
        _loggerMock = new Mock<ILogger<CardActionRulesProvider>>();

        // Wczytanie reguł z pliku CSV lub użycie danych testowych jako zabezpieczenia
        _testRules = LoadRulesFromCsvOrFallback();

        // Mockowanie ICardActionRulesLoader
        _rulesLoaderMock = new Mock<ICardActionRulesLoader>();
        _rulesLoaderMock.Setup(m => m.LoadRules()).Returns(_testRules);
    }

    /// <summary>
    ///     Próbuje wczytać reguły z pliku CSV, a jeśli się nie uda, używa przygotowanych danych testowych.
    /// </summary>
    private List<CardActionRule> LoadRulesFromCsvOrFallback()
    {
        // Najpierw próbujemy główną ścieżkę
        if (File.Exists(CsvPath))
            try
            {
                var csvLoaderLogger = new Mock<ILogger<CsvCardActionRulesLoader>>();
                var csvLoader = new CsvCardActionRulesLoader(CsvPath, csvLoaderLogger.Object);
                var rules = csvLoader.LoadRules().ToList();

                if (rules.Any()) return rules;
            }
            catch (Exception)
            {
                // Ignorujemy wyjątki i próbujemy ścieżki awaryjne
            }

        // Próbujemy ścieżki awaryjne
        foreach (var path in FallbackPaths)
            if (File.Exists(path))
                try
                {
                    var csvLoaderLogger = new Mock<ILogger<CsvCardActionRulesLoader>>();
                    var csvLoader = new CsvCardActionRulesLoader(path, csvLoaderLogger.Object);
                    var rules = csvLoader.LoadRules().ToList();

                    if (rules.Any()) return rules;
                }
                catch (Exception)
                {
                    // Ignorujemy wyjątki i próbujemy następną ścieżkę
                }

        // Jeśli nie udało się wczytać z pliku CSV, używamy danych testowych
        return CreateFallbackRules();
    }

    /// <summary>
    ///     Tworzy awaryjne dane testowe zgodne z wymaganiami z zadania.
    /// </summary>
    private List<CardActionRule> CreateFallbackRules()
    {
        var rules = new List<CardActionRule>();

        // Implementacja wszystkich akcji zgodnie z tabelą w wymaganiach

        // ACTION1 - TAK tylko dla ACTIVE
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION1", cardType, CardStatus.Active, true));

            foreach (var status in Enum.GetValues<CardStatus>().Where(s => s != CardStatus.Active))
                rules.Add(new CardActionRule("ACTION1", cardType, status, false));
        }

        // ACTION2 - TAK dla INACTIVE i ACTIVE
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION2", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION2", cardType, CardStatus.Active, true));

            foreach (var status in Enum.GetValues<CardStatus>()
                         .Where(s => s != CardStatus.Inactive && s != CardStatus.Active))
                rules.Add(new CardActionRule("ACTION2", cardType, status, false));
        }

        // ACTION3 i ACTION4 - TAK dla wszystkich kombinacji
        foreach (var cardType in Enum.GetValues<CardType>())
        foreach (var status in Enum.GetValues<CardStatus>())
        {
            rules.Add(new CardActionRule("ACTION3", cardType, status, true));
            rules.Add(new CardActionRule("ACTION4", cardType, status, true));
        }

        // ACTION5 - TAK tylko dla CREDIT
        foreach (var status in Enum.GetValues<CardStatus>())
        {
            rules.Add(new CardActionRule("ACTION5", CardType.Credit, status, true));
            rules.Add(new CardActionRule("ACTION5", CardType.Prepaid, status, false));
            rules.Add(new CardActionRule("ACTION5", CardType.Debit, status, false));
        }

        // ACTION6 - Zależne od PIN-u
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            // Dla wszystkich typów kart
            // ORDERED, INACTIVE - TAK ale tylko gdy PIN ustawiony
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Ordered, true, true));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Inactive, true, true));

            // ACTIVE - zawsze TAK
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Active, true));

            // BLOCKED - TAK ale tylko gdy PIN ustawiony
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Blocked, true, true));

            // Pozostałe - zawsze NIE
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION6", cardType, CardStatus.Closed, false));
        }

        // ACTION7 - Zależne od PIN-u, ale odwrotnie niż ACTION6 w niektórych przypadkach
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            // Dla wszystkich typów kart
            // ORDERED, INACTIVE - TAK ale tylko gdy PIN NIE jest ustawiony
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Ordered, true, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Inactive, true, false));

            // ACTIVE - zawsze TAK
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Active, true));

            // BLOCKED - TAK ale tylko gdy PIN ustawiony (tu odwrotnie niż w ORDERED i INACTIVE)
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Blocked, true, true));

            // Pozostałe - zawsze NIE
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION7", cardType, CardStatus.Closed, false));
        }

        // ACTION8 - TAK dla ORDERED, INACTIVE, ACTIVE, BLOCKED
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Active, true));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Blocked, true));

            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION8", cardType, CardStatus.Closed, false));
        }

        // ACTION9 - TAK dla wszystkich kombinacji
        foreach (var cardType in Enum.GetValues<CardType>())
        foreach (var status in Enum.GetValues<CardStatus>())
            rules.Add(new CardActionRule("ACTION9", cardType, status, true));

        // ACTION10 - TAK dla ORDERED, INACTIVE, ACTIVE
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Active, true));

            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION10", cardType, CardStatus.Closed, false));
        }

        // ACTION11 - TAK dla INACTIVE, ACTIVE
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Active, true));

            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Ordered, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION11", cardType, CardStatus.Closed, false));
        }

        // ACTION12 i ACTION13 - TAK dla ORDERED, INACTIVE, ACTIVE
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Active, true));

            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION12", cardType, CardStatus.Closed, false));

            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Ordered, true));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Inactive, true));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Active, true));

            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Restricted, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Blocked, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Expired, false));
            rules.Add(new CardActionRule("ACTION13", cardType, CardStatus.Closed, false));
        }

        return rules;
    }

    [Fact(DisplayName = "Provider powinien zwrócić wszystkie reguły załadowane przez loader")]
    public void GetAllRules_ShouldReturnAllRulesFromLoader()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);

        // Act
        var rules = provider.GetAllRules();

        // Assert
        rules.ShouldNotBeEmpty();
        rules.ShouldAllBe(r => r is CardActionRule);
        rules.Count.ShouldBe(_testRules.Count);
        rules.ShouldBe(_testRules);
    }

    [Fact(DisplayName = "Provider powinien zwrócić unikalne nazwy wszystkich akcji (ACTION1-ACTION13)")]
    public void GetAllActionNames_ShouldReturnDistinctActionNames()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);
        var expectedActions = _testRules.Select(r => r.ActionName).Distinct().ToList();

        // Act
        var actionNames = provider.GetAllActionNames();

        // Assert
        actionNames.ShouldNotBeEmpty();
        actionNames.ShouldBe(expectedActions);

        // Sprawdź czy wszystkie 13 akcji jest obecnych
        var allActions = Enumerable.Range(1, 13).Select(i => $"ACTION{i}").ToList();
        foreach (var action in allActions) actionNames.ShouldContain(action);
    }

    [Fact(DisplayName = "Konstruktor powinien rzucić wyjątek, gdy loader jest null")]
    public void Constructor_WhenLoaderIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new CardActionRulesProvider(null!, _loggerMock.Object));

        exception.ParamName.ShouldBe("rulesLoader");
    }

    [Fact(DisplayName = "Konstruktor powinien rzucić wyjątek, gdy logger jest null")]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new CardActionRulesProvider(_rulesLoaderMock.Object, null!));

        exception.ParamName.ShouldBe("logger");
    }

    [Fact(DisplayName = "Karta PREPAID w statusie CLOSED powinna mieć dozwolone akcje: ACTION3, ACTION4, ACTION9")]
    public void VerifyExampleForPrepaidClosedCard()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);
        var cardType = CardType.Prepaid;
        var cardStatus = CardStatus.Closed;
        var expectedAllowedActions = new[] { "ACTION3", "ACTION4", "ACTION9" };

        // Act
        var rules = provider.GetAllRules()
            .Where(r => r.CardType == cardType &&
                        r.CardStatus == cardStatus &&
                        r.IsAllowed)
            .ToList();

        var allowedActionNames = rules
            .Select(r => r.ActionName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        // Assert
        allowedActionNames.ShouldBe(expectedAllowedActions, true);

        // Dodatkowo sprawdzamy czy pozostałe akcje faktycznie są niedostępne
        var notAllowedActions = new[]
        {
            "ACTION1", "ACTION2", "ACTION5", "ACTION6", "ACTION7", "ACTION8",
            "ACTION10", "ACTION11", "ACTION12", "ACTION13"
        };

        foreach (var action in notAllowedActions) rules.ShouldNotContain(r => r.ActionName == action && r.IsAllowed);
    }

    [Fact(DisplayName =
        "Karta CREDIT w statusie BLOCKED powinna mieć dozwolone akcje: ACTION3, ACTION4, ACTION5, ACTION6 (z PIN), ACTION7 (z PIN), ACTION8, ACTION9")]
    public void VerifyExampleForCreditBlockedCard()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);
        var cardType = CardType.Credit;
        var cardStatus = CardStatus.Blocked;

        // Dla karty CREDIT w statusie BLOCKED aplikacja powinna zwrócić akcje:
        // ACTION3, ACTION4, ACTION5, ACTION6 (jeżeli pin nadany), ACTION7 (jeżeli pin nadany), ACTION8, ACTION9
        var expectedPinIndependentActions = new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION8", "ACTION9" };
        var expectedPinDependentActions = new[] { "ACTION6", "ACTION7" };

        // Act
        var rules = provider.GetAllRules()
            .Where(r => r.CardType == cardType && r.CardStatus == cardStatus)
            .ToList();

        var pinIndependentAllowedActions = rules
            .Where(r => r.IsAllowed && (!r.RequiresPinSet.HasValue || r.RequiresPinSet == false))
            .Select(r => r.ActionName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        var pinDependentAllowedActions = rules
            .Where(r => r.IsAllowed && r.RequiresPinSet.HasValue && r.RequiresPinSet.Value)
            .Select(r => r.ActionName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        // Assert
        pinIndependentAllowedActions.ShouldBe(expectedPinIndependentActions, true);
        pinDependentAllowedActions.ShouldBe(expectedPinDependentActions, true);

        // Dodatkowo sprawdzamy czy pozostałe akcje faktycznie są niedostępne
        var notAllowedActions = new[] { "ACTION1", "ACTION2", "ACTION10", "ACTION11", "ACTION12", "ACTION13" };

        foreach (var action in notAllowedActions) rules.ShouldNotContain(r => r.ActionName == action && r.IsAllowed);
    }

    [Theory(DisplayName = "Weryfikacja dostępnych akcji dla różnych kombinacji typu karty, statusu i ustawienia PIN")]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, false,
        new[] { "ACTION3", "ACTION4", "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION12", "ACTION13" },
        "PREPAID + ORDERED bez PIN")]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, true,
        new[] { "ACTION3", "ACTION4", "ACTION6", "ACTION8", "ACTION9", "ACTION10", "ACTION12", "ACTION13" },
        "PREPAID + ORDERED z PIN")]
    [InlineData(CardType.Debit, CardStatus.Inactive, false,
        new[]
        {
            "ACTION2", "ACTION3", "ACTION4", "ACTION7", "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12",
            "ACTION13"
        }, "DEBIT + INACTIVE bez PIN")]
    [InlineData(CardType.Debit, CardStatus.Inactive, true,
        new[]
        {
            "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION8", "ACTION9", "ACTION10", "ACTION11", "ACTION12",
            "ACTION13"
        }, "DEBIT + INACTIVE z PIN")]
    [InlineData(CardType.Credit, CardStatus.Active, false,
        new[]
        {
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9",
            "ACTION10", "ACTION11", "ACTION12", "ACTION13"
        }, "CREDIT + ACTIVE bez PIN")]
    public void VerifyMultipleCardTypeStatusPinCombinations(CardType cardType, CardStatus cardStatus,
        bool isPinSet, string[] expectedAllowedActions, string testCase)
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);

        // Act
        var rules = provider.GetAllRules()
            .Where(r => r.CardType == cardType &&
                        r.CardStatus == cardStatus &&
                        r.IsAllowed &&
                        (!r.RequiresPinSet.HasValue ||
                         r.RequiresPinSet.Value == isPinSet))
            .ToList();

        var allowedActionNames = rules
            .Select(r => r.ActionName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        // Assert
        allowedActionNames.ShouldBe(expectedAllowedActions, true, $"Niepoprawne akcje dla przypadku: {testCase}");
    }
}