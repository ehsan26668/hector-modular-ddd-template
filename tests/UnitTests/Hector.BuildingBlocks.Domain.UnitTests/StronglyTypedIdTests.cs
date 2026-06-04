using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void StronglyTypedId_Should_Expose_Value()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var id = new TestId(value);

        // Assert
        id.Value.Should().Be(value);
    }

    [Fact]
    public void StronglyTypedId_Should_Be_Equal_When_Values_Are_Equal()
    {
        // Arrange
        var value = Guid.NewGuid();

        var id1 = new TestId(value);
        var id2 = new TestId(value);


        // Assert
        id1.Should().Be(id2);
    }

    [Fact]
    public void StronglyTypedId_Should_Not_Be_Equal_When_Values_Are_Different()
    {
        // Arrange
        var id1 = new TestId(Guid.NewGuid());
        var id2 = new TestId(Guid.NewGuid());

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void StronglyTypedId_Should_Throw_When_Value_Is_Default()
    {
        // Act
        var act = () => new TestId(Guid.Empty);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Strongly typed id value cannot be default.");
    }

    [Fact]
    public void Different_StronglyTypedId_Types_Should_Not_Be_Equal_Even_When_Values_Are_Equal()
    {
        // Arrange
        var value = Guid.NewGuid();

        var testId = new TestId(value);
        var anotherTestId = new AnotherTestId(value);

        // Assert
        testId.Equals(anotherTestId).Should().BeFalse();
    }

    private sealed class TestId : StronglyTypedId<Guid>
    {
        public TestId(Guid value)
            : base(value)
        {
        }
    }

    private sealed class AnotherTestId : StronglyTypedId<Guid>
    {
        public AnotherTestId(Guid value)
            : base(value)
        {
        }
    }
}