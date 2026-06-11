using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed class FirstNotificationHandler(
    List<string> executionOrder)
    : INotificationHandler<TestNotification>
{
    public Task HandleAsync(
        TestNotification notification,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("FirstHandler");
        return Task.CompletedTask;
    }
}

internal sealed class SecondNotificationHandler(
    List<string> executionOrder)
    : INotificationHandler<TestNotification>
{
    public Task HandleAsync(
        TestNotification notification,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("SecondHandler");
        return Task.CompletedTask;
    }
}

internal sealed class ThirdNotificationHandler(
    List<string> executionOrder)
    : INotificationHandler<TestNotification>
{
    public Task HandleAsync(
        TestNotification notification,
        CancellationToken cancellationToken)
    {
        executionOrder.Add("ThirdHandler");
        return Task.CompletedTask;
    }
}

internal sealed class CancellationAwareNotificationHandler(
    CancellationCapture capture)
    : INotificationHandler<TestNotification>
{
    public Task HandleAsync(
        TestNotification notification,
        CancellationToken cancellationToken)
    {
        capture.Token = cancellationToken;
        return Task.CompletedTask;
    }
}