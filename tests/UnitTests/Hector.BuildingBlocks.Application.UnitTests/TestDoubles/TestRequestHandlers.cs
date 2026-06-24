using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed class TestCommandHandler(
    List<string> executionOrder)
    : ICommandHandler<TestCommand, string>
{
    public Task<Result<string>> Handle(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        executionOrder.Add("Handler");

        return Task.FromResult(
            Result<string>.Success($"Hello {request.Name}"));
    }
}

internal sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
{
    public Task<Result<int>> Handle(
        TestQuery request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            Result<int>.Success(request.Value * 2));
    }
}

internal sealed class ThrowingCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<Result<string>> Handle(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Handler failure");
    }
}

internal sealed class TrackingCommandHandler(
    List<string> executionOrder)
    : ICommandHandler<TestCommand, string>
{
    public Task<Result<string>> Handle(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        executionOrder.Add("Handler");

        return Task.FromResult(
            Result<string>.Success("OK"));
    }
}

internal sealed class CancellationAwareCommandHandler(
    CancellationCapture capture)
    : ICommandHandler<TestCommand, string>
{
    public Task<Result<string>> Handle(
        TestCommand request,
        CancellationToken cancellationToken = default)
    {
        capture.Token = cancellationToken;

        return Task.FromResult(
            Result<string>.Success("OK"));
    }
}
