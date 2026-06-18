using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.ArchitectureTests;

public sealed class QueryResponseTypeTests
{
    [Fact]
    public void Should_NotReturnDomainTypes_When_ClassImplementsQuery()
    {
        // Arrange
        var applicationAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a =>
                a.GetName().Name is not null &&
                a.GetName().Name!.Contains(".Application"));

        var queryInterface = typeof(IQuery<>);

        // Act
        var violations = new List<string>();

        foreach (var assembly in applicationAssemblies)
        {
            var queryTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.GetInterfaces()
                        .Any(i => i.IsGenericType &&
                                  i.GetGenericTypeDefinition() == queryInterface));

            foreach (var queryType in queryTypes)
            {
                var responseType = queryType
                    .GetInterfaces()
                    .First(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == queryInterface)
                    .GetGenericArguments()[0];

                if (responseType.Namespace != null &&
                    responseType.Namespace.Contains(".Domain"))
                {
                    violations.Add(
                        $"{queryType.FullName} returns domain type {responseType.FullName}");
                }
            }
        }

        // Assert
        Assert.True(
            violations.Count == 0,
            "Queries must return DTO read models and not domain entities. Violations: " +
            string.Join(", ", violations));
    }
}
