using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Testing.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class OutboxProcessorDeserializationTests
{
    [Fact]
    public async Task Should_IncrementRetryCount_When_MessageTypeCannotBeResolved()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        await using var dbContext = await PersistenceTestInfrastructure.CreateContextAsync(connection);

        var messageId = Guid.NewGuid();

        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = messageId,
            Type = "Missing.Namespace.MissingEvent, Missing.Assembly",
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var mediator = Substitute.For<IMediator>();
        var serializer = Substitute.For<IOutboxEventSerializer>();

        serializer.Deserialize(Arg.Any<OutboxMessage>())
                  .Throws(new InvalidOperationException("Event type could not be resolved"));

        var publisher = new OutboxPublisher(mediator, serializer);

        var processor = new OutboxProcessor(
            dbContext,
            publisher,
            NullLogger<OutboxProcessor>.Instance,
            Options.Create(new OutboxOptions
            {
                BatchSize = 20,
                MaxRetryCount = 5,
                LockDuration = TimeSpan.FromMinutes(2)
            }));

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        var message = await dbContext.OutboxMessages
            .SingleAsync(x => x.Id == messageId);

        message.ProcessedOn.Should().BeNull();
        message.RetryCount.Should().Be(1);
        message.Error.Should().NotBeNullOrWhiteSpace();
        message.Error.Should().Contain("could not be resolved");
        message.LockId.Should().BeNull();
        message.LockedUntil.Should().NotBeNull();
        message.LockedUntil.Should().BeAfter(DateTime.UtcNow);

        await mediator.DidNotReceive()
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }
}
