using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void Should_ExposeValue_When_IdIsCreatedFromExistingGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = TestId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Should_BeEqual_When_UnderlyingValuesAreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var first = TestId.From(guid);
        var second = TestId.From(guid);

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Should_NotBeEqual_When_UnderlyingValuesAreDifferent()
    {
        // Arrange
        var first = TestId.New();
        var second = TestId.New();

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Should_ThrowException_When_CreatingFromEmptyGuid()
    {
        // Arrange
        var empty = Guid.Empty;

        // Act
        Action act = () => TestId.From(empty);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Should_NotBeEqual_When_StronglyTypedIdTypesAreDifferent()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var first = TestId.From(guid);
        var second = AnotherTestId.From(guid);

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Should_ImplicitlyConvertToGuid_When_CastingId()
    {
        // Arrange
        var id = TestId.New();

        // Act
        Guid converted = id;

        // Assert
        converted.Should().Be(id.Value);
    }

    [Fact]
    public void Should_GenerateVersion7Guid_When_NewIdIsCreated()
    {
        // Arrange


        // Act
        var id = TestId.New();

        // Assert
        var version = (id.Value.ToByteArray()[7] >> 4) & 0x0f;
        version.Should().Be(7);
    }

    [Fact]
    public void Should_ParseStringIntoStronglyTypeId_When_GuidIsValid()
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
    public void Should_ReturnTrue_When_TryParseReceivesValidGuid()
    {
        // Arrange
        var text = Guid.CreateVersion7().ToString();

        // Act
        var success = AdvancedTestId.TryParse(text, out var id);

        // Assert
        success.Should().BeTrue();
        id.Should().NotBeNull();
        id!.Value.ToString().Should().Be(text);
    }

    [Fact]
    public void Should_ReturnFalse_When_TryParseReceivesInvalidGuid()
    {
        // Arrange
        var invalid = "invalid-guid";

        // Act
        var success = AdvancedTestId.TryParse(invalid, out var id);

        // Assert
        success.Should().BeFalse();
        id.Should().BeNull();
    }

    private sealed class TestId : StronglyTypedId<TestId>
    {
        private TestId(Guid value) : base(value) { }

        public static TestId New()
            => CreateNew(static v => new TestId(v));

        internal static TestId From(Guid value)
            => FromExisting(value, static v => new TestId(v));
    }

    private sealed class AnotherTestId : StronglyTypedId<AnotherTestId>
    {
        private AnotherTestId(Guid value) : base(value) { }

        internal static AnotherTestId From(Guid value)
            => FromExisting(value, static v => new AnotherTestId(v));
    }

    private sealed class AdvancedTestId : StronglyTypedId<AdvancedTestId>
    {
        private AdvancedTestId(Guid value) : base(value) { }

        public static AdvancedTestId New()
            => CreateNew(static v => new AdvancedTestId(v));

        public static AdvancedTestId Parse(string value)
            => FromExisting(Guid.Parse(value), static v => new AdvancedTestId(v));

        public static bool TryParse(string value, out AdvancedTestId? id)
        {
            if (Guid.TryParse(value, out var guid))
            {
                id = FromExisting(guid, static v => new AdvancedTestId(v));
                return true;
            }

            id = null;
            return false;
        }
    }
}