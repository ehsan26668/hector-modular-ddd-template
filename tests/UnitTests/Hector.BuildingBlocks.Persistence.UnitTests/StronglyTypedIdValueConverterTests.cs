using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Converters;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class StronglyTypedIdValueConverterTests
{
    [Fact]
    public void Should_ConvertStronglyTypedIdToGuid()
    {
        // Arrange
        var converter = new StronglyTypedIdValueConverter<TestId>();
        var convert = converter.ConvertToProviderExpression.Compile();

        var id = TestId.New();

        // Act
        var guid = convert(id);

        // Assert
        guid.Should().Be(id.Value);
    }

    [Fact]
    public void Should_ConvertGuidToStronglyTypedId()
    {
        // Arrange
        var converter = new StronglyTypedIdValueConverter<TestId>();
        var convert = converter.ConvertFromProviderExpression.Compile();

        var guid = Guid.NewGuid();

        // Act
        var id = convert(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Should_ReuseCachedFactory_When_CreatingMultipleInstances()
    {
        // Arrange
        var converter = new StronglyTypedIdValueConverter<TestId>();
        var convert = converter.ConvertFromProviderExpression.Compile();

        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        // Act
        var id1 = convert(guid1);
        var id2 = convert(guid2);

        // Assert
        id1.Value.Should().Be(guid1);
        id2.Value.Should().Be(guid2);
    }

    private sealed class TestId : StronglyTypedId<TestId>
    {
        private TestId(Guid value) : base(value) { }

        public static TestId New()
            => CreateNew(static v => new TestId(v));
    }
}
