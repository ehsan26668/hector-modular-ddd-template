using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Xunit;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class StronglyTypedIdCrtpTests
{
    [Fact]
    public void Should_CreateNewIdentifier_When_NewIsCalled()
    {
        // Arrange


        // Act
        var id = TestId.New();

        // Assert
        id.Should().NotBeNull();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Should_ReturnEmptyIdentifier_When_EmptyIsAccessed()
    {
        // Arrange


        // Act
        var id = TestId.Empty();

        // Assert
        id.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Should_CreateIdentifier_When_ParsingValidString()
    {
        // Arrange
        var guid = Guid.CreateVersion7();

        // Act
        var id = TestId.Parse(guid.ToString());

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Should_ReturnTrue_When_TryParseReceivesValidValue()
    {
        // Arrange
        var guid = Guid.CreateVersion7();

        // Act
        var result = TestId.TryParse(guid.ToString(), out var id);

        // Assert
        result.Should().BeTrue();
        id.Should().NotBeNull();
        id!.Value.Should().Be(guid);
    }

    [Fact]
    public void Should_ReturnFalse_When_TryParseReceivesInvalidValue()
    {
        // Arrange
        const string invalidValue = "invalid-guid";

        // Act
        var result = TestId.TryParse(invalidValue, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }
}

public sealed class TestId
    : StronglyTypedIdCrtp<TestId>, IStronglyTypedId<TestId>
{
    private TestId(Guid value) : base(value) { }

    public static TestId Create(Guid value) => new(value);

    public static TestId CreateEmpty() => new(Guid.Empty);
}