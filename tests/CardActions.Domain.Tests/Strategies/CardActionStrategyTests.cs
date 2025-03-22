using CardActions.Domain.Enums;
using CardActions.Domain.Strategies;
using Xunit;

namespace CardActions.Domain.Tests.Strategies;

public class CardActionStrategyTests
{
    #region ActiveCardsOnlyStrategy Tests
    
    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Active, true)]
    [InlineData(CardType.Debit, CardStatus.Active, true)]
    [InlineData(CardType.Credit, CardStatus.Active, true)]
    [InlineData(CardType.Prepaid, CardStatus.Active, false)]
    [InlineData(CardType.Debit, CardStatus.Active, false)]
    [InlineData(CardType.Credit, CardStatus.Active, false)]
    public void ActiveCardsOnlyStrategy_ShouldReturnTrue_ForActiveCards(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var strategy = new ActiveCardsOnlyStrategy();
        
        // Act
        var result = strategy.IsAvailable(cardType, cardStatus, isPinSet);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, true)]
    [InlineData(CardType.Debit, CardStatus.Inactive, true)]
    [InlineData(CardType.Credit, CardStatus.Blocked, true)]
    [InlineData(CardType.Prepaid, CardStatus.Restricted, false)]
    [InlineData(CardType.Debit, CardStatus.Expired, false)]
    [InlineData(CardType.Credit, CardStatus.Closed, false)]
    public void ActiveCardsOnlyStrategy_ShouldReturnFalse_ForNonActiveCards(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var strategy = new ActiveCardsOnlyStrategy();
        
        // Act
        var result = strategy.IsAvailable(cardType, cardStatus, isPinSet);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region AlwaysAvailableStrategy Tests
    
    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Active, true)]
    [InlineData(CardType.Debit, CardStatus.Inactive, true)]
    [InlineData(CardType.Credit, CardStatus.Blocked, true)]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, false)]
    [InlineData(CardType.Debit, CardStatus.Restricted, false)]
    [InlineData(CardType.Credit, CardStatus.Expired, false)]
    [InlineData(CardType.Virtual, CardStatus.Closed, false)]
    public void AlwaysAvailableStrategy_ShouldAlwaysReturnTrue(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var strategy = new AlwaysAvailableStrategy();
        
        // Act
        var result = strategy.IsAvailable(cardType, cardStatus, isPinSet);
        
        // Assert
        Assert.True(result);
    }
    
    #endregion
    
    #region BasicOperationalStatusStrategy Tests
    
    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, true)]
    [InlineData(CardType.Debit, CardStatus.Inactive, true)]
    [InlineData(CardType.Credit, CardStatus.Active, true)]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, false)]
    [InlineData(CardType.Debit, CardStatus.Inactive, false)]
    [InlineData(CardType.Credit, CardStatus.Active, false)]
    public void BasicOperationalStatusStrategy_ShouldReturnTrue_ForBasicStatuses(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var strategy = new BasicOperationalStatusStrategy();
        
        // Act
        var result = strategy.IsAvailable(cardType, cardStatus, isPinSet);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Restricted, true)]
    [InlineData(CardType.Debit, CardStatus.Blocked, true)]
    [InlineData(CardType.Credit, CardStatus.Expired, true)]
    [InlineData(CardType.Prepaid, CardStatus.Closed, false)]
    [InlineData(CardType.Virtual, CardStatus.Active, true)]
    [InlineData(CardType.Unknown, CardStatus.Active, false)]
    public void BasicOperationalStatusStrategy_ShouldReturnFalse_ForNonBasicStatuses(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var strategy = new BasicOperationalStatusStrategy();
        
        // Act
        var result = strategy.IsAvailable(cardType, cardStatus, isPinSet);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region ComplexPinDependentStrategy Tests
    
    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, true)]
    [InlineData(CardType.Debit, CardStatus.Inactive, true)]
    [InlineData(CardType.Credit, CardStatus.Active, true)]
    public void ComplexPinDependentStrategy_ShouldReturnTrue_ForSupportedCardsWithPin(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var strategy = new ComplexPinDependentStrategy();
        
        // Act
        var result = strategy.IsAvailable(cardType, cardStatus, isPinSet);
        
        // Assert
        Assert.True(result);
    }
    
    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Ordered, false)]
    [InlineData(CardType.Debit, CardStatus.Inactive, false)]
    [InlineData(CardType.Credit, CardStatus.Active, false)]
    [InlineData(CardType.Prepaid, CardStatus.Blocked, true)]
    [InlineData(CardType.Debit, CardStatus.Expired, true)]
    [InlineData(CardType.Credit, CardStatus.Closed, true)]
    [InlineData(CardType.Virtual, CardStatus.Active, true)]
    public void ComplexPinDependentStrategy_ShouldReturnFalse_ForUnsupportedCombinations(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        // Arrange
        var strategy = new ComplexPinDependentStrategy();
        
        // Act
        var result = strategy.IsAvailable(cardType, cardStatus, isPinSet);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
} 