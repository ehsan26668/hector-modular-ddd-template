using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Inbox;

namespace Hector.ArchitectureTests;

public sealed class IntegrationEventBusContractTests
{
    [Fact]
    public void Should_NotAcceptInboxMessage_When_PublishingIntegrationEvent()
    {
        // Arrange
        var publishAsync = typeof(IIntegrationEventBus)
            .GetMethod(nameof(IIntegrationEventBus.PublishAsync));

        // Act
        var parameterTypes = publishAsync!
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToList();

        // Assert
        parameterTypes.Should().Contain(typeof(IIntegrationEvent));
        parameterTypes.Should().NotContain(typeof(IInboxMessage));
    }
}
