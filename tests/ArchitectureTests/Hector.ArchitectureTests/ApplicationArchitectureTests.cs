using FluentAssertions;
using Hector.BuildingBlocks.Application;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class ApplicationArchitectureTests
{
    [Fact]
    public void Should_NotDependOnInboxAbstractions_When_InApplicationLayer()
    {
        // Arrange
        var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(applicationAssembly)
            .That()
            .DoNotResideInNamespace("Hector.BuildingBlocks.Application.Messaging.Inbox")
            .ShouldNot()
            .HaveDependencyOn("Hector.BuildingBlocks.Application.Messaging.Inbox")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application publishing contracts must not depend on Inbox abstractions. " +
            "Inbox belongs to the consumer side, while IIntegrationEvent and IIntegrationEventBus belong to the producer-facing application messaging API as defined in ADR-0039.");
    }
}
