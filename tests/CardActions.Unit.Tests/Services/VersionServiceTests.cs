using System.Reflection;
using CardActions.Application.Features.Version.Queries.GetVersionInfo;
using CardActions.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardActions.Unit.Tests.Services;

/// <summary>
/// Testy jednostkowe dla VersionService, który pobiera informacje o wersji aplikacji
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Services")]
[Trait("Feature", "Version")]
public class VersionServiceTests
{
    private readonly Mock<ILogger<VersionService>> _loggerMock;
    private readonly VersionService _service;
    private readonly Assembly _testAssembly;

    public VersionServiceTests()
    {
        _loggerMock = new Mock<ILogger<VersionService>>();
        _service = new VersionService(_loggerMock.Object);
        _testAssembly = typeof(VersionServiceTests).Assembly;
    }

    [Fact(DisplayName = "GetVersionInfo powinien zwrócić podstawowe informacje o wersji")]
    public void GetVersionInfo_ShouldReturnBasicVersionInfo()
    {
        // Arrange
        var includeDetailedInfo = true;

        // Act
        var result = _service.GetVersionInfo(_testAssembly, includeDetailedInfo);

        // Assert
        result.ShouldNotBeNull();
        result.Version.ShouldNotBeNullOrEmpty();
        result.FileVersion.ShouldNotBeNullOrEmpty();
        result.AssemblyVersion.ShouldNotBeNullOrEmpty();
        result.FullVersion.ShouldNotBeNullOrEmpty();
        result.Product.ShouldNotBeNullOrEmpty();
        result.Description.ShouldNotBeNullOrEmpty();
    }

    [Fact(DisplayName = "GetVersionInfo z includeDetailedInfo=false powinien zwrócić ograniczone informacje")]
    public void GetVersionInfo_WithDetailedInfoFalse_ShouldReturnLimitedInfo()
    {
        // Arrange
        var includeDetailedInfo = false;

        // Act
        var result = _service.GetVersionInfo(_testAssembly, includeDetailedInfo);

        // Assert
        result.ShouldNotBeNull();
        result.Version.ShouldNotBeNullOrEmpty();
        result.BuildDate.ShouldBe("Niedostępne");
    }

    [Fact(DisplayName = "GetVersionInfo powinien obsłużyć brak metadanych")]
    public void GetVersionInfo_ShouldHandleMissingMetadata()
    {
        // Arrange
        var includeDetailedInfo = true;

        // Act
        var result = _service.GetVersionInfo(_testAssembly, includeDetailedInfo);

        // Assert
        result.ShouldNotBeNull();
        // Nawet jeśli metadane są niedostępne, service powinien zwrócić jakieś domyślne wartości
        result.ReleaseChannel.ShouldNotBeNull();
        result.CommitHash.ShouldNotBeNull();
        result.BuildDate.ShouldNotBeNull();
    }

    [Fact(DisplayName = "GetVersionInfo powinien wyciągnąć hash commita z informacyjnej wersji")]
    public void GetVersionInfo_ShouldExtractCommitHashFromInformationalVersion()
    {
        // Arrange & Act
        var result = _service.GetVersionInfo(_testAssembly, true);

        // Assert
        result.ShouldNotBeNull();
        
        // Sprawdzamy, czy hash commita został wyciągnięty z InformationalVersion
        if (result.FullVersion.Contains("+"))
        {
            var commitHash = result.FullVersion.Split('+')[1];
            result.CommitHash.ShouldContain(commitHash);
        }
    }
} 