using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;
using Hector.BuildingBlocks.Application.UnitTests.Infrastructure;
using Hector.BuildingBlocks.Application.UnitTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class MediatorTests
{
    [Fact]
    public async Task Should_DispatchCommandToHandler_When_HandlerIsRegistered()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>();
        });

        // Act
        var result = await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello Hector");

        fixture.ExecutionOrder.Should().Equal("Handler");
    }

    [Fact]
    public async Task Should_DispatchQueryToHandler_When_HandlerIsRegistered()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<IQueryHandler<TestQuery, int>, TestQueryHandler>();
        });

        // Act
        var result = await fixture.Mediator.SendAsync(new TestQuery(21));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_RequestHandlerIsMissing()
    {
        // Arrange
        var fixture = new MediatorTestFixture();

        // Act
        var act = async () => await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ExecuteSinglePipelineBehaviorAroundHandler_When_BehaviorIsRegistered()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>();
            services.AddTransient<IPipelineBehavior<TestCommand, Result<string>>, TrackingPipelineBehavior>();
        });

        // Act
        var result = await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello Hector");

        fixture.ExecutionOrder.Should().Equal(
            "Pipeline:Before",
            "Handler",
            "Pipeline:After");
    }

    [Fact]
    public async Task Should_ExecutePipelineBehaviorsInRegistrationOrder_When_MultipleBehaviorsAreRegistered()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>();
            services.AddTransient<IPipelineBehavior<TestCommand, Result<string>>, FirstPipelineBehavior>();
            services.AddTransient<IPipelineBehavior<TestCommand, Result<string>>, SecondPipelineBehavior>();
        });

        // Act
        var result = await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello Hector");

        fixture.ExecutionOrder.Should().Equal(
            "First:Before",
            "Second:Before",
            "Handler",
            "Second:After",
            "First:After");
    }

    [Fact]
    public async Task Should_ShortCircuitPipeline_When_BehaviorDoesNotInvokeNext()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>();
            services.AddTransient<IPipelineBehavior<TestCommand, Result<string>>, ShortCircuitPipelineBehavior>();
        });

        // Act
        var result = await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ShortCircuited");

        fixture.ExecutionOrder.Should().Equal("ShortCircuit");
    }

    [Fact]
    public async Task Should_PropagateException_When_HandlerThrows()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<ICommandHandler<TestCommand, string>, ThrowingCommandHandler>();
        });

        // Act
        var act = async () => await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Handler failure");
    }

    [Fact]
    public async Task Should_PropagateCancellationTokenToRequestHandler_When_RequestIsSent()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddSingleton<CancellationCapture>();
            services.AddTransient<ICommandHandler<TestCommand, string>, CancellationAwareCommandHandler>();
        });

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await fixture.Mediator.SendAsync(new TestCommand("Hector"), cancellationTokenSource.Token);

        // Assert
        fixture.GetRequiredService<CancellationCapture>()
            .Token.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Should_PropagateCancellationTokenToPipelineBehavior_When_RequestIsSent()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddSingleton<CancellationCapture>();
            services.AddTransient<ICommandHandler<TestCommand, string>, TestCommandHandler>();
            services.AddTransient<IPipelineBehavior<TestCommand, Result<string>>, CancellationAwarePipelineBehavior>();
        });

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await fixture.Mediator.SendAsync(new TestCommand("Hector"), cancellationTokenSource.Token);

        // Assert
        fixture.GetRequiredService<CancellationCapture>()
            .Token.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Should_InvokeAllNotificationHandlers_When_NotificationIsPublished()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<INotificationHandler<TestNotification>, FirstNotificationHandler>();
            services.AddTransient<INotificationHandler<TestNotification>, SecondNotificationHandler>();
        });

        // Act
        await fixture.Mediator.PublishAsync(new TestNotification());

        // Assert
        fixture.ExecutionOrder.Should().Equal(
            "FirstHandler",
            "SecondHandler");
    }

    [Fact]
    public async Task Should_InvokeNotificationHandlersInRegistrationOrder_When_MultipleHandlersAreRegistered()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddTransient<INotificationHandler<TestNotification>, FirstNotificationHandler>();
            services.AddTransient<INotificationHandler<TestNotification>, SecondNotificationHandler>();
            services.AddTransient<INotificationHandler<TestNotification>, ThirdNotificationHandler>();
        });

        // Act
        await fixture.Mediator.PublishAsync(new TestNotification());

        // Assert
        fixture.ExecutionOrder.Should().Equal(
            "FirstHandler",
            "SecondHandler",
            "ThirdHandler");
    }

    [Fact]
    public async Task Should_PropagateCancellationTokenToNotificationHandlers_When_NotificationIsPublished()
    {
        // Arrange
        var fixture = new MediatorTestFixture(services =>
        {
            services.AddSingleton<CancellationCapture>();
            services.AddTransient<INotificationHandler<TestNotification>, CancellationAwareNotificationHandler>();
        });

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await fixture.Mediator.PublishAsync(new TestNotification(), cancellationTokenSource.Token);

        // Assert
        fixture.GetRequiredService<CancellationCapture>()
            .Token.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Should_NotThrow_When_NoNotificationHandlerIsRegistered()
    {
        // Arrange
        var fixture = new MediatorTestFixture();

        // Act
        var act = async () => await fixture.Mediator.PublishAsync(new TestNotification());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        // Arrange
        var fixture = new MediatorTestFixture();

        // Act
        var act = async () => await fixture.Mediator.SendAsync<Result<string>>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_NotificationIsNull()
    {
        // Arrange
        var fixture = new MediatorTestFixture();

        // Act
        var act = async () => await fixture.Mediator.PublishAsync<TestNotification>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
