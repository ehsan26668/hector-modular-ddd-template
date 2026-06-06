using FluentAssertions;
using FluentValidation;
using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Should_ThrowValidationException_When_RequestIsInvalid()
    {
        // Arrange
        var services = new ServiceCollection();

        var executionOrder = new List<string>();
        services.AddSingleton(executionOrder);

        services.AddSingleton<IMediator, Mediator>();

        services.AddTransient<IRequestHandler<TestCommand, string>>(
            serviceProvider => new TestCommandHandler(
                serviceProvider.GetRequiredService<List<string>>()));

        services.AddTransient<IValidator<TestCommand>, TestCommandValidator>();

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        var serviceProvider = services.BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCommand(Name: string.Empty);

        // Act
        var act = async () => await mediator.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        executionOrder.Should().BeEmpty();
    }

    private sealed record TestCommand(string Name) : ICommand<string>;

    private sealed class TestCommandHandler : IRequestHandler<TestCommand, string>
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

            return Task.FromResult("OK");
        }
    }

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty();
        }
    }
}