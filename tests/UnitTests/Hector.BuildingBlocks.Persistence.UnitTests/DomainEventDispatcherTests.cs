using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public class DomainEventDispatcherTests
{
    private readonly IMediator _mediator;
    private readonly IDomainEventDispatcher _sut;

    public DomainEventDispatcherTests()
    {
        _mediator = Substitute.For<IMediator>();
        _sut = new DomainEventDispatcher(_mediator);
    }

    [Fact]
    public async Task Should_PublishAllEvents_When_EventsAreProvided()
    {
        // Arrange
        var event1 = Substitute.For<IDomainEvent>();
        var event2 = Substitute.For<IDomainEvent>();
        var events = new List<IDomainEvent> { event1, event2 };
        var cancellationToken = new CancellationToken();

        // Act
        await _sut.DispatchAsync(events, cancellationToken);

        // Assert
        await _mediator.Received(1).PublishAsync(Arg.Is<INotification>(e => e == event1), cancellationToken);
        await _mediator.Received(1).PublishAsync(Arg.Is<INotification>(e => e == event2), cancellationToken);
    }

    [Fact]
    public async Task Should_NotPublishAnyEvent_When_EventListIsEmpty()
    {
        // Arrange
        var events = Enumerable.Empty<IDomainEvent>();

        // Act
        await _sut.DispatchAsync(events);

        // Assert
        await _mediator.DidNotReceiveWithAnyArgs().PublishAsync<INotification>(default!);
    }
}
