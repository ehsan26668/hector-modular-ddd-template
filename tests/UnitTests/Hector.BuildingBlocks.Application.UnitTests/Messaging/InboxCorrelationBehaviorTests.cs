using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Correlation;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
using NSubstitute;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class InboxCorrelationBehaviorTests
{
    [Fact]
    public async Task Should_RestoreCorrelationContext_When_IntegrationEventIsHandled()
    {
        // Arrange
        var inboxStore = Substitute.For<IInboxStore>();
        var consumerNameProvider = Substitute.For<IInboxConsumerNameProvider>();
        var accessor = new CorrelationContextAccessor();

        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        consumerNameProvider.ConsumerName.Returns("Projects");

        inboxStore
            .TryStoreAsync(messageId, "Projects", Arg.Any<CancellationToken>())
            .Returns(true);

        var behavior = new InboxPipelineBehavior<TestIntegrationEvent, bool>(
            inboxStore,
            consumerNameProvider,
            accessor);

        var integrationEvent = new TestIntegrationEvent(
            messageId,
            correlationId,
            null,
            "trace-1");

        CorrelationContext? captured = null;

        // Act
        await behavior.Handle(
            integrationEvent,
            () =>
            {
                captured = accessor.Current;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.CorrelationId.Should().Be(correlationId);
        captured.CausationId.Should().Be(messageId);
        captured.TraceId.Should().Be("trace-1");
    }

    [Fact]
    public async Task Should_ClearCorrelationContext_When_HandlerCompletes()
    {
        // Arrange
        var inboxStore = Substitute.For<IInboxStore>();
        var consumerNameProvider = Substitute.For<IInboxConsumerNameProvider>();
        var accessor = new CorrelationContextAccessor();

        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        consumerNameProvider.ConsumerName.Returns("Projects");

        inboxStore
            .TryStoreAsync(messageId, "Projects", Arg.Any<CancellationToken>())
            .Returns(true);

        var behavior = new InboxPipelineBehavior<TestIntegrationEvent, bool>(
            inboxStore,
            consumerNameProvider,
            accessor);

        var integrationEvent = new TestIntegrationEvent(
            messageId,
            correlationId,
            null,
            null);

        // Act
        await behavior.Handle(
            integrationEvent,
            () => Task.FromResult(true),
            CancellationToken.None);

        // Assert
        accessor.Current.Should().BeNull();
    }

    [Fact]
    public async Task Should_NotExecuteHandler_When_MessageAlreadyProcessed()
    {
        // Arrange
        var inboxStore = Substitute.For<IInboxStore>();
        var consumerNameProvider = Substitute.For<IInboxConsumerNameProvider>();
        var accessor = new CorrelationContextAccessor();

        var messageId = Guid.NewGuid();

        consumerNameProvider.ConsumerName.Returns("Projects");

        inboxStore
            .TryStoreAsync(messageId, "Projects", Arg.Any<CancellationToken>())
            .Returns(false);

        var behavior = new InboxPipelineBehavior<TestIntegrationEvent, bool>(
            inboxStore,
            consumerNameProvider,
            accessor);

        var integrationEvent = new TestIntegrationEvent(
            messageId,
            Guid.NewGuid(),
            null,
            null);

        var handlerExecuted = false;

        // Act
        await behavior.Handle(
            integrationEvent,
            () =>
            {
                handlerExecuted = true;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        handlerExecuted.Should().BeFalse();
        accessor.Current.Should().BeNull();
    }

    private sealed record TestIntegrationEvent(
        Guid MessageId,
        Guid CorrelationId,
        Guid? CausationId,
        string? TraceId)
        : IRequest<bool>, IIntegrationEvent;
}
