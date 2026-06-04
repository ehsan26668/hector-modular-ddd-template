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

    [Fact]
    public void StronglyTypedId_Should_Implicitly_Convert_To_Value()
    {
        // Arrange
        var value = Guid.NewGuid();
        var id = new TestId(value);

        // Act
        Guid convertedValue = id;

        // Assert
        convertedValue.Should().Be(value);
    }

    [Fact]
    public void GuidBaseId_Should_Work_Correctly()
    {
        // Act
        var id = new GuidBaseId(Guid.NewGuid());

        // Assert
        id.Should().BeAssignableTo<StronglyTypedId<Guid>>();
    }

    // ===============================
    // ADR-0010 Advanced Capabilities
    // ===============================

    [Fact]
    public void StronglyTypedId_New_Should_Create_New_Id_With_Uuid7()
    {
        // Arrange & Act
        var id = AdvancedTestId.New();

        // Assert
        id.Value.Should().NotBe(Guid.Empty);

        var version = (id.Value.ToByteArray()[7] >> 4) & 0x0F;
        version.Should().Be(7);
    }

    [Fact]
    public void StronglyTypedId_Empty_Should_Return_Empty_Id()
    {
        // Arrange & Act
        var id = AdvancedTestId.Empty;

        // Assert
        id.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void StronglyTypedId_Parse_Should_Create_Id_From_String()
    {
        // Arrange
        var guid = Guid.CreateVersion7();
        var text = guid.ToString();

        // Act
        var id = AdvancedTestId.Parse(text);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void StronglyTypedId_TryParse_Should_Return_True_For_Valid_Value()
    {
        // Arrange
        var guid = Guid.CreateVersion7().ToString();

        // Act
        var result = AdvancedTestId.TryParse(guid, out var id);

        // Assert
        result.Should().BeTrue();
        id.Should().NotBeNull();
        id!.Value.ToString().Should().Be(guid);
    }

    [Fact]
    public void StronglyTypedId_TryParse_Should_Return_False_For_Invalid_Value()
    {
        // Arrange & Act
        var result = AdvancedTestId.TryParse("invalid-guid", out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }

    // ===============================
    // Test Helper Types
    // ===============================
    private sealed class TestId : StronglyTypedId<Guid>
    {
        public TestId(Guid value) : base(value) { }
    }

    private sealed class AnotherTestId : StronglyTypedId<Guid>
    {
        public AnotherTestId(Guid value) : base(value) { }
    }

    private sealed class GuidBaseId : StronglyTypedId
    {
        public GuidBaseId(Guid value) : base(value){} 
    }

    private sealed class AdvancedTestId : StronglyTypedId
    {
        private AdvancedTestId(Guid value) : base(value) { }

        private AdvancedTestId(Guid value, bool isEmpty) : base(value, isEmpty) { }

        public static AdvancedTestId New()
            => new(Guid.CreateVersion7());

        public static AdvancedTestId Empty
            => new(Guid.Empty, true);

        public static AdvancedTestId Parse(string value)
            => new(Guid.Parse(value));

        public static bool TryParse(string value, out AdvancedTestId? id)
        {
            if (Guid.TryParse(value, out var guid))
            {
                id = new AdvancedTestId(guid);
                return true;
            }

            id = null;
            return false;
        }
    }
}