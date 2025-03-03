using NetArchTest.Rules;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using CardActions.Architecture.Tests.Helpers;

namespace CardActions.Architecture.Tests;

public class DependencyTests
{
    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        // Arrange
        var assembly = NamespaceHelper.GetDomainAssembly();

        var otherProjects = new[]
        {
            NamespaceHelper.GetApplicationNamespace(),
            NamespaceHelper.GetInfrastructureNamespace(),
            NamespaceHelper.GetInfrastructureDataNamespace(),
            NamespaceHelper.GetApiNamespace()
        };

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        // Assert
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"Domain layer should not have dependencies on other projects: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void Application_ShouldOnlyHaveDependencyOnDomain()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        var forbiddenDependencies = new[]
        {
            NamespaceHelper.GetInfrastructureNamespace(),
            NamespaceHelper.GetInfrastructureDataNamespace(),
            NamespaceHelper.GetApiNamespace()
        };

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        // Assert
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"Application layer should only depend on Domain layer: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnApi()
    {
        // Arrange
        var assembly = NamespaceHelper.GetInfrastructureAssembly();

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(NamespaceHelper.GetApiNamespace())
            .GetResult();

        // Assert
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"Infrastructure layer should not have dependencies on API layer: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void InfrastructureData_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        // Arrange
        var dataAssembly = NamespaceHelper.GetInfrastructureDataAssembly();
        
        // Sprawdź faktyczne referencje projektu
        var referencedAssemblies = dataAssembly.GetReferencedAssemblies();
        
        // Act & Assert
        // Sprawdź, czy nie ma bezpośredniej referencji do CardActions.Infrastructure
        var hasInfrastructureDependency = referencedAssemblies
            .Any(a => a.Name == "CardActions.Infrastructure");
        
        // Sprawdź, czy nie ma bezpośredniej referencji do CardActions.Api
        var hasApiDependency = referencedAssemblies
            .Any(a => a.Name == "CardActions.Api");
        
        Assert.False(hasInfrastructureDependency, "Infrastructure.Data nie powinno mieć bezpośredniej zależności od Infrastructure");
        Assert.False(hasApiDependency, "Infrastructure.Data nie powinno mieć bezpośredniej zależności od Api");
    }

    [Fact]
    public void Controllers_ShouldHaveControllerSuffix()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApiAssembly();

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApiNamespace()}.Controllers")
            .And()
            .ArePublic()
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"Controllers should have 'Controller' suffix: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void Services_ShouldHaveInterfaceImplementation()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var services = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApplicationNamespace()}.Services")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        var failingTypes = services
            .Where(type => !type.GetInterfaces().Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Service")))
            .ToList();

        // Assert
        Assert.Empty(failingTypes);
    }

    [Fact]
    public void Repositories_ShouldImplementRepositoryInterfaces()
    {
        // Arrange
        var assembly = NamespaceHelper.GetInfrastructureAssembly();

        // Act
        var repositories = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetInfrastructureNamespace()}.Repositories")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        var failingTypes = repositories
            .Where(type => !type.GetInterfaces().Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Repository")))
            .ToList();

        // Assert
        Assert.Empty(failingTypes);
    }
} 