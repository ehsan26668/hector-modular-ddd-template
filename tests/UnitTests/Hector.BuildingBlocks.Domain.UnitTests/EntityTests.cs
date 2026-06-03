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
    }

    internal class TestEntity : Entity<Guid>
    {
        public string Name { get; }
        public TestEntity(Guid id, string name) : base(id) => Name = name;
    }
}