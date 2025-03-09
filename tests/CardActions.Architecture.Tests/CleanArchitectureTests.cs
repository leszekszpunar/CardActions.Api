using System.Runtime.CompilerServices;
using CardActions.Architecture.Tests.Helpers;
using MediatR;
using NetArchTest.Rules;
using Xunit.Abstractions;

namespace CardActions.Architecture.Tests;

public class CleanArchitectureTests
{
    private readonly ITestOutputHelper _output;

    public CleanArchitectureTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherLayers()
    {
        // Arrange
        var assembly = NamespaceHelper.GetDomainAssembly();

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                NamespaceHelper.GetApplicationNamespace(),
                NamespaceHelper.GetInfrastructureNamespace(),
                NamespaceHelper.GetInfrastructureDataNamespace(),
                NamespaceHelper.GetApiNamespace())
            .GetResult();

        // Assert
        var failingTypeNames = result.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(result.IsSuccessful,
            $"Domain should not have dependencies on other layers: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void Application_ShouldOnlyHaveDependencyOnDomain()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                NamespaceHelper.GetInfrastructureNamespace(),
                NamespaceHelper.GetInfrastructureDataNamespace(),
                NamespaceHelper.GetApiNamespace())
            .GetResult();

        // Assert
        var failingTypeNames = result.FailingTypeNames ?? Array.Empty<string>();
        Assert.True(result.IsSuccessful,
            $"Application should only have dependency on Domain: {string.Join(", ", failingTypeNames)}");
    }

    [Fact]
    public void Domain_Entities_ShouldHavePrivateSetters()
    {
        // Arrange
        var assembly = NamespaceHelper.GetDomainAssembly();

        // Act
        var entities = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetDomainNamespace()}.Entities")
            .GetTypes();

        var entitiesWithPublicSetters = new List<string>();

        foreach (var entity in entities)
        {
            var properties = entity.GetProperties();
            foreach (var property in properties)
            {
                var setter = property.GetSetMethod();
                if (setter != null && setter.IsPublic && !setter.ReturnParameter.GetRequiredCustomModifiers()
                        .Contains(typeof(IsExternalInit)))
                    entitiesWithPublicSetters.Add($"{entity.Name}.{property.Name}");
            }
        }

        // Assert
        _output.WriteLine(
            $"Znaleziono {entitiesWithPublicSetters.Count} właściwości z publicznymi setterami w encjach:");
        foreach (var entityProperty in entitiesWithPublicSetters) _output.WriteLine($"- {entityProperty}");

        Assert.Empty(entitiesWithPublicSetters);
    }

    [Fact]
    public void Application_Commands_ShouldBeImmutable()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var commands = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .Or()
            .ImplementInterface(typeof(IRequest))
            .And()
            .HaveNameEndingWith("Command")
            .GetTypes();

        var mutableCommands = new List<string>();

        foreach (var command in commands)
        {
            var properties = command.GetProperties();
            foreach (var property in properties)
            {
                var setter = property.GetSetMethod();
                if (setter != null && setter.IsPublic && !setter.ReturnParameter.GetRequiredCustomModifiers()
                        .Contains(typeof(IsExternalInit))) mutableCommands.Add($"{command.Name}.{property.Name}");
            }
        }

        // Assert
        _output.WriteLine($"Znaleziono {mutableCommands.Count} właściwości z publicznymi setterami w komendach:");
        foreach (var commandProperty in mutableCommands) _output.WriteLine($"- {commandProperty}");

        Assert.Empty(mutableCommands);
    }
}