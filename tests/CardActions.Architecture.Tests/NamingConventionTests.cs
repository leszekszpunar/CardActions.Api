using System.Reflection;
using NetArchTest.Rules;
using Xunit;
using CardActions.Architecture.Tests.Helpers;

namespace CardActions.Architecture.Tests;

public class NamingConventionTests
{
    private readonly Assembly _domainAssembly;
    private readonly Assembly _applicationAssembly;
    private readonly Assembly _infrastructureAssembly;
    private readonly Assembly _infrastructureDataAssembly;
    private readonly Assembly _apiAssembly;

    public NamingConventionTests()
    {
        _domainAssembly = NamespaceHelper.GetDomainAssembly();
        _applicationAssembly = NamespaceHelper.GetApplicationAssembly();
        _infrastructureAssembly = NamespaceHelper.GetInfrastructureAssembly();
        _infrastructureDataAssembly = NamespaceHelper.GetInfrastructureDataAssembly();
        _apiAssembly = NamespaceHelper.GetApiAssembly();
    }

    [Fact]
    public void Entities_Should_Have_Proper_Naming()
    {
        var testResult = Types
            .InAssembly(_domainAssembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetDomainNamespace()}.Entities")
            .And()
            .AreClasses()
            .Should()
            .NotHaveNameEndingWith("Entity")
            .GetResult();

        AssertNamingConvention(testResult, "Entities should not have 'Entity' suffix");
    }

    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        foreach (var assembly in new[] { _domainAssembly, _applicationAssembly, _infrastructureAssembly, _infrastructureDataAssembly })
        {
            var testResult = Types
                .InAssembly(assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            AssertNamingConvention(testResult, "Interfaces should start with 'I'");
        }
    }

    [Fact]
    public void DTOs_Should_Have_Dto_Suffix()
    {
        var testResult = Types
            .InAssembly(_applicationAssembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApplicationNamespace()}.DTOs")
            .Should()
            .HaveNameEndingWith("Dto")
            .GetResult();

        AssertNamingConvention(testResult, "DTOs should have 'Dto' suffix");
    }

    [Fact]
    public void Services_Should_Have_Service_Suffix()
    {
        var testResult = Types
            .InAssembly(_applicationAssembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApplicationNamespace()}.Services")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Service")
            .GetResult();

        AssertNamingConvention(testResult, "Services should have 'Service' suffix");
    }

    [Fact]
    public void Repositories_Should_Have_Repository_Suffix()
    {
        foreach (var assembly in new[] { _infrastructureAssembly, _infrastructureDataAssembly })
        {
            var testResult = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Repositories")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            AssertNamingConvention(testResult, "Repositories should have 'Repository' suffix");
        }
    }

    [Fact]
    public void Repository_Interfaces_Should_Have_Repository_Suffix()
    {
        var testResult = Types
            .InAssembly(_domainAssembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetDomainNamespace()}.Repositories")
            .And()
            .AreInterfaces()
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        AssertNamingConvention(testResult, "Repository interfaces should have 'Repository' suffix");
    }

    [Fact]
    public void Controllers_Should_Have_Controller_Suffix()
    {
        var testResult = Types
            .InAssembly(_apiAssembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetApiNamespace()}.Controllers")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        AssertNamingConvention(testResult, "Controllers should have 'Controller' suffix");
    }

    private static void AssertNamingConvention(NetArchTest.Rules.TestResult testResult, string message)
    {
        var failingTypeNames = testResult.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(testResult.IsSuccessful, $"{message}: {string.Join(", ", failingTypeNames)}");
    }
} 