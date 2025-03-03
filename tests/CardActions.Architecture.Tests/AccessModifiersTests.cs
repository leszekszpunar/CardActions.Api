using NetArchTest.Rules;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using CardActions.Architecture.Tests.Helpers;

namespace CardActions.Architecture.Tests;

public class AccessModifiersTests
{
    private readonly ITestOutputHelper _output;

    public AccessModifiersTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Domain_ValueObjects_Should_Be_ReadOnly()
    {
        // Arrange
        var assembly = NamespaceHelper.GetDomainAssembly();

        // Act
        var valueObjects = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetDomainNamespace()}.ValueObjects")
            .And()
            .AreClasses()
            .GetTypes();

        var nonReadOnlyValueObjects = valueObjects
            .Where(vo => !IsReadOnly(vo))
            .ToList();

        // Assert
        _output.WriteLine($"Found {nonReadOnlyValueObjects.Count} non-readonly value objects:");
        foreach (var vo in nonReadOnlyValueObjects)
        {
            _output.WriteLine($"- {vo.FullName}");
        }

        Assert.Empty(nonReadOnlyValueObjects);
    }

    [Fact]
    public void Infrastructure_Implementation_Classes_Should_Be_Internal()
    {
        // Arrange
        var assembly = NamespaceHelper.GetInfrastructureAssembly();

        // Act
        var implementationClasses = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{NamespaceHelper.GetInfrastructureNamespace()}.Services")
            .Or()
            .ResideInNamespace($"{NamespaceHelper.GetInfrastructureNamespace()}.Repositories")
            .And()
            .DoNotHaveNameEndingWith("Factory")
            .And()
            .AreClasses()
            .GetTypes();

        var publicImplementationClasses = implementationClasses
            .Where(c => c.IsPublic && !c.IsAbstract && !c.IsInterface)
            .ToList();

        // Assert
        _output.WriteLine($"Found {publicImplementationClasses.Count} public implementation classes that should be internal:");
        foreach (var cls in publicImplementationClasses)
        {
            _output.WriteLine($"- {cls.FullName}");
        }

        Assert.Empty(publicImplementationClasses);
    }

    [Fact]
    public void Application_Command_Handlers_Should_Be_Internal()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var commandHandlers = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        var publicCommandHandlers = commandHandlers
            .Where(h => h.IsPublic)
            .ToList();

        // Assert
        _output.WriteLine($"Found {publicCommandHandlers.Count} public command handlers that should be internal:");
        foreach (var handler in publicCommandHandlers)
        {
            _output.WriteLine($"- {handler.FullName}");
        }

        Assert.Empty(publicCommandHandlers);
    }

    [Fact]
    public void Domain_Interfaces_Should_Be_Public()
    {
        // Arrange
        var assembly = NamespaceHelper.GetDomainAssembly();

        // Act
        var interfaces = Types
            .InAssembly(assembly)
            .That()
            .AreInterfaces()
            .GetTypes();

        var nonPublicInterfaces = interfaces
            .Where(i => !i.IsPublic)
            .ToList();

        // Assert
        _output.WriteLine($"Found {nonPublicInterfaces.Count} non-public interfaces:");
        foreach (var iface in nonPublicInterfaces)
        {
            _output.WriteLine($"- {iface.FullName}");
        }

        Assert.Empty(nonPublicInterfaces);
    }

    private bool IsReadOnly(Type type)
    {
        // Check if all fields are readonly
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var allFieldsReadOnly = fields.All(f => f.IsInitOnly);

        // Check if all properties have only getters or init-only setters
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var allPropertiesReadOnly = properties.All(p => {
            var setMethod = p.GetSetMethod();
            if (setMethod == null) return true; // Only getter, so it's readonly
            
            // Check if it's an init-only setter
            var modifiers = setMethod.ReturnParameter.GetRequiredCustomModifiers();
            return modifiers.Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
        });

        return allFieldsReadOnly && allPropertiesReadOnly;
    }
} 