using Hector.ArchitectureTests.Common;

namespace Hector.ArchitectureTests.CodingConventions;

[Trait("Category", "ArchitectureTests")]
public sealed class ArchitectureTestNamingTests : ArchitectureTestBase
{
    [Fact(DisplayName = "ADR-0036 | TC-07 | Should follow architecture test naming convention")]
    public void Should_FollowTestMethodNamingConvention_When_DefiningTests()
    {
        // Arrange
        var testMethods = GetTestMethods(TestAssemblies);

        // Act & Assert
        ArchitectureAssertions.ShouldFollowTestMethodNamingConvention(testMethods);
    }
}
