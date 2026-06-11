using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed class TrackingPipelineBehavior(
    List<string> executionOrder)
    : IPipelineBehavior<TestCommand, string>
{
    public async Task<string> HandleAsync(
        TestCommand request,
        RequestHandlerDelegate<string> next,
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
    : IPipelineBehavior<TestCommand, string>
{
    public async Task<string> HandleAsync(
        TestCommand request,
        RequestHandlerDelegate<string> next,
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
    : IPipelineBehavior<TestCommand, string>
{
    public async Task<string> HandleAsync(
        TestCommand request,
        RequestHandlerDelegate<string> next,
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
    : IPipelineBehavior<TestCommand, string>
{
    public Task<string> HandleAsync(
        TestCommand request,
        RequestHandlerDelegate<string> next,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("ShortCircuit");
        return Task.FromResult("ShortCircuited");
    }
}

internal sealed class CancellationAwarePipelineBehavior(
    CancellationCapture capture)
    : IPipelineBehavior<TestCommand, string>
{
    public Task<string> HandleAsync(
        TestCommand request,
        RequestHandlerDelegate<string> next,
        CancellationToken cancellationToken)
    {
        capture.Token = cancellationToken;
        return next();
    }
}