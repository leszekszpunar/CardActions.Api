using CardActions.Application.Common.Exceptions;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using CardActions.Application.Services;
using CardActions.Domain.Models;
using CardActions.Domain.Services.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardActions.Unit.Tests.Features.CardActions;

/// <summary>
///     Testy jednostkowe dla handlera zapytania GetAllowedCardActions, który pobiera dozwolone akcje
///     dla karty użytkownika na podstawie jej typu, statusu i stanu PIN.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Features")]
[Trait("Feature", "GetAllowedCardActions")]
public class GetAllowedCardActionsTests
{
    private readonly Mock<ICardActionService> _cardActionServiceMock;
    private readonly Mock<ICardService> _cardServiceMock;
    private readonly GetAllowedCardActionsQueryHandler _handler;
    private readonly Mock<ILocalizationService> _localizationServiceMock;
    private readonly Mock<ILogger<GetAllowedCardActionsQueryHandler>> _loggerMock;
    private readonly Mock<IValidator<GetAllowedCardActionsQuery>> _validatorMock;

    public GetAllowedCardActionsTests()
    {
        _cardServiceMock = new Mock<ICardService>();
        _cardActionServiceMock = new Mock<ICardActionService>();
        _localizationServiceMock = new Mock<ILocalizationService>();
        _validatorMock = new Mock<IValidator<GetAllowedCardActionsQuery>>();
        _loggerMock = new Mock<ILogger<GetAllowedCardActionsQueryHandler>>();

        // Setup default validation behavior
        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<GetAllowedCardActionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new GetAllowedCardActionsQueryHandler(
            _cardServiceMock.Object,
            _cardActionServiceMock.Object,
            _validatorMock.Object,
            _localizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName =
        "Handler powinien zwrócić poprawne akcje (ACTION3, ACTION4, ACTION9) dla karty PREPAID w statusie CLOSED")]
    public async Task Handle_ForPrepaidCardInClosedStatus_ShouldReturnCorrectActions()
    {
        // Arrange
        var userId = "testUser";
        var cardNumber = "testCard";
        var expectedActionNames = new[] { "ACTION3", "ACTION4", "ACTION9" };
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        var cardDetails = new CardDetails(cardNumber, CardType.Prepaid, CardStatus.Closed, true);

        _cardServiceMock.Setup(x => x.GetCardDetails(userId, cardNumber))
            .ReturnsAsync(cardDetails);

        var cardActions = expectedActionNames.Select(name => CardAction.Create(name)).ToList();
        _cardActionServiceMock.Setup(x => x.GetAllowedActions(
                CardType.Prepaid, CardStatus.Closed, true))
            .Returns(cardActions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        foreach (var expectedAction in expectedActionNames)
            result.AllowedActions.ShouldContain(expectedAction,
                $"Akcja {expectedAction} powinna być dozwolona dla karty PREPAID w statusie CLOSED");
        result.AllowedActions.Count.ShouldBe(expectedActionNames.Length,
            "Liczba dozwolonych akcji powinna być zgodna z oczekiwaną");

        _cardServiceMock.Verify(x => x.GetCardDetails(userId, cardNumber), Times.Once);

        _cardActionServiceMock.Verify(x => x.GetAllowedActions(
            cardDetails.CardType, cardDetails.CardStatus, cardDetails.IsPinSet), Times.Once);

        _validatorMock.Verify(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler powinien zwrócić poprawne akcje dla karty CREDIT w statusie BLOCKED z ustawionym PIN")]
    public async Task Handle_ForCreditCardInBlockedStatusWithPin_ShouldReturnCorrectActions()
    {
        // Arrange
        var userId = "testUser";
        var cardNumber = "testCard";
        var expectedActionNames = new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" };
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        var cardDetails = new CardDetails(cardNumber, CardType.Credit, CardStatus.Blocked, true);

        _cardServiceMock.Setup(x => x.GetCardDetails(userId, cardNumber))
            .ReturnsAsync(cardDetails);

        var cardActions = expectedActionNames.Select(name => CardAction.Create(name)).ToList();
        _cardActionServiceMock.Setup(x => x.GetAllowedActions(
                CardType.Credit, CardStatus.Blocked, true))
            .Returns(cardActions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        foreach (var expectedAction in expectedActionNames) result.AllowedActions.ShouldContain(expectedAction);
        result.AllowedActions.Count.ShouldBe(expectedActionNames.Length);

        _cardServiceMock.Verify(x => x.GetCardDetails(userId, cardNumber), Times.Once);

        _cardActionServiceMock.Verify(x => x.GetAllowedActions(
            cardDetails.CardType, cardDetails.CardStatus, cardDetails.IsPinSet), Times.Once);

        _validatorMock.Verify(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler powinien rzucić wyjątek NotFoundException dla nieistniejącej karty")]
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
        _localizationServiceMock.Setup(x => x.GetString("Error.CardNotFound.Detail", It.IsAny<object[]>()))
            .Returns("Card not found for specified user");

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        exception.Message.ShouldContain("Card not found");

        _cardServiceMock.Verify(x => x.GetCardDetails(userId, cardNumber), Times.Once);

        _cardActionServiceMock.Verify(x => x.GetAllowedActions(
            It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()), Times.Never);

        _validatorMock.Verify(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler powinien rzucić wyjątek ValidationException dla niepoprawnych danych wejściowych")]
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
        var exception = await Should.ThrowAsync<ValidationException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        exception.Errors.ShouldContain(e => e.PropertyName == "UserId" && e.ErrorMessage == "User ID is required");

        exception.Errors.ShouldContain(e =>
            e.PropertyName == "CardNumber" && e.ErrorMessage == "Card number is required");

        _cardServiceMock.Verify(x => x.GetCardDetails(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _cardActionServiceMock.Verify(x => x.GetAllowedActions(
            It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory(DisplayName = "Handler powinien weryfikować dane wejściowe przed przetwarzaniem")]
    [InlineData("", "card123", "Brak ID użytkownika")]
    [InlineData("user123", "", "Brak numeru karty")]
    [InlineData("", "", "Brak ID użytkownika i numeru karty")]
    public async Task Handle_WithMissingRequiredData_ShouldValidateInput(string userId, string cardNumber,
        string testCase)
    {
        // Arrange - przygotowanie danych dla przypadku: {testCase}
        _loggerMock.Object.LogInformation($"Testowanie walidacji dla przypadku: {testCase}");

        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        var validationFailures = new List<ValidationFailure>();

        if (string.IsNullOrEmpty(userId))
            validationFailures.Add(new ValidationFailure("UserId", "User ID is required"));

        if (string.IsNullOrEmpty(cardNumber))
            validationFailures.Add(new ValidationFailure("CardNumber", "Card number is required"));

        _validatorMock
            .Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(
            async () => await _handler.Handle(query, CancellationToken.None),
            $"Brak wymaganego pola powinien skutkować wyjątkiem ValidationException - {testCase}");

        // Sprawdzenie czy walidator został wywołany dokładnie raz
        _validatorMock.Verify(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}