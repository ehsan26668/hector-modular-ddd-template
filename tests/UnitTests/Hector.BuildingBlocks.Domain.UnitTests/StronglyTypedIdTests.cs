using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void StronglyTypedId_Value_Should_Be_Exposed()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = TestId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void StronglyTypedId_Should_Be_Equal_When_Values_Are_Equal()
    {
        var guid = Guid.NewGuid();

        var id1 = TestId.From(guid);
        var id2 = TestId.From(guid);

        id1.Should().Be(id2);
    }

    [Fact]
    public void StronglyTypedId_Should_Not_Be_Equal_When_Values_Are_Different()
    {
        var id1 = TestId.New();
        var id2 = TestId.New();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void StronglyTypedId_Should_Throw_When_Creating_From_Empty()
    {
        Action act = () => TestId.From(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Different_StronglyTypedId_Types_Should_Not_Be_Equal()
    {
        var guid = Guid.NewGuid();

        var a = TestId.From(guid);
        var b = AnotherTestId.From(guid);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void StronglyTypedId_Should_Implicitly_Convert_To_Guid()
    {
        var id = TestId.New();

        Guid guid = id;

        guid.Should().Be(id.Value);
    }

    // =======================
    // Guid v7 Tests
    // =======================
    [Fact]
    public void StronglyTypedId_New_Should_Generate_Version7()
    {
        var id = TestId.New();

        var version = (id.Value.ToByteArray()[7] >> 4) & 0x0F;

        version.Should().Be(7);
    }

    // =======================
    // Parse & TryParse
    // =======================
    [Fact]
    public void StronglyTypedId_Parse_Should_Create_Id()
    {
        var guid = Guid.CreateVersion7();
        var text = guid.ToString();

        var id = AdvancedTestId.Parse(text);

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void StronglyTypedId_TryParse_Should_Return_True_For_Valid()
    {
        var str = Guid.CreateVersion7().ToString();

        var result = AdvancedTestId.TryParse(str, out var id);

        result.Should().BeTrue();
        id.Should().NotBeNull();
        id!.Value.ToString().Should().Be(str);
    }

    [Fact]
    public void StronglyTypedId_TryParse_Should_Return_False_For_Invalid()
    {
        var result = AdvancedTestId.TryParse("invalid-guid", out var id);

        result.Should().BeFalse();
        id.Should().BeNull();
    }

    // =======================
    // Test Helper Types
    // =======================
    private sealed class TestId : StronglyTypedId<TestId>
    {
        private TestId(Guid value) : base(value) { }

        public static TestId New()
            => CreateNew(v => new TestId(v));

        internal static TestId From(Guid value)
            => FromExisting(value, v => new TestId(v));
    }

    private sealed class AnotherTestId : StronglyTypedId<AnotherTestId>
    {
        private AnotherTestId(Guid value) : base(value) { }

        internal static AnotherTestId From(Guid value)
            => FromExisting(value, v => new AnotherTestId(v));
    }

    private sealed class AdvancedTestId : StronglyTypedId<AdvancedTestId>
    {
        private AdvancedTestId(Guid value) : base(value) { }

        public static AdvancedTestId New()
            => CreateNew(v => new AdvancedTestId(v));

        public static AdvancedTestId Parse(string s)
            => FromExisting(Guid.Parse(s), v => new AdvancedTestId(v));

        public static bool TryParse(string s, out AdvancedTestId? id)
        {
            if (Guid.TryParse(s, out var g))
            {
                id = FromExisting(g, v => new AdvancedTestId(v));
                return true;
            }

            id = null;
            return false;
        }
    }
}
