using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed class TestCommandHandler(
    List<string> executionOrder)
    : IRequestHandler<TestCommand, string>
{
    public Task<string> HandleAsync(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        executionOrder.Add("Handler");
        return Task.FromResult($"Hello {request.Name}");
    }
}

internal sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
{
    public Task<int> HandleAsync(
        TestQuery request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(request.Value * 2);
    }
}

internal sealed class ThrowingCommandHandler : IRequestHandler<TestCommand, string>
{
    public Task<string> HandleAsync(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Handler failure");
    }
}

internal sealed class TrackingCommandHandler(
    List<string> executionOrder)
    : IRequestHandler<TestCommand, string>
{
    public Task<string> HandleAsync(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        executionOrder.Add("Handler");
        return Task.FromResult("OK");
    }
}

internal sealed class CancellationAwareCommandHandler(
    CancellationCapture capture)
    : IRequestHandler<TestCommand, string>
{
    public Task<string> HandleAsync(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        capture.Token = cancellationToken;
        return Task.FromResult("OK");
    }
}