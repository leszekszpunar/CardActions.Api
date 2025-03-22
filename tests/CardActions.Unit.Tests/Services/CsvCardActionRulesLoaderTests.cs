using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardActions.Unit.Tests.Services;

/// <summary>
///     Testy dla klasy CsvCardActionRulesLoader odpowiedzialnej za wczytywanie reguł akcji z pliku CSV.
///     Testy weryfikują poprawność parsowania danych, obsługę błędów oraz zgodność z wymaganiami biznesowymi.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "RulesLoader")]
public class CsvCardActionRulesLoaderTests
{
    private readonly string _csvPath;
    private readonly Mock<ILogger<CsvCardActionRulesLoader>> _loggerMock;

    public CsvCardActionRulesLoaderTests()
    {
        _loggerMock = new Mock<ILogger<CsvCardActionRulesLoader>>();
        _csvPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv"));
    }

    [Fact(DisplayName = "Loader powinien poprawnie wczytać reguły z pliku CSV")]
    public void LoadRules_ShouldLoadRulesFromCsv()
    {
        // Arrange
        var loader = new CsvCardActionRulesLoader(_csvPath, _loggerMock.Object);

        // Act
        var rules = loader.LoadRules().ToList();

        // Assert
        rules.ShouldNotBeEmpty("Reguły nie powinny być puste");

        // Liczba unikalnych akcji powinna wynosić 13
        var uniqueActions = rules.Select(r => r.ActionName).Distinct().ToList();
        uniqueActions.Count.ShouldBe(13, "Powinno być dokładnie 13 unikalnych akcji");

        // Sprawdzanie czy wszystkie akcje są obecne
        for (var i = 1; i <= 13; i++) uniqueActions.ShouldContain($"ACTION{i}", $"Brakuje akcji ACTION{i}");
    }

    [Fact(DisplayName = "Konstruktor powinien rzucić wyjątek, gdy plik nie istnieje")]
    public void Constructor_WhenFileNotFound_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "non_existent_file.csv";

        // Act & Assert
        var exception = Should.Throw<FileNotFoundException>(() =>
            new CsvCardActionRulesLoader(nonExistentPath, _loggerMock.Object));

