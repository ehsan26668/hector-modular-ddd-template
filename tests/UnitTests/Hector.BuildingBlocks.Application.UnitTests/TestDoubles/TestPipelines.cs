using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed class TrackingPipelineBehavior(
    List<string> executionOrder)
    : IPipelineBehavior<TestCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        TestCommand request,
        RequestHandlerDelegate<Result<string>> next,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("Pipeline:Before");

        var result = await next();

        executionOrder.Add("Pipeline:After");

        return result;
    }
}

internal sealed class FirstPipelineBehavior(
    List<string> executionOrder)
    : IPipelineBehavior<TestCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        TestCommand request,
        RequestHandlerDelegate<Result<string>> next,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("First:Before");

        var result = await next();

        executionOrder.Add("First:After");

        return result;
    }
}

internal sealed class SecondPipelineBehavior(
    List<string> executionOrder)
    : IPipelineBehavior<TestCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        TestCommand request,
        RequestHandlerDelegate<Result<string>> next,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("Second:Before");

        var result = await next();

        executionOrder.Add("Second:After");

        return result;
    }
}

internal sealed class ShortCircuitPipelineBehavior(
    List<string> executionOrder)
    : IPipelineBehavior<TestCommand, Result<string>>
{
    public Task<Result<string>> Handle(
        TestCommand request,
        RequestHandlerDelegate<Result<string>> next,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("ShortCircuit");

        return Task.FromResult(
            Result<string>.Success("ShortCircuited"));
    }
}

internal sealed class CancellationAwarePipelineBehavior(
    CancellationCapture capture)
    : IPipelineBehavior<TestCommand, Result<string>>
{
    public Task<Result<string>> Handle(
        TestCommand request,
        RequestHandlerDelegate<Result<string>> next,
        CancellationToken cancellationToken)
    {
        capture.Token = cancellationToken;

        return next();
    }
}
