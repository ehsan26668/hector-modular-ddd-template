using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Domain.UnitTests;

public sealed class ProjectTests
{
    [Fact]
    public void Should_CreateProject_When_NameIsValid()
    {
        // Arrange
        var name = "DDD Project";

        // Act
        var project = Project.Create(name);

        // Assert
        project.Should().NotBeNull();
        project.Name.Should().Be(name);
        project.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Should_RecordDomainEvent_When_ProjectIsCreated()
    {
        // Arrange
        var name = "My Project";

        // Act
        var project = Project.Create(name);

        // Assert
        IHasDomainEvents aggregate = project;
        var events = aggregate.GetDomainEvents();

        events.Single()
            .Should()
            .BeOfType<ProjectCreatedDomainEvent>();
    }

    [Fact]
    public void Should_RecordCorrectEventData_When_ProjectIsCreated()
    {
        // Arrange
        var name = "Hector";

        // Act
        var project = Project.Create(name);

        // Extract event
        IHasDomainEvents aggregate = project;
        var domainEvent = aggregate.GetDomainEvents()
            .Single()
            .As<ProjectCreatedDomainEvent>();

        // Assert
        domainEvent.ProjectId.Should().Be(project.Id);
        domainEvent.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_ThrowDomainException_When_NameIsInvalid(string? invalidName)
    {
        // Act
        var act = () => Project.Create(invalidName!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_ClearDomainEvents_When_EventsDispatched()
    {
        // Arrange
        var project = Project.Create("DDD");

        IHasDomainEvents aggregate = project;

        aggregate.GetDomainEvents().Should().HaveCount(1);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void Should_NotRaiseAdditionalEvents_When_NoStateChangeOccurs()
    {
        // Arrange
        var project = Project.Create("DDD");

        IHasDomainEvents aggregate = project;

        // Act
        var events = aggregate.GetDomainEvents();

        // Assert
        events.Should().HaveCount(1);
    }

    [Fact]
    public void Should_FollowStronglyTypedIdRules_When_ProjectIsCreated()
    {
        // Arrange
        var project1 = Project.Create("DDD");
        var project2 = Project.Create("DDD");

        // Act
        var id1 = project1.Id;
        var id2 = project2.Id;

        // Assert

        // Id should not be empty
        id1.Value.Should().NotBe(Guid.Empty);

        // Each project should have unique Id
        id1.Should().NotBe(id2);

        // Equality should be based on underlying value
        id1.Equals(id1).Should().BeTrue();
    }

    [Fact]
    public void Should_NotBeEqual_When_UnderlyingValuesAreDifferent()
    {
        var project1 = Project.Create("A");
        var project2 = Project.Create("B");

        project1.Id.Should().NotBe(project2.Id);
    }
}