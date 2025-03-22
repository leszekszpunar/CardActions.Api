using CardActions.Domain.Factories;
using CardActions.Domain.Strategies;
using Xunit;

namespace CardActions.Domain.Tests.Factories;

public class CardActionAvailabilityStrategyFactoryTests
{
    [Theory]
    [InlineData("ACTION1", typeof(ActiveCardsOnlyStrategy))]
    [InlineData("ACTION2", typeof(BasicOperationalStatusStrategy))]
    [InlineData("ACTION3", typeof(BasicOperationalStatusStrategy))]
    [InlineData("ACTION4", typeof(AlwaysAvailableStrategy))]
    [InlineData("ACTION6", typeof(ComplexPinDependentStrategy))]
    public void GetStrategy_ShouldReturnCorrectStrategy_ForValidActionName(string actionName, Type expectedStrategyType)
    {
        // Act
        var strategy = CardActionAvailabilityStrategyFactory.GetStrategy(actionName);
        
        // Assert
        Assert.NotNull(strategy);
        Assert.IsType(expectedStrategyType, strategy);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("INVALID_ACTION")]
    public void GetStrategy_ShouldReturnDefaultStrategy_ForInvalidActionName(string actionName)
    {
        // Act
        var strategy = CardActionAvailabilityStrategyFactory.GetStrategy(actionName);
        
        // Assert
        Assert.NotNull(strategy);
        
        // Default strategy should always return false for any card type/status
        Assert.False(strategy.IsAvailable(Enums.CardType.Prepaid, Enums.CardStatus.Active, true));
        Assert.False(strategy.IsAvailable(Enums.CardType.Debit, Enums.CardStatus.Active, true));
        Assert.False(strategy.IsAvailable(Enums.CardType.Credit, Enums.CardStatus.Active, true));
    }
} 