        exception.Message.ShouldContain(nonExistentPath,
            customMessage: "Wyjątek powinien zawierać informację o nieistniejącym pliku");
    }

    [Theory(DisplayName = "Loader powinien wczytać reguły dla wszystkich typów kart i statusów")]
    [InlineData(CardType.Prepaid, CardStatus.Active, "Karta przedpłacona w statusie aktywnym")]
    [InlineData(CardType.Debit, CardStatus.Active, "Karta debetowa w statusie aktywnym")]
    [InlineData(CardType.Credit, CardStatus.Active, "Karta kredytowa w statusie aktywnym")]
    public void LoadRules_ShouldIncludeRulesForAllCardTypes(CardType cardType, CardStatus cardStatus, string testCase)
    {
        // Arrange
        var loader = new CsvCardActionRulesLoader(_csvPath, _loggerMock.Object);

        // Act
        var rules = loader.LoadRules().ToList();

        // Assert
        rules.ShouldContain(r => r.CardType == cardType && r.CardStatus == cardStatus,
            $"Brak reguł dla przypadku: {testCase}");
    }

    [Fact(DisplayName = "Loader powinien wczytać reguły zależne od ustawienia PIN")]
    public void LoadRules_ShouldIncludeRulesWithPinDependency()
    {
        // Arrange
        var loader = new CsvCardActionRulesLoader(_csvPath, _loggerMock.Object);

        // Act
        var rules = loader.LoadRules().ToList();

        // Assert
        // Powinny istnieć reguły wymagające ustawionego PIN-u
        rules.ShouldContain(r => r.RequiresPinSet.HasValue && r.RequiresPinSet.Value,
            "Brak reguł wymagających ustawionego PIN-u");

        // Powinny istnieć reguły wymagające NIE ustawionego PIN-u
        rules.ShouldContain(r => r.RequiresPinSet.HasValue && !r.RequiresPinSet.Value,
            "Brak reguł wymagających NIE ustawionego PIN-u");

        // Sprawdzenie konkretnych reguł zależnych od PIN (np. ACTION6, ACTION7)
        rules.ShouldContain(r => r.ActionName == "ACTION6" && r.RequiresPinSet.HasValue,
            "ACTION6 powinna mieć reguły zależne od PIN");
        rules.ShouldContain(r => r.ActionName == "ACTION7" && r.RequiresPinSet.HasValue,
            "ACTION7 powinna mieć reguły zależne od PIN");
    }

    [Theory(DisplayName = "ParseRuleValue powinno prawidłowo interpretować wartości z CSV")]
    [InlineData("TAK", true, null, "Wartość 'TAK' powinna być interpretowana jako dozwolona")]
    [InlineData("NIE", false, null, "Wartość 'NIE' powinna być interpretowana jako niedozwolona")]
    [InlineData("TAK - ale jak nie ma pin to NIE", true, true, "Wartość warunkowa powinna wymagać PIN-u")]
    [InlineData("TAK - jeżeli pin nadany", true, true, "Wartość z warunkiem PIN-u nadanego")]
    [InlineData("TAK - jeżeli brak pin", true, false, "Wartość z warunkiem braku PIN-u")]
    public void ParseRuleValue_ShouldReturnCorrectValues(string value, bool expectedIsAllowed,
        bool? expectedRequiresPin, string testCase)
    {
        // Act
        var result = CsvCardActionRulesLoader.ParseRuleValue(value);

        // Assert
        result.isAllowed.ShouldBe(expectedIsAllowed, $"Niepoprawna wartość isAllowed dla '{value}' - {testCase}");
        result.requiresPinSet.ShouldBe(expectedRequiresPin,
            $"Niepoprawna wartość requiresPin dla '{value}' - {testCase}");
    }

    [Fact(DisplayName = "Reguły dla przykładu PREPAID+CLOSED powinny zawierać tylko ACTION3, ACTION4, ACTION9")]
    public void VerifyExampleForPrepaidClosedCard()
    {
        // Arrange
        var loader = new CsvCardActionRulesLoader(_csvPath, _loggerMock.Object);
        var cardType = CardType.Prepaid;
        var cardStatus = CardStatus.Closed;
        var expectedAllowedActions = new[] { "ACTION3", "ACTION4", "ACTION9" };

        // Act
        var rules = loader.LoadRules()
            .Where(r => r.CardType == cardType && r.CardStatus == cardStatus && r.IsAllowed)
            .ToList();

        var allowedActionNames = rules
            .Select(r => r.ActionName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        // Assert
        allowedActionNames.ShouldBe(expectedAllowedActions, true,
            "Dla karty PREPAID w statusie CLOSED dozwolone powinny być tylko ACTION3, ACTION4, ACTION9");

        // Sprawdź czy pozostałe akcje są niedozwolone
        var allActions = Enumerable.Range(1, 13).Select(i => $"ACTION{i}").ToList();
        var shouldNotBeAllowed = allActions.Except(expectedAllowedActions).ToList();

        foreach (var action in shouldNotBeAllowed)
            rules.ShouldNotContain(r => r.ActionName == action && r.IsAllowed,
                $"Akcja {action} nie powinna być dozwolona dla PREPAID+CLOSED");
    }

    [Fact(DisplayName = "Reguły dla przykładu CREDIT+BLOCKED powinny zawierać odpowiednie akcje z uwzględnieniem PIN")]
    public void VerifyExampleForCreditBlockedCard()
    {
        // Arrange
        var loader = new CsvCardActionRulesLoader(_csvPath, _loggerMock.Object);
        var cardType = CardType.Credit;
        var cardStatus = CardStatus.Blocked;
        var expectedPinIndependentActions = new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION8", "ACTION9" };
        var expectedPinDependentActions = new[] { "ACTION6", "ACTION7" };

        // Act
        var rules = loader.LoadRules()
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
        pinIndependentAllowedActions.ShouldBe(expectedPinIndependentActions, true,
            "Dla karty CREDIT w statusie BLOCKED akcje niezależne od PIN-u są niepoprawne");

        pinDependentAllowedActions.ShouldBe(expectedPinDependentActions, true,
            "Dla karty CREDIT w statusie BLOCKED akcje zależne od PIN-u są niepoprawne");

        // Sprawdź czy pozostałe akcje są niedozwolone
        var allActions = Enumerable.Range(1, 13).Select(i => $"ACTION{i}").ToList();
        var allowedActions = expectedPinIndependentActions.Concat(expectedPinDependentActions).ToList();
        var shouldNotBeAllowed = allActions.Except(allowedActions).ToList();

        foreach (var action in shouldNotBeAllowed)
            rules.ShouldNotContain(r => r.ActionName == action && r.IsAllowed,
                $"Akcja {action} nie powinna być dozwolona dla CREDIT+BLOCKED");
    }
}