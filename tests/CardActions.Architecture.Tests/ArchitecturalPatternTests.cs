using NetArchTest.Rules;
using Xunit;
using Xunit.Abstractions;
using CardActions.Architecture.Tests.Helpers;

namespace CardActions.Architecture.Tests;

public class ArchitecturalPatternTests
{
    private readonly ITestOutputHelper _output;

    public ArchitecturalPatternTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void MediatR_Handlers_ShouldBeInApplicationLayer()
    {
        // Arrange
        var applicationAssembly = NamespaceHelper.GetApplicationAssembly();
        var domainAssembly = NamespaceHelper.GetDomainAssembly();
        var infrastructureAssembly = NamespaceHelper.GetInfrastructureAssembly();
        var apiAssembly = NamespaceHelper.GetApiAssembly();

        // Act & Assert
        // Sprawdzamy, czy handlery MediatR są tylko w warstwie aplikacji
        var applicationResult = Types
            .InAssembly(applicationAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .ResideInNamespace(NamespaceHelper.GetApplicationNamespace())
            .GetResult();

        var failingTypeNames = applicationResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(applicationResult.IsSuccessful, $"MediatR handlers should be in Application layer: {string.Join(", ", failingTypeNames)}");

        // Sprawdzamy, czy w innych warstwach nie ma handlerów MediatR
        var domainResult = Types
            .InAssembly(domainAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .GetTypes();

        Assert.Empty(domainResult);

        var infrastructureResult = Types
            .InAssembly(infrastructureAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .GetTypes();

        Assert.Empty(infrastructureResult);

        var apiResult = Types
            .InAssembly(apiAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .GetTypes();

        Assert.Empty(apiResult);
    }

    [Fact]
    public void Controllers_ShouldNotAccessRepositoriesDirectly()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApiAssembly();

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApiNamespace()}.Controllers")
            .Should()
            .NotHaveDependencyOnAny(
                $"{NamespaceHelper.GetDomainNamespace()}.Repositories",
                $"{NamespaceHelper.GetInfrastructureNamespace()}.Repositories")
            .GetResult();

        // Assert
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"Controllers should not access repositories directly: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void Entities_ShouldBeSealed()
    {
        // Arrange
        var assembly = NamespaceHelper.GetDomainAssembly();

        // Act
        var entities = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetDomainNamespace()}.Entities")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameMatching(".*Record.*")
            .GetTypes();
            
        var nonRecordEntities = entities.Where(e => !e.IsRecord()).ToList();
        var nonSealedEntities = nonRecordEntities.Where(e => !e.IsSealed).ToList();
        
        // Wypisz listę encji, które nie są sealed
        _output.WriteLine($"Znaleziono {nonSealedEntities.Count} encji, które nie są oznaczone jako sealed:");
        foreach (var entity in nonSealedEntities)
        {
            _output.WriteLine($"- {entity.FullName}");
        }

        // Assert
        Assert.Empty(nonSealedEntities);
    }

    [Fact]
    public void DTOs_ShouldBeSealed()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var dtos = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApplicationNamespace()}.DTOs")
            .And()
            .AreClasses()
            .GetTypes();
            
        var nonSealedDtos = dtos.Where(d => !d.IsSealed).ToList();
        
        // Wypisz listę DTOs, które nie są sealed
        _output.WriteLine($"Znaleziono {nonSealedDtos.Count} DTOs, które nie są oznaczone jako sealed:");
        foreach (var dto in nonSealedDtos)
        {
            _output.WriteLine($"- {dto.FullName}");
        }

        // Assert
        Assert.Empty(nonSealedDtos);
    }

    [Fact]
    public void Controllers_ShouldUseMediatR()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApiAssembly();

        // Act
        var controllers = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApiNamespace()}.Controllers")
            .And()
            .AreClasses()
            .GetTypes();
            
        var controllersWithoutMediatR = controllers
            .Where(c => !c.GetConstructors()
                .Any(ctor => ctor.GetParameters()
                    .Any(p => p.ParameterType.FullName != null && p.ParameterType.FullName.Contains("MediatR"))))
            .ToList();
        
        // Wypisz listę kontrolerów, które nie używają MediatR
        _output.WriteLine($"Znaleziono {controllersWithoutMediatR.Count} kontrolerów, które nie używają MediatR:");
        foreach (var controller in controllersWithoutMediatR)
        {
            _output.WriteLine($"- {controller.FullName}");
        }

        // Assert
        Assert.Empty(controllersWithoutMediatR);
    }

    [Fact]
    public void Services_ShouldNotAccessDbContextDirectly()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApplicationNamespace()}.Services")
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"Application services should not access DbContext directly: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnEntityFramework()
    {
        // Arrange
        var assembly = NamespaceHelper.GetDomainAssembly();

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"Domain layer should not have dependency on Entity Framework: {string.Join(", ", failingTypeNames)}");
    }
} 