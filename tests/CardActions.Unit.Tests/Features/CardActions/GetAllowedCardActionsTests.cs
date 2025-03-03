using CardActions.Application.Common.Interfaces;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using CardActions.Application.Services;
using CardActions.Domain.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CardActions.Unit.Tests.Features.CardActions;

public class GetAllowedCardActionsTests
{
    private readonly Mock<ICardService> _cardServiceMock;
    private readonly Mock<ICardActionRulesProvider> _rulesProviderMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock;
    private readonly Mock<IValidator<GetAllowedCardActionsQuery>> _validatorMock;
    private readonly Mock<ILogger<GetAllowedCardActionsQueryHandler>> _loggerMock;
    private readonly GetAllowedCardActionsQueryHandler _handler;

    public GetAllowedCardActionsTests()
    {
        _cardServiceMock = new Mock<ICardService>();
        _rulesProviderMock = new Mock<ICardActionRulesProvider>();
        _localizationServiceMock = new Mock<ILocalizationService>();
        _validatorMock = new Mock<IValidator<GetAllowedCardActionsQuery>>();
        _loggerMock = new Mock<ILogger<GetAllowedCardActionsQueryHandler>>();

        // Setup default validation behavior
        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<GetAllowedCardActionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new GetAllowedCardActionsQueryHandler(
            _cardServiceMock.Object, 
            _rulesProviderMock.Object,
            _validatorMock.Object,
            _localizationServiceMock.Object,
            _loggerMock.Object);
    }


    [Fact]
    public async Task Handle_ForPrepaidCardInClosedStatus_ShouldReturnCorrectActions()
    {
        // Arrange
        var userId = "testUser";
        var cardNumber = "testCard";
        var expectedActions = new[] { "ACTION3", "ACTION4", "ACTION9" };
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        var cardDetails = new CardDetails(cardNumber, CardType.Prepaid, CardStatus.Closed, true);

        _cardServiceMock.Setup(x => x.GetCardDetails(userId, cardNumber))
            .ReturnsAsync(cardDetails);

        _rulesProviderMock.Setup(x => x.GetAllowedActions(
                CardType.Prepaid, CardStatus.Closed, true))
            .Returns(expectedActions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AllowedActions.ShouldBe(expectedActions);
        _cardServiceMock.Verify(x => x.GetCardDetails(userId, cardNumber), Times.Once);
        _rulesProviderMock.Verify(x => x.GetAllowedActions(
            CardType.Prepaid, CardStatus.Closed, true), Times.Once);
        _validatorMock.Verify(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ForCreditCardInBlockedStatusWithPin_ShouldReturnCorrectActions()
    {
        // Arrange
        var userId = "testUser";
        var cardNumber = "testCard";
        var expectedActions = new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" };
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        var cardDetails = new CardDetails(cardNumber, CardType.Credit, CardStatus.Blocked, true);

        _cardServiceMock.Setup(x => x.GetCardDetails(userId, cardNumber))
            .ReturnsAsync(cardDetails);

        _rulesProviderMock.Setup(x => x.GetAllowedActions(
                CardType.Credit, CardStatus.Blocked, true))
            .Returns(expectedActions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AllowedActions.ShouldBe(expectedActions);
        _cardServiceMock.Verify(x => x.GetCardDetails(userId, cardNumber), Times.Once);
        _rulesProviderMock.Verify(x => x.GetAllowedActions(
            CardType.Credit, CardStatus.Blocked, true), Times.Once);
        _validatorMock.Verify(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ForNonExistentCard_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "testUser";
        var cardNumber = "nonExistentCard";
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);

        _cardServiceMock.Setup(x => x.GetCardDetails(userId, cardNumber))
            .ReturnsAsync((CardDetails?)null);

        _localizationServiceMock.Setup(x => x.GetString("Error.CardNotFound.Title"))
            .Returns("Card not found");
        _localizationServiceMock.Setup(x => x.GetString("Error.CardNotFound.Detail"))
            .Returns("Card {CardNumber} for user {UserId} was not found");

        // Act & Assert
        await Should.ThrowAsync<Application.Common.Exceptions.NotFoundException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        _cardServiceMock.Verify(x => x.GetCardDetails(userId, cardNumber), Times.Once);
        _rulesProviderMock.Verify(x => x.GetAllowedActions(
            It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()), Times.Never);
        _validatorMock.Verify(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var userId = "";
        var cardNumber = "";
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        var validationFailures = new List<ValidationFailure>
        {
            new("UserId", "User ID is required"),
            new("CardNumber", "Card number is required")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        _cardServiceMock.Verify(x => x.GetCardDetails(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _rulesProviderMock.Verify(x => x.GetAllowedActions(
            It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()), Times.Never);
    }
} 