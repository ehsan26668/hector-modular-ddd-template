using FluentAssertions;
using Hector.ArchitectureTests.Common;
using Hector.BuildingBlocks.Domain.Primitives;
using Xunit;

namespace Hector.ArchitectureTests.DomainRules;

[Trait("Category", "ArchitectureTests")]
public sealed class StronglyTypedIdTests : ArchitectureTestBase
{
    [Fact(DisplayName = "ADR-0036 | TC-05 | Should inherit from StronglyTypedId")]
    public void Should_InheritFromStronglyTypedId_When_DeclaringDomainIdentifiers()
    {
        // Arrange
        var domainAssemblies = DomainAssemblies;

        // Act
        var idTypes = domainAssemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                        t.IsValueType &&
                        !t.IsEnum)
            .ToList();

        var invalidIds = idTypes.Where(t =>
            t.BaseType == null ||
            !t.BaseType.IsGenericType ||
            t.BaseType.GetGenericTypeDefinition() != typeof(StronglyTypedId<>)).ToList();

        // Assert
        invalidIds.Should().BeEmpty("All domain identifier types ending with 'Id' must inherit from StronglyTypedId<>.");
    }
}
