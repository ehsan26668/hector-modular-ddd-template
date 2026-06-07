using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Domain.UnitTests;

public sealed class ProjectTests
{
    [Fact]
    public void Should_CreateProject_When_NameIsValid()
    {
        // Arrange
        var name = "Test Project";

        // Act
        var project = Project.Create(name);

        // Assert
        project.Should().NotBeNull();
        project.Name.Should().Be(name);
        project.Id.Should().NotBeNull();
    }

    [Fact]
    public void Should_RecordDomainEvent_When_ProjectIsCreated()
    {
        // Arrange
        var name = "Test Project";

        // Act
        var project = Project.Create(name);

        // Assert
        var domainEvent = project.GetDomainEvents()
            .OfType<ProjectCreatedDomainEvent>()
            .Single();

        domainEvent.ProjectId.Should().Be(project.Id);
        domainEvent.Name.Should().Be(name);
    }

    [Fact]
    public void Should_ThrowException_When_NameIsEmpty()
    {
        // Act
        var act = () => Project.Create("");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
    }
}