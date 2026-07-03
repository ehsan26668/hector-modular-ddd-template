using FluentAssertions;

namespace Hector.ArchitectureTests.Framework.Unit;

[Trait("Category", "ArchitectureTests")]
public sealed class FluentSyntaxTests
{
    [Fact(DisplayName = "ADR-0056 | TC-01 | Should start fluent chain with types selection")]
    public void Should_StartFluentChain_When_TypesIsCalled()
    {
        // Arrange & Act
        var builder = ArchitectureRule.Types();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact(DisplayName = "ADR-0056 | TC-01 | Should allow filtering types using That()")]
    public void Should_AllowFiltering_When_ThatIsCalled()
    {
        // Arrange
        var builder = ArchitectureRule.Types();

        // Act
        var filterBuilder = builder.That().ResideInNamespace("Hector.Domain");

        // Assert
        // Explicitly use FluentAssertions to avoid conflict with DSL's .Should()
        AssertionExtensions.Should(filterBuilder).NotBeNull();
    }

    [Fact(DisplayName = "ADR-0056 | TC-01 | Should allow defining constraints using Should()")]
    public void Should_AllowConstraints_When_ShouldIsCalled()
    {
        // Arrange
        var builder = ArchitectureRule.Types()
            .That()
            .ResideInNamespace("Hector.Domain");

        // Act
        var constraintBuilder = builder.Should().NotDependOn("Hector.Infrastructure");

        // Assert
        // Explicitly use FluentAssertions to avoid conflict with DSL's .Should()
        AssertionExtensions.Should(constraintBuilder).NotBeNull();
    }
}
