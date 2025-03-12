using CardActions.Api.Controllers;
using CardActions.Application.Features.Version.Queries.GetVersionInfo;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;

namespace CardActions.Unit.Tests.Controllers;

/// <summary>
///     Testy jednostkowe dla VersionController, który zwraca informacje o wersji aplikacji
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Controllers")]
[Trait("Feature", "Version")]
public class VersionControllerTests
{
    private readonly VersionController _controller;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly Mock<IMediator> _mediatorMock;

    public VersionControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _environmentMock = new Mock<IHostEnvironment>();
        _controller = new VersionController(_mediatorMock.Object, _environmentMock.Object);
    }

    [Fact(DisplayName = "Get powinien zwracać szczegółowe informacje o wersji w środowisku deweloperskim")]
    public async Task Get_InDevelopmentEnvironment_ShouldReturnDetailedVersionInfo()
    {
        // Arrange - symulujemy środowisko developerskie
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        var versionInfo = new VersionInfoDto
        {
            Version = "1.0.0",
            FileVersion = "1.0.0",
            AssemblyVersion = "1.0.0.0",
            FullVersion = "1.0.0+abc1234",
            Product = "CardActions.Api",
            Description = "Test Description",
            CommitHash = "abc1234",
            BuildDate = "2023-11-01T10:00:00Z",
            ReleaseChannel = "development"
        };

        // Setup mediatora z parametrem includeDetailedInfo=true
        _mediatorMock.Setup(m => m.Send(It.Is<GetVersionInfoQuery>(q => q.IncludeDetailedInfo == true),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(versionInfo);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnedVersionInfo = okResult.Value.ShouldBeOfType<VersionInfoDto>();

        returnedVersionInfo.Version.ShouldBe(versionInfo.Version);
        returnedVersionInfo.CommitHash.ShouldBe(versionInfo.CommitHash);
        returnedVersionInfo.BuildDate.ShouldBe(versionInfo.BuildDate);
        returnedVersionInfo.ReleaseChannel.ShouldBe(versionInfo.ReleaseChannel);

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetVersionInfoQuery>(q => q.IncludeDetailedInfo == true), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Get powinien zwracać podstawowe informacje o wersji w środowisku produkcyjnym")]
    public async Task Get_InProductionEnvironment_ShouldReturnBasicVersionInfo()
    {
        // Arrange - symulujemy środowisko produkcyjne
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        var versionInfo = new VersionInfoDto
        {
            Version = "1.0.0",
            FileVersion = "1.0.0",
            AssemblyVersion = "1.0.0.0",
            FullVersion = "1.0.0+abc1234",
            Product = "CardActions.Api",
            Description = "Test Description",
            CommitHash = "abc1234",
            BuildDate = "Niedostępne",
            ReleaseChannel = "production"
        };

        // Setup mediatora z parametrem includeDetailedInfo=false
        _mediatorMock.Setup(m => m.Send(It.Is<GetVersionInfoQuery>(q => q.IncludeDetailedInfo == false),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(versionInfo);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnedVersionInfo = okResult.Value.ShouldBeOfType<VersionInfoDto>();

        returnedVersionInfo.Version.ShouldBe(versionInfo.Version);
        returnedVersionInfo.CommitHash.ShouldBe(versionInfo.CommitHash);
        returnedVersionInfo.BuildDate.ShouldBe(versionInfo.BuildDate);
        returnedVersionInfo.ReleaseChannel.ShouldBe(versionInfo.ReleaseChannel);

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetVersionInfoQuery>(q => q.IncludeDetailedInfo == false), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Get powinien zwracać szczegółowe informacje dla środowiska testowego")]
    public async Task Get_InTestEnvironment_ShouldReturnDetailedVersionInfo()
    {
        // Arrange - symulujemy środowisko testowe
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Testing");

        var versionInfo = new VersionInfoDto
        {
            Version = "1.0.0",
            FileVersion = "1.0.0",
            AssemblyVersion = "1.0.0.0",
            FullVersion = "1.0.0+abc1234",
            Product = "CardActions.Api",
            Description = "Test Description",
            CommitHash = "abc1234",
            BuildDate = "2023-11-01T10:00:00Z",
            ReleaseChannel = "test"
        };

        // Setup mediatora z parametrem includeDetailedInfo=true
        _mediatorMock.Setup(m => m.Send(It.Is<GetVersionInfoQuery>(q => q.IncludeDetailedInfo == true),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(versionInfo);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnedVersionInfo = okResult.Value.ShouldBeOfType<VersionInfoDto>();

        returnedVersionInfo.Version.ShouldBe(versionInfo.Version);
        returnedVersionInfo.CommitHash.ShouldBe(versionInfo.CommitHash);
        returnedVersionInfo.BuildDate.ShouldBe(versionInfo.BuildDate);
        returnedVersionInfo.ReleaseChannel.ShouldBe(versionInfo.ReleaseChannel);

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetVersionInfoQuery>(q => q.IncludeDetailedInfo == true), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}