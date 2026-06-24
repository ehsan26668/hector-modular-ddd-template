using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class QueryHandlerReturnTypeTests
{
    [Fact]
    public void QueryHandlers_Should_Return_TaskOfResultOfT()
    {
        // Arrange
        var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            .ToList();

        var invalidHandlers = new List<Type>();

        // Act
        foreach (var handler in handlerTypes)
        {
            var method = handler.GetMethod("Handle");

            if (method is null)
            {
                invalidHandlers.Add(handler);
                continue;
            }

            var returnType = method.ReturnType;

            if (!returnType.IsGenericType ||
                returnType.GetGenericTypeDefinition() != typeof(Task<>))
            {
                invalidHandlers.Add(handler);
                continue;
            }

            var innerType = returnType.GetGenericArguments()[0];

            if (!innerType.IsGenericType ||
                innerType.GetGenericTypeDefinition() != typeof(Result<>))
            {
                invalidHandlers.Add(handler);
            }
        }

        // Assert
        invalidHandlers.Should().BeEmpty(
            $"QueryHandlers must return Task<Result<T>>. Violations: {string.Join(", ", invalidHandlers.Select(x => x.Name))}");
    }
}
