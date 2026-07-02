using Hector.ArchitectureTests.Common;

namespace Hector.ArchitectureTests.DomainRules;

[Trait("Category", "ArchitectureTests")]
public sealed class DomainIdentityTests : ArchitectureTestBase
{
    [Fact(DisplayName = "ADR-0036 | TC-06 | Should not use Guid.NewGuid() for domain identities")]
    public void Should_NotUseGuidNewGuid_When_ImplementingStronglyTypedIds()
    {
        // Arrange, Act & Assert
        ArchitectureAssertions.ShouldNotUseGuidNewGuidForStronglyTypedIds(DomainAssemblies);
    }

    [Fact(DisplayName = "ADR-0036 | TC-08 | Should keep infrastructure concerns out of domain layer")]
    public void Should_NotDependOnInfrastructureConcerns_When_InDomainLayer()
    {
        // Arrange
        string[] forbidden = ["Microsoft.EntityFrameworkCore", "System.Data", "Microsoft.AspNetCore"];

        // Act & Assert
        ArchitectureAssertions.ShouldNotDependOnAny(
            DomainAssemblies,
            forbidden,
            because: "Infrastructure concerns and database/web types must not leak into Domain layer.");
    }
}
