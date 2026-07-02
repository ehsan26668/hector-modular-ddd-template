using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class CommandHandlerReturnTypeTests
{
    [Fact]
    public void Should_ReturnTaskOfResultOrResultOfT_When_ImplementingCommandHandler()
    {
        // Arrange
        var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            .ToList();

        var invalidHandlers = new List<Type>();

        // Act
        foreach (var handler in handlerTypes)
        {
            var method = handler.GetMethod("Handle");

            if (method is null || !IsValidReturnType(method.ReturnType))
            {
                invalidHandlers.Add(handler);
            }
        }

        // Assert
        invalidHandlers.Should().BeEmpty(
            $"CommandHandlers must return Task<Result> or Task<Result<T>>. Violations: {string.Join(", ", invalidHandlers.Select(x => x.FullName))}");
    }

    private static bool IsValidReturnType(Type returnType)
    {
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            return false;
        }

        var innerType = returnType.GetGenericArguments()[0];

        return innerType == typeof(Result)
               || (innerType.IsGenericType &&
                   innerType.GetGenericTypeDefinition() == typeof(Result<>));
    }
}
