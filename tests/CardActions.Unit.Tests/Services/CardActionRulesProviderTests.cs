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
    // Ścieżka do pliku CSV z użyciem AppContext.BaseDirectory
    private static readonly string CsvPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory,
            "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv"));

    private readonly Mock<ILogger<CardActionRulesProvider>> _loggerMock;
    private readonly Mock<ICardActionRulesLoader> _rulesLoaderMock;
    private readonly List<CardActionRule> _testRules;

    public CardActionRulesProviderTests()
    {
        _loggerMock = new Mock<ILogger<CardActionRulesProvider>>();

        // Sprawdzenie, czy plik CSV istnieje
        if (!File.Exists(CsvPath)) throw new FileNotFoundException($"Plik CSV z regułami nie istnieje: {CsvPath}");

        // Wczytanie reguł z pliku CSV
        var csvLoaderLogger = new Mock<ILogger<CsvCardActionRulesLoader>>();
        var csvLoader = new CsvCardActionRulesLoader(CsvPath, csvLoaderLogger.Object);
        _testRules = csvLoader.LoadRules().ToList();

        // Mockowanie ICardActionRulesLoader
        _rulesLoaderMock = new Mock<ICardActionRulesLoader>();
        _rulesLoaderMock.Setup(m => m.LoadRules()).Returns(_testRules);
    }

    [Fact]
    public void GetAllRules_ShouldReturnAllRules()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);

        // Act
        var result = provider.GetAllRules();

        // Assert
        result.ShouldBe(_testRules);
        _rulesLoaderMock.Verify(m => m.LoadRules(), Times.Once);
    }

    [Fact]
    public void GetAllActionNames_ShouldReturnDistinctActionNames()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);
        var expectedActionNames = _testRules
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        // Act
        var result = provider.GetAllActionNames();

        // Assert
        result.Count.ShouldBe(expectedActionNames.Count);
        foreach (var name in expectedActionNames) result.ShouldContain(name);
        _rulesLoaderMock.Verify(m => m.LoadRules(), Times.Once);
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, false,
        "PREPAID + ORDERED bez PIN")]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, true,
        "PREPAID + ORDERED z PIN")]
    [InlineData(CardType.Debit, CardStatus.Inactive, false,
        "DEBIT + INACTIVE bez PIN")]
    [InlineData(CardType.Debit, CardStatus.Inactive, true,
        "DEBIT + INACTIVE z PIN")]
    [InlineData(CardType.Credit, CardStatus.Active, false,
        "CREDIT + ACTIVE bez PIN")]
    public void VerifyMultipleCardTypeStatusPinCombinations(CardType cardType, CardStatus cardStatus,
        bool isPinSet, string testCase)
    {
        // Arrange
        var provider = new CardActionRulesProvider(_rulesLoaderMock.Object, _loggerMock.Object);

        // Act
        var allowedActions = provider.GetAllRules()
            .Where(r => r.CardType == cardType &&
                        r.CardStatus == cardStatus &&
                        r.IsAllowed &&
                        (!r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet))
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        // Assert
        allowedActions.ShouldNotBeEmpty($"Brak dozwolonych akcji dla przypadku: {testCase}");
    }
}