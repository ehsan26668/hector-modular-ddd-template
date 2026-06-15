using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxEventTypeResolverTests
{
    private const string EventName = "test.resolver-domain-event";
    private const int EventVersion = 1;

    [Fact]
    public void Should_ReturnMetadata_When_EventTypeHasOutboxEventAttribute()
    {
        // Arrange
        var resolver = new AttributedOutboxEventTypeResolver(
            [Assembly.GetExecutingAssembly()]);

        // Act
        var metadata = resolver.GetMetadata(typeof(TestDomainEvent));

        // Assert
        metadata.Name.Should().Be(EventName);
        metadata.Version.Should().Be(EventVersion);
        metadata.ClrType.Should().Be<TestDomainEvent>();
    }

    [Fact]
    public void Should_ResolveType_When_LogicalNameAndVersionMatch()
    {
        // Arrange
        var resolver = new AttributedOutboxEventTypeResolver(
            [Assembly.GetExecutingAssembly()]);

        // Act
        var result = resolver.Resolve(EventName, EventVersion);

        // Assert
        result.Should().Be<TestDomainEvent>();
    }

    [Fact]
    public void Should_ThrowException_When_EventTypeDoesNotDefineMetadata()
    {
        // Arrange
        var resolver = new AttributedOutboxEventTypeResolver(
            [Assembly.GetExecutingAssembly()]);

        // Act
        var action = () => resolver.GetMetadata(typeof(EventWithoutMetadata));

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*does not define outbox metadata*");
    }

    [Fact]
    public void Should_ThrowException_When_EventContractCannotBeResolved()
    {
        // Arrange
        var resolver = new AttributedOutboxEventTypeResolver(
            [Assembly.GetExecutingAssembly()]);

        // Act
        var action = () => resolver.Resolve("unknown.event", 1);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*could not be resolved*");
    }

    [OutboxEvent(EventName, EventVersion)]
    private sealed record TestDomainEvent(
        Guid EventId,
        DateTime OccurredOnUtc) : IDomainEvent;

    private sealed record EventWithoutMetadata(
        Guid EventId,
        DateTime OccurredOnUtc) : IDomainEvent;
}