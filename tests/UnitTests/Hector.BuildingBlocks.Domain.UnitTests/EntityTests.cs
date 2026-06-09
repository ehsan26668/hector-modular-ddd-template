using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class EntityTests
{
    [Fact]
    public void Should_BeEqual_When_EntitiesHaveSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "First");
        var entity2 = new TestEntity(id, "Second");

        // Act&Assert
        (entity1 == entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeTrue();
    }

    [Fact]
    public void Should_NotBeEqual_When_EntitiesHaveDifferentIds()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid(), "First");
        var entity2 = new TestEntity(Guid.NewGuid(), "Second");

        // Act & Assert
        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Should_NotBeEqual_When_EntitiesHaveSameIdButDifferentTypes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "First");
        var entity2 = new AnotherTestEntity(id);

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnSameHashCode_When_EntitiesHaveSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "First");
        var entity2 = new TestEntity(id, "Second");

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Should_NotBeEqual_When_ComperedWithNull()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid(), "Test");

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
    }

    internal class TestEntity(Guid id, string name) : Entity<Guid>(id)
    {
        public string Name { get; } = name;
    }

    internal class AnotherTestEntity(Guid id) : Entity<Guid>(id) { }
}