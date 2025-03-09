using CardActions.Domain.Models;
using CardActions.Domain.Policies.Interfaces;
using CardActions.Domain.Services;
using Moq;

namespace CardActions.Unit.Tests.Services;

public class CardActionServiceTests
{
    private readonly IReadOnlyList<string> _allActionNames;
    private readonly Mock<ICardActionPolicy> _policyMock;
    private readonly CardActionService _service;

    public CardActionServiceTests()
    {
        _policyMock = new Mock<ICardActionPolicy>();
        _allActionNames = new List<string>
        {
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION5",
            "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10",
            "ACTION11", "ACTION12", "ACTION13"
        };
        _service = new CardActionService(_policyMock.Object, _allActionNames);
    }

    [Theory]
    [InlineData(CardType.Prepaid, CardStatus.Closed, true, new[] { "ACTION3", "ACTION4", "ACTION9" })]
    [InlineData(CardType.Credit, CardStatus.Blocked, true,
        new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION6", "ACTION7", "ACTION8", "ACTION9" })]
    [InlineData(CardType.Debit, CardStatus.Active, true,
        new[]
        {
            "ACTION1", "ACTION2", "ACTION3", "ACTION4", "ACTION6", "ACTION7", "ACTION8", "ACTION9", "ACTION10",
            "ACTION11", "ACTION12", "ACTION13"
        })]
    [InlineData(CardType.Credit, CardStatus.Restricted, false, new[] { "ACTION3", "ACTION4", "ACTION5", "ACTION9" })]
    public void GetAllowedActions_ForVariousCardTypesAndStatuses_ShouldReturnCorrectActions(
        CardType cardType,
        CardStatus cardStatus,
        bool isPinSet,
        string[] expectedActions)
    {
        // Arrange
        foreach (var action in _allActionNames)
        {
            var isAllowed = expectedActions.Contains(action);
            _policyMock.Setup(p => p.IsActionAllowed(action, cardType, cardStatus, isPinSet))
                .Returns(isAllowed);
        }

        // Act
        var result = _service.GetAllowedActions(cardType, cardStatus, isPinSet);

        // Assert
        result.Count.ShouldBe(expectedActions.Length);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in expectedActions) resultNames.ShouldContain(expectedAction);

        foreach (var action in _allActionNames)
            _policyMock.Verify(p => p.IsActionAllowed(action, cardType, cardStatus, isPinSet), Times.Once);
    }

    [Fact]
    public void GetAllActions_ShouldReturnAllActions()
    {
        // Act
        var result = _service.GetAllActions();

        // Assert
        result.Count.ShouldBe(_allActionNames.Count);
        var resultNames = result.Select(a => a.Name).ToList();
        foreach (var expectedAction in _allActionNames) resultNames.ShouldContain(expectedAction);
    }
}