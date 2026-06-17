using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Correlation;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
using NSubstitute;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class InboxPipelineBehaviorTests
{
    private sealed record TestIntegrationEvent(Guid MessageId) : IIntegrationEvent, IRequest<object>
    {
        public Guid CorrelationId { get; init; } = Guid.NewGuid();

        public Guid? CausationId { get; init; }

        public string? TraceId { get; init; }
    }

    [Fact]
    public async Task Should_InvokeHandler_When_MessageIsNotDuplicate()
    {
        // Arrange
        var inboxStore = Substitute.For<IInboxStore>();
        var consumerNameProvider = Substitute.For<IInboxConsumerNameProvider>();
        var correlationContextAccessor = new CorrelationContextAccessor();

        consumerNameProvider.ConsumerName.Returns("projects");

        inboxStore.TryStoreAsync(Arg.Any<Guid>(), "projects", Arg.Any<CancellationToken>())
            .Returns(true);

        var behavior = new InboxPipelineBehavior<TestIntegrationEvent, object>(
            inboxStore,
            consumerNameProvider,
            correlationContextAccessor);

        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());
        var nextCalled = false;

        Task<object> Next()
        {
            nextCalled = true;
            return Task.FromResult(new object());
        }

        // Act
        await behavior.HandleAsync(integrationEvent, Next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_NotInvokeHandler_When_MessageIsDuplicate()
    {
        // Arrange
        var inboxStore = Substitute.For<IInboxStore>();
        var consumerNameProvider = Substitute.For<IInboxConsumerNameProvider>();
        var correlationContextAccessor = new CorrelationContextAccessor();

        consumerNameProvider.ConsumerName.Returns("projects");

        inboxStore.TryStoreAsync(Arg.Any<Guid>(), "projects", Arg.Any<CancellationToken>())
            .Returns(false);

        var behavior = new InboxPipelineBehavior<TestIntegrationEvent, object>(
            inboxStore,
            consumerNameProvider,
            correlationContextAccessor);

        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());
        var nextCalled = false;

        Task<object> Next()
        {
            nextCalled = true;
            return Task.FromResult(new object());
        }

        // Act
        await behavior.HandleAsync(integrationEvent, Next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
    }
}
