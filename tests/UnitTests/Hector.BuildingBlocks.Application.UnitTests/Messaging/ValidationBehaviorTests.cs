using FluentAssertions;
using FluentValidation;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.UnitTests.Infrastructure;
using Hector.BuildingBlocks.Application.UnitTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Should_InvokeHandler_When_RequestIsValid()
    {
        // Arrange
        var fixture = CreateFixture(services =>
        {
            services.AddTransient<IValidator<TestCommand>, TestCommandValidator>();
        });

        // Act
        var result = await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        result.Should().Be("OK");
        fixture.ExecutionOrder.Should().Equal("Handler");
    }

    [Fact]
    public async Task Should_ThrowValidationException_When_RequestIsInvalid()
    {
        // Arrange
        var fixture = CreateFixture(services =>
        {
            services.AddTransient<IValidator<TestCommand>, TestCommandValidator>();
        });

        // Act
        var act = async () => await fixture.Mediator.SendAsync(new TestCommand(string.Empty));

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        fixture.ExecutionOrder.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_AggregateAllValidationFailures_When_MultipleValidatorsFail()
    {
        // Arrange
        var fixture = CreateFixture(services =>
        {
            services.AddTransient<IValidator<TestCommand>, EmptyNameValidator>();
            services.AddTransient<IValidator<TestCommand>, MinimumLengthValidator>();
        });

        // Act
        var act = async () => await fixture.Mediator.SendAsync(new TestCommand(string.Empty));

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
        fixture.ExecutionOrder.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_InvokeHandler_When_NoValidatorsAreRegistered()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var result = await fixture.Mediator.SendAsync(new TestCommand("Hector"));

        // Assert
        result.Should().Be("OK");
        fixture.ExecutionOrder.Should().Equal("Handler");
    }

    [Fact]
    public async Task Should_PropagateCancellationTokenToValidators_When_RequestIsValidated()
    {
        // Arrange
        var fixture = CreateFixture(services =>
        {
            services.AddSingleton<CancellationCapture>();
            services.AddTransient<IValidator<TestCommand>, CancellationAwareValidator>();
        });

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await fixture.Mediator.SendAsync(new TestCommand("Hector"), cancellationTokenSource.Token);

        // Assert
        fixture.GetRequiredService<CancellationCapture>().Token.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Should_PropagateCancellationTokenToNextHandler_When_ValidationPasses()
    {
        // Arrange
        var fixture = CreateFixture(services =>
        {
            services.AddSingleton<CancellationCapture>();
            services.AddTransient<IValidator<TestCommand>, TestCommandValidator>();
            services.AddTransient<IRequestHandler<TestCommand, string>, CancellationAwareCommandHandler>();
        });

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await fixture.Mediator.SendAsync(new TestCommand("Hector"), cancellationTokenSource.Token);

        // Assert
        fixture.GetRequiredService<CancellationCapture>().Token.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var act = async () => await fixture.Mediator.SendAsync<string>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Should_RegisterExpectedValidationPipelineAndValidators_Once()
    {
        // Arrange
        var fixture = CreateFixture(services =>
        {
            services.AddTransient<IValidator<TestCommand>, EmptyNameValidator>();
            services.AddTransient<IValidator<TestCommand>, MinimumLengthValidator>();
        });

        // Act
        var behaviors = fixture.GetServices<IPipelineBehavior<TestCommand, string>>();
        var validators = fixture.GetServices<IValidator<TestCommand>>();

        // Assert
        behaviors.Should().HaveCount(1);
        behaviors[0].Should().BeOfType<ValidationBehavior<TestCommand, string>>();

        validators.Should().HaveCount(2);
        validators.Should().ContainSingle(v => v.GetType() == typeof(EmptyNameValidator));
        validators.Should().ContainSingle(v => v.GetType() == typeof(MinimumLengthValidator));
    }

    [Fact]
    public async Task Should_ReturnExpectedFailures_FromValidators_InIsolation()
    {
        // Arrange
        var validators = new IValidator<TestCommand>[]
        {
        new EmptyNameValidator(),
        new MinimumLengthValidator()
        };

        var command = new TestCommand(string.Empty);

        // Act
        var results = await Task.WhenAll(
            validators.Select(validator =>
                validator.ValidateAsync(
                    new ValidationContext<TestCommand>(command),
                    CancellationToken.None)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        // Assert
        results.Should().HaveCount(2);
        failures.Should().HaveCount(2);
        failures.Should().Contain(failure => failure.ErrorMessage.Contains("must not be empty"));
        failures.Should().Contain(failure => failure.ErrorMessage.Contains("at least 3 characters"));
    }

    private static MediatorTestFixture CreateFixture(Action<IServiceCollection>? configureServices = null)
    {
        return new MediatorTestFixture(services =>
        {
            services.AddTransient<IRequestHandler<TestCommand, string>, TrackingCommandHandler>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            configureServices?.Invoke(services);
        });
    }
}
