using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
using NSubstitute;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class InboxBehaviorTests
{
    [Fact]
    public async Task Should_NotExecuteHandler_When_MessageAlreadyProcessed()
    {
        // Arrange
        var inbox = Substitute.For<IInboxStore>();

        var messageId = Guid.NewGuid();
        var consumer = "TestConsumer";

        inbox.TryStoreAsync(messageId, consumer, Arg.Any<CancellationToken>())
            .Returns(false);

        var behavior = new InboxBehavior<string>(
            inbox,
            messageId,
            consumer);

        var nextExecuted = false;

        Task<string> Next()
        {
            nextExecuted = true;
            return Task.FromResult("ok");
        }

        // Act
        var response = await behavior.Handle("request", Next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeFalse();
        response.Should().BeNull();
    }

    [Fact]
    public async Task Should_ExecuteHandler_When_MessageIsNew()
    {
        // Arrange
        var inbox = Substitute.For<IInboxStore>();

        var messageId = Guid.NewGuid();
        var consumer = "TestConsumer";

        inbox.TryStoreAsync(messageId, consumer, Arg.Any<CancellationToken>())
            .Returns(true);

        var behavior = new InboxBehavior<string>(
            inbox,
            messageId,
            consumer);

        var nextExecuted = false;

        Task<string> Next()
        {
            nextExecuted = true;
            return Task.FromResult("ok");
        }

        // Act
        var response = await behavior.Handle("request", Next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeTrue();
        response.Should().Be("ok");
    }

    [Fact]
    public async Task Should_StoreMessageBeforeExecutingHandler()
    {
        // Arrange
        var inbox = Substitute.For<IInboxStore>();

        var messageId = Guid.NewGuid();
        var consumer = "TestConsumer";
        var storeCalled = false;

        inbox.TryStoreAsync(messageId, consumer, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                storeCalled = true;
                return true;
            });

        var behavior = new InboxBehavior<string>(
            inbox,
            messageId,
            consumer);

        Task<string> Next()
        {
            storeCalled.Should().BeTrue();
            return Task.FromResult("ok");
        }

        // Act
        var response = await behavior.Handle("request", Next, CancellationToken.None);

        // Assert
        response.Should().Be("ok");
    }
}
