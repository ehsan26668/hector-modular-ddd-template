using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
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

        inbox.ExistsAsync(messageId, consumer, Arg.Any<CancellationToken>())
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
        await behavior.Handle("request", Next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeFalse();
    }
}