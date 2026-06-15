using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
using NSubstitute;
using Xunit;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class InboxPipelineBehaviorTests
{
    private sealed record TestIntegrationEvent(Guid MessageId) : IIntegrationEvent, IRequest<object>;

    [Fact]
    public async Task Should_InvokeHandler_When_MessageIsNotDuplicate()
    {
        // Arrange
        var inboxStore = Substitute.For<IInboxStore>();
        var moduleIdentity = Substitute.For<IModuleIdentity>();

        moduleIdentity.ModuleName.Returns("projects");

        inboxStore.TryStoreAsync(Arg.Any<Guid>(), "projects", Arg.Any<CancellationToken>())
            .Returns(true);

        var behavior = new InboxPipelineBehavior<TestIntegrationEvent, object>(
            inboxStore,
            moduleIdentity);

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
        var moduleIdentity = Substitute.For<IModuleIdentity>();

        moduleIdentity.ModuleName.Returns("projects");

        inboxStore.TryStoreAsync(Arg.Any<Guid>(), "projects", Arg.Any<CancellationToken>())
            .Returns(false);

        var behavior = new InboxPipelineBehavior<TestIntegrationEvent, object>(
            inboxStore,
            moduleIdentity);

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
