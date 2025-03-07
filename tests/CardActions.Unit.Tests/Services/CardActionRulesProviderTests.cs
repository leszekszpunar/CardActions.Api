using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CardActions.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CardActions.Unit.Tests.Services;

public class CardActionRulesProviderTests
{
    private readonly Mock<ILogger<CardActionRulesProvider>> _loggerMock;
    private readonly string _csvPath;

    public CardActionRulesProviderTests()
    {
        _loggerMock = new Mock<ILogger<CardActionRulesProvider>>();
        _csvPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/CardActions.Api/Resources/Allowed_Actions_Table.csv"));
    }

    [Fact]
    public void GetAllRules_ShouldReturnAllRulesFromCsv()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);

        // Act
        var rules = provider.GetAllRules();

        // Assert
        rules.ShouldNotBeEmpty();
        rules.ShouldAllBe(r => r is CardActionRule);
        rules.Select(r => r.ActionName).Distinct().Count().ShouldBeGreaterThan(1);
    }

    [Fact]
    public void GetAllActionNames_ShouldReturnDistinctActionNames()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);
        var expectedActionCount = 13; // Zgodnie z tabelą w zadaniu

        // Act
        var actionNames = provider.GetAllActionNames();

        // Assert
        actionNames.Count.ShouldBe(expectedActionCount);
        actionNames.Distinct().Count().ShouldBe(expectedActionCount);
        actionNames.ShouldContain("ACTION1");
        actionNames.ShouldContain("ACTION2");
        actionNames.ShouldContain("ACTION3");
        // ... i tak dalej
    }

    [Fact]
    public void Constructor_WhenFileNotFound_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "nonexistent.csv";

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => 
            new CardActionRulesProvider(nonExistentPath, _loggerMock.Object));
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Active)]
    [InlineData(CardType.Debit, CardStatus.Active)]
    [InlineData(CardType.Credit, CardStatus.Active)]
    public void GetAllRules_ShouldIncludeRulesForAllCardTypes(CardType cardType, CardStatus cardStatus)
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);

        // Act
        var rules = provider.GetAllRules();

        // Assert
        rules.ShouldContain(r => r.CardType == cardType && r.CardStatus == cardStatus);
    }

    [Fact]
    public void GetAllRules_ShouldIncludeRulesWithPinDependency()
    {
        // Arrange
        var provider = new CardActionRulesProvider(_csvPath, _loggerMock.Object);

        // Act
        var rules = provider.GetAllRules();

        // Assert
        rules.ShouldContain(r => r.RequiresPinSet.HasValue);
        rules.ShouldContain(r => r.RequiresPinSet == true);
        rules.ShouldContain(r => r.RequiresPinSet == false);
    }
} 