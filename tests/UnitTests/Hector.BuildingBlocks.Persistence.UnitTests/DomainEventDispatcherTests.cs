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

    private record TestDomainEvent : IDomainEvent;

    [Fact]
    public async Task Should_PublishAllEvents_When_EventsAreProvided()
    {
        // Arrange
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        var events = new List<IDomainEvent> { event1, event2 };
        var cancellationToken = new CancellationToken();

        // Act
        await _sut.DispatchAsync(events, cancellationToken);

        // Assert
        await _mediator.Received(1)
            .PublishAsync(Arg.Is<IDomainEvent>(e => ReferenceEquals(e, event1)), cancellationToken);

        await _mediator.Received(1)
            .PublishAsync(Arg.Is<IDomainEvent>(e => ReferenceEquals(e, event2)), cancellationToken);
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

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_EventsIsNull()
    {
        // Arrange
        IEnumerable<IDomainEvent> events = null!;

        // Act
        var act = () => _sut.DispatchAsync(events);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }
}
