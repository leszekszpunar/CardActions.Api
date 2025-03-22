using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Domain.Strategies;
using Moq;
using Xunit;

namespace CardActions.Domain.Tests.Models;

public class CardActionTests
{
    private const string ActionName = "TEST_ACTION";
    private const string ActionDisplayName = "Test Action";
    
    [Fact]
    public void Create_ShouldReturnCardActionWithCorrectProperties()
    {
        // Act
        var cardAction = CardAction.Create(ActionName, ActionDisplayName);
        
        // Assert
        Assert.Equal(ActionName, cardAction.Name);
        Assert.Equal(ActionDisplayName, cardAction.DisplayName);
    }
    
    [Fact]
    public void Create_ShouldUseActionNameAsDisplayName_WhenDisplayNameIsNull()
    {
        // Act
        var cardAction = CardAction.Create(ActionName);
        
        // Assert
        Assert.Equal(ActionName, cardAction.Name);
        Assert.Equal(ActionName, cardAction.DisplayName);
    }
    
    [Fact]
    public void CheckAvailability_ShouldDelegateToStrategy()
    {
        // Arrange
        var mockStrategy = new Mock<ICardActionAvailabilityStrategy>();
        mockStrategy
            .Setup(s => s.IsAvailable(CardType.Prepaid, CardStatus.Active, true))
            .Returns(true);
            
        var cardAction = new CardAction(ActionName, ActionDisplayName, mockStrategy.Object);
        
        // Act
        var result = cardAction.CheckAvailability(CardType.Prepaid, CardStatus.Active, true);
        
        // Assert
        Assert.True(result);
        mockStrategy.Verify(s => s.IsAvailable(CardType.Prepaid, CardStatus.Active, true), Times.Once);
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessResult_WhenActionIsAvailable()
    {
        // Arrange
        var mockStrategy = new Mock<ICardActionAvailabilityStrategy>();
        mockStrategy
            .Setup(s => s.IsAvailable(It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()))
            .Returns(true);
            
        var cardAction = new CardAction(ActionName, ActionDisplayName, mockStrategy.Object);
        
        // Act
        var result = await cardAction.ExecuteAsync(CardType.Prepaid, CardStatus.Active, true);
        
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Contains("Wykonano akcję", result.Message);
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailureResult_WhenActionIsNotAvailable()
    {
        // Arrange
        var mockStrategy = new Mock<ICardActionAvailabilityStrategy>();
        mockStrategy
            .Setup(s => s.IsAvailable(It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()))
            .Returns(false);
            
        var cardAction = new CardAction(ActionName, ActionDisplayName, mockStrategy.Object);
        
        // Act
        var result = await cardAction.ExecuteAsync(CardType.Prepaid, CardStatus.Blocked, true);
        
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Akcja nie jest dostępna dla tej karty", result.Message);
    }
} 