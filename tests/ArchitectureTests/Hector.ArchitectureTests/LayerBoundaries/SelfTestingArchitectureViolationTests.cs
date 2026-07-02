using FluentAssertions;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.LayerBoundaries;

[Trait("Category", "ArchitectureTests")]
public sealed class SelfTestingArchitectureViolationTests
{
    [Fact(DisplayName = "ADR-0036 | TC-10 | Should fail architecture tests when rules are violated")]
    public void Should_FailArchitectureTests_When_RulesAreViolated()
    {
        // Arrange
        var types = Types.InAssembly(typeof(SelfTestingArchitectureViolationTests).Assembly);

        // Act
        var result = types
            .That()
            .HaveName(nameof(SelfTestingArchitectureViolationTests))
            .ShouldNot()
            .HaveDependencyOn("Xunit")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeFalse("The architecture guard system should correctly fail and report violations when rules are violated.");
        result.FailingTypes.Should().NotBeNull();
    }
}