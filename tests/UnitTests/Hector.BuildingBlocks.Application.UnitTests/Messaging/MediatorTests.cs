using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class MediatorTests
{
    [Fact]
    public async Task Should_DispatchCommandToHandler_When_HandlerIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var executionOrder = new List<string>();

        services.AddSingleton(executionOrder);
        services.AddTransient<IRequestHandler<TestCommand, string>, TestCommandHandler>();
        services.AddSingleton<IMediator, Mediator>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new TestCommand("Hector");

        // Act
        var result = await mediator.SendAsync(command);

        // Assert
        result.Should().Be("Hello Hector");
    }

    [Fact]
    public async Task Should_ExecutePipelineBehavior_Around_Handler()
    {
        // Arrange
        var services = new ServiceCollection();
        var executionOrder = new List<string>();

        services.AddSingleton(executionOrder);
        services.AddTransient<IRequestHandler<TestCommand, string>>(
            _ => new TestCommandHandler(executionOrder));
        services.AddTransient<IPipelineBehavior<TestCommand, string>>(
            _ => new TestPipelineBehavior(executionOrder));
        services.AddSingleton<IMediator, Mediator>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new TestCommand("Hector");

        // Act
        var result = await mediator.SendAsync(command);

        // Assert
        result.Should().Be("Hello Hector");

        executionOrder.Should().Equal(
            "Pipeline:Before",
            "Handler",
            "Pipeline:After");
    }

    [Fact]
    public async Task Should_Execute_MultiplePipelineBehaviors_In_Registration_Order()
    {
        // Arrange
        var services = new ServiceCollection();
        var executionOrder = new List<string>();

        services.AddSingleton(executionOrder);

        services.AddTransient<IRequestHandler<TestCommand, string>>(
            _ => new TestCommandHandler(executionOrder));

        services.AddTransient<IPipelineBehavior<TestCommand, string>>(
            _ => new FirstPipelineBehavior(executionOrder));

        services.AddTransient<IPipelineBehavior<TestCommand, string>>(
            _ => new SecondPipelineBehavior(executionOrder));

        services.AddSingleton<IMediator, Mediator>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new TestCommand("Hector");

        // Act
        var result = await mediator.SendAsync(command);

        // Assert
        executionOrder.Should().Equal(
            "First:Before",
            "Second:Before",
            "Handler",
            "Second:After",
            "First:After");
    }

    private sealed record TestCommand(string Name) : ICommand<string>;

    private sealed class TestCommandHandler
    : IRequestHandler<TestCommand, string>
    {
        private readonly List<string> _executionOrder;

        public TestCommandHandler(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public Task<string> HandleAsync(
            TestCommand request,
            CancellationToken cancellationToken = default)
        {
            _executionOrder.Add("Handler");
            return Task.FromResult($"Hello {request.Name}");
        }
    }

    private sealed class TestPipelineBehavior
    : IPipelineBehavior<TestCommand, string>
    {
        private readonly List<string> _executionOrder;

        public TestPipelineBehavior(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async Task<string> HandleAsync(
            TestCommand request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            _executionOrder.Add("Pipeline:Before");

            var response = await next();

            _executionOrder.Add("Pipeline:After");

            return response;
        }
    }

    private sealed class FirstPipelineBehavior
        : IPipelineBehavior<TestCommand, string>
    {
        private readonly List<string> _executionOrder;

        public FirstPipelineBehavior(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async Task<string> HandleAsync(
            TestCommand request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            _executionOrder.Add("First:Before");

            var response = await next();

            _executionOrder.Add("First:After");

            return response;
        }
    }

    private sealed class SecondPipelineBehavior
        : IPipelineBehavior<TestCommand, string>
    {
        private readonly List<string> _executionOrder;

        public SecondPipelineBehavior(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async Task<string> HandleAsync(
            TestCommand request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            _executionOrder.Add("Second:Before");

            var response = await next();

            _executionOrder.Add("Second:After");

            return response;
        }
    }
}
