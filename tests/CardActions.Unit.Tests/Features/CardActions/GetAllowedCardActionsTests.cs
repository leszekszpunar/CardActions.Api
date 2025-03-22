using CardActions.Application.Common.Exceptions;
using CardActions.Application.Common.Interfaces;
using CardActions.Application.Common.Models;
using CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;
using CardActions.Application.Services;
using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Domain.Services.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Net;

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

    public GetAllowedCardActionsTests()
    {
        _cardServiceMock = new Mock<ICardService>();
        _cardActionServiceMock = new Mock<ICardActionService>();
        _localizationServiceMock = new Mock<ILocalizationService>();
        _loggerMock = new Mock<ILogger<GetAllowedCardActionsQueryHandler>>();

        _handler = new GetAllowedCardActionsQueryHandler(
            _cardServiceMock.Object,
            _cardActionServiceMock.Object,
            _localizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "Handler powinien zwrócić poprawne akcje dla karty PREPAID w statusie CLOSED z ustawionym PIN")]
    public async Task Handle_ForPrepaidCardInClosedStatusWithPin_ShouldReturnCorrectActions()
    {
        // Arrange
        var userId = "testUser";
        var cardNumber = "testCard";
        var expectedActionNames = new[] { "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION5" };
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);
        var cardDetails = new CardDetails(cardNumber, CardType.Prepaid, CardStatus.Closed, true);

        _cardServiceMock.Setup(x => x.GetCardDetailsAsync(userId, cardNumber))
            .ReturnsAsync(cardDetails);

        var cardActions = expectedActionNames.Select(name => CardAction.Create(name)).ToList();
        _cardActionServiceMock.Setup(x => x.GetAllowedActionsAsync(
                CardType.Prepaid, CardStatus.Closed, true))
            .ReturnsAsync(cardActions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue("Rezultat operacji powinien być pomyślny");
        result.StatusCode.ShouldBe(HttpStatusCode.OK, "Kod statusu powinien być 200 OK");
        foreach (var expectedAction in expectedActionNames)
            result.Data.AllowedActions.ShouldContain(expectedAction,
                $"Akcja {expectedAction} powinna być dozwolona dla karty PREPAID w statusie CLOSED");
        result.Data.AllowedActions.Count.ShouldBe(expectedActionNames.Length,
            "Liczba dozwolonych akcji powinna być zgodna z oczekiwaną");

        _cardServiceMock.Verify(x => x.GetCardDetailsAsync(userId, cardNumber), Times.Once);

        _cardActionServiceMock.Verify(x => x.GetAllowedActionsAsync(
            cardDetails.CardType, cardDetails.CardStatus, cardDetails.IsPinSet), Times.Once);
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

        _cardServiceMock.Setup(x => x.GetCardDetailsAsync(userId, cardNumber))
            .ReturnsAsync(cardDetails);

        var cardActions = expectedActionNames.Select(name => CardAction.Create(name)).ToList();
        _cardActionServiceMock.Setup(x => x.GetAllowedActionsAsync(
                CardType.Credit, CardStatus.Blocked, true))
            .ReturnsAsync(cardActions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue("Rezultat operacji powinien być pomyślny");
        result.StatusCode.ShouldBe(HttpStatusCode.OK, "Kod statusu powinien być 200 OK");
        foreach (var expectedAction in expectedActionNames)
            result.Data.AllowedActions.ShouldContain(expectedAction,
                $"Akcja {expectedAction} powinna być dozwolona dla karty CREDIT w statusie BLOCKED");
        result.Data.AllowedActions.Count.ShouldBe(expectedActionNames.Length,
            "Liczba dozwolonych akcji powinna być zgodna z oczekiwaną");

        _cardServiceMock.Verify(x => x.GetCardDetailsAsync(userId, cardNumber), Times.Once);

        _cardActionServiceMock.Verify(x => x.GetAllowedActionsAsync(
            cardDetails.CardType, cardDetails.CardStatus, cardDetails.IsPinSet), Times.Once);
    }

    [Fact(DisplayName = "Handler powinien zwrócić błąd dla nieistniejącej karty")]
    public async Task Handle_ForNonExistentCard_ShouldReturnCardNotFoundError()
    {
        // Arrange
        var userId = "testUser";
        var cardNumber = "nonExistentCard";
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);

        _cardServiceMock.Setup(x => x.GetCardDetailsAsync(userId, cardNumber))
            .ReturnsAsync((CardDetails?)null);

        _localizationServiceMock.Setup(x => x.GetString("Error.CardNotFound.Title"))
            .Returns("Card not found");
        _localizationServiceMock.Setup(x => x.GetString("Error.CardNotFound.Detail", It.IsAny<object[]>()))
            .Returns("Card not found for specified user");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse("Rezultat operacji powinien być niepomyślny");
        result.StatusCode.ShouldBe(HttpStatusCode.NotFound, "Kod statusu powinien być 404 NotFound");
        //result.ErrorMessage.ShouldContain("Card not found", "Komunikat błędu powinien zawierać informację o braku karty");

        _cardServiceMock.Verify(x => x.GetCardDetailsAsync(userId, cardNumber), Times.Once);

        _cardActionServiceMock.Verify(x => x.GetAllowedActionsAsync(
            It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact(DisplayName = "Handler powinien zwrócić błąd walidacji dla niepoprawnych danych wejściowych")]
    public async Task Handle_WithInvalidRequest_ShouldReturnValidationError()
    {
        // Arrange
        var userId = "";
        var cardNumber = "";
        var query = new GetAllowedCardActionsQuery(userId, cardNumber);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse("Rezultat operacji powinien być niepomyślny");
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest, "Kod statusu powinien być 400 BadRequest");
        result.ValidationErrors.ShouldNotBeNull("Lista błędów walidacji nie powinna być pusta");
        result.ValidationErrors.ShouldContain(e => e.Key == "UserId");
        result.ValidationErrors.ShouldContain(e => e.Key == "CardNumber");

        _cardServiceMock.Verify(x => x.GetCardDetailsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _cardActionServiceMock.Verify(x => x.GetAllowedActionsAsync(
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

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse($"Rezultat operacji powinien być niepomyślny - {testCase}");
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest, $"Kod statusu powinien być 400 BadRequest - {testCase}");
        result.ValidationErrors.ShouldNotBeNull($"Lista błędów walidacji nie powinna być pusta - {testCase}");
        
        if (string.IsNullOrEmpty(userId))
            result.ValidationErrors.ShouldContain(e => e.Key == "UserId");
            
        if (string.IsNullOrEmpty(cardNumber))
            result.ValidationErrors.ShouldContain(e => e.Key == "CardNumber");
    }
}