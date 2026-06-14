using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class DefaultOutboxMessageFactoryTests
{
    [Fact]
    public void Should_ThrowArgumentNullException_When_IntegrationEventIsNull()
    {
        // Arrange
        var serializer = Substitute.For<IOutboxEventSerializer>();
        var typeResolver = Substitute.For<IOutboxEventTypeResolver>();
        var factory = new DefaultOutboxMessageFactory(serializer, typeResolver);

        // Act
        var act = () => factory.Create(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_CreateOutboxMessage_When_IntegrationEventIsValid()
    {
        // Arrange
        var serializer = Substitute.For<IOutboxEventSerializer>();
        var typeResolver = Substitute.For<IOutboxEventTypeResolver>();
        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());

        const string eventName = "projects.project-created";
        const int version = 1;
        const string serializedContent = "{\"projectId\":\"123\"}";

        typeResolver
            .GetMetadata(typeof(TestIntegrationEvent))
            .Returns(new OutboxEventMetadata(eventName, version, typeof(TestIntegrationEvent)));

        serializer
            .Serialize(integrationEvent)
            .Returns(serializedContent);

        var factory = new DefaultOutboxMessageFactory(serializer, typeResolver);

        // Act
        var result = factory.Create(integrationEvent);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(integrationEvent.MessageId);
        result.Type.Should().Be(eventName);
        result.Version.Should().Be(version);
        result.Content.Should().Be(serializedContent);
        result.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Should_CallDependenciesWithCorrectParameters_When_CreatingMessage()
    {
        // Arrange
        var serializer = Substitute.For<IOutboxEventSerializer>();
        var typeResolver = Substitute.For<IOutboxEventTypeResolver>();
        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());

        typeResolver.GetMetadata(Arg.Any<Type>())
            .Returns(new OutboxEventMetadata("any", 1, typeof(TestIntegrationEvent)));

        var factory = new DefaultOutboxMessageFactory(serializer, typeResolver);

        // Act
        factory.Create(integrationEvent);

        // Assert
        typeResolver.Received(1).GetMetadata(typeof(TestIntegrationEvent));
        serializer.Received(1).Serialize(integrationEvent);
    }

    private sealed record TestIntegrationEvent(Guid MessageId) : IIntegrationEvent;
}
