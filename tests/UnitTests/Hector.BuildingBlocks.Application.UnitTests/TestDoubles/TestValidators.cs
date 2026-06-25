using FluentValidation;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed class TestCommandValidator : AbstractValidator<TestCommand>
{
    public TestCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty();
    }
}

internal sealed class EmptyNameValidator : AbstractValidator<TestCommand>
{
    public EmptyNameValidator()
    {
        RuleFor(command => command.Name).NotEmpty();
    }
}

internal sealed class MinimumLengthValidator : AbstractValidator<TestCommand>
{
    public MinimumLengthValidator()
    {
        RuleFor(command => command.Name).MinimumLength(3);
    }
}

internal sealed class CancellationAwareValidator : AbstractValidator<TestCommand>
{
    private readonly CancellationCapture _capture;

    public CancellationAwareValidator(CancellationCapture capture)
    {
        _capture = capture;

        RuleFor(command => command.Name)
            .MustAsync(async (_, _, cancellationToken) =>
            {
                _capture.Token = cancellationToken;
                await Task.CompletedTask;
                return true;
            });
    }
}

internal sealed class TestResultCommandValidator : AbstractValidator<TestResultCommand>
{
    public TestResultCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty();
    }
}