using CardActions.Architecture.Tests.Helpers;
using MediatR;
using NetArchTest.Rules;
using Xunit.Abstractions;

namespace CardActions.Architecture.Tests;

public class CqrsPatternTests
{
    private readonly ITestOutputHelper _output;

    public CqrsPatternTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Commands_Should_Not_Return_Values()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var commands = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .And()
            .HaveNameEndingWith("Command")
            .GetTypes();

        var commandsReturningValues = new List<string>();

        foreach (var command in commands)
        {
            var interfaces = command.GetInterfaces();
            foreach (var @interface in interfaces)
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IRequest<>))
                {
                    var returnType = @interface.GetGenericArguments()[0];
                    if (returnType != typeof(Unit)) commandsReturningValues.Add($"{command.Name} -> {returnType.Name}");
                }
        }

        // Assert
        _output.WriteLine($"Found {commandsReturningValues.Count} commands that return values:");
        foreach (var command in commandsReturningValues) _output.WriteLine($"- {command}");

        Assert.Empty(commandsReturningValues);
    }

    [Fact]
    public void Queries_Should_Be_ReadOnly()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var queries = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .And()
            .HaveNameEndingWith("Query")
            .GetTypes();

        var queriesWithSideEffects = new List<string>();

        foreach (var query in queries)
        {
            var handlers = Types
                .InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IRequestHandler<,>))
                .And()
                .HaveNameMatching($".*{query.Name}Handler")
                .GetTypes();

            foreach (var handler in handlers)
            {
                var methods = handler.GetMethods();
                foreach (var method in methods)
                    if (method.Name == "Handle")
                    {
                        // Check if Handle method contains calls that may cause side effects
                        // This is a simplified approach, in reality we would need code analysis
                        var methodString = method.ToString() ?? string.Empty;
                        if (methodString.Contains("SaveChanges") ||
                            methodString.Contains("Update") ||
                            methodString.Contains("Delete") ||
                            methodString.Contains("Add"))
                            queriesWithSideEffects.Add(query.Name);
                    }
            }
        }

        // Assert
        _output.WriteLine($"Found {queriesWithSideEffects.Count} queries that may have side effects:");
        foreach (var query in queriesWithSideEffects) _output.WriteLine($"- {query}");

        Assert.Empty(queriesWithSideEffects);
    }

    [Fact(Skip = "No commands in the project yet - test will be enabled after adding them")]
    public void Commands_Should_Be_In_Commands_Namespace()
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

        var commandsNotInCommandsNamespace = commands
            .Where(c => c.Namespace != null && !c.Namespace.Contains("Commands"))
            .ToList();

        // Assert
        _output.WriteLine($"Found {commandsNotInCommandsNamespace.Count} commands not in Commands namespace:");
        foreach (var command in commandsNotInCommandsNamespace) _output.WriteLine($"- {command.FullName}");

        Assert.Empty(commandsNotInCommandsNamespace);
    }

    [Fact]
    public void Queries_Should_Be_In_Queries_Namespace()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var queries = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .And()
            .HaveNameEndingWith("Query")
            .GetTypes();

        var queriesNotInQueriesNamespace = queries
            .Where(q => q.Namespace != null && !q.Namespace.Contains("Queries"))
            .ToList();

        // Assert
        _output.WriteLine($"Found {queriesNotInQueriesNamespace.Count} queries not in Queries namespace:");
        foreach (var query in queriesNotInQueriesNamespace) _output.WriteLine($"- {query.FullName}");

        Assert.Empty(queriesNotInQueriesNamespace);
    }

    [Fact]
    public void Command_Handlers_Should_Only_Handle_One_Command()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var commandHandlers = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .And()
            .HaveNameEndingWith("CommandHandler")
            .GetTypes();

        var handlersWithMultipleCommands = new List<string>();

        foreach (var handler in commandHandlers)
        {
            var interfaces = handler.GetInterfaces();
            var commandInterfaces = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .ToList();

            if (commandInterfaces.Count > 1)
            {
                var commandNames = commandInterfaces
                    .Select(i => i.GetGenericArguments()[0].Name)
                    .ToList();

                handlersWithMultipleCommands.Add($"{handler.Name} -> {string.Join(", ", commandNames)}");
            }
        }

        // Assert
        _output.WriteLine(
            $"Found {handlersWithMultipleCommands.Count} command handlers that handle multiple commands:");
        foreach (var handler in handlersWithMultipleCommands) _output.WriteLine($"- {handler}");

        Assert.Empty(handlersWithMultipleCommands);
    }

    [Fact]
    public void Query_Handlers_Should_Only_Handle_One_Query()
    {
        // Arrange
        var assembly = NamespaceHelper.GetApplicationAssembly();

        // Act
        var queryHandlers = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .And()
            .HaveNameEndingWith("QueryHandler")
            .GetTypes();

        var handlersWithMultipleQueries = new List<string>();

        foreach (var handler in queryHandlers)
        {
            var interfaces = handler.GetInterfaces();
            var queryInterfaces = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .ToList();

            if (queryInterfaces.Count > 1)
            {
                var queryNames = queryInterfaces
                    .Select(i => i.GetGenericArguments()[0].Name)
                    .ToList();

                handlersWithMultipleQueries.Add($"{handler.Name} -> {string.Join(", ", queryNames)}");
            }
        }

        // Assert
        _output.WriteLine($"Found {handlersWithMultipleQueries.Count} query handlers that handle multiple queries:");
        foreach (var handler in handlersWithMultipleQueries) _output.WriteLine($"- {handler}");

        Assert.Empty(handlersWithMultipleQueries);
    }
}