using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging.Correlation;
using Hector.BuildingBlocks.Web.Correlation;
using Microsoft.AspNetCore.Http;

namespace Hector.BuildingBlocks.Web.UnitTests.Correlation;

public sealed class CorrelationMiddlewareTests
{
    [Fact]
    public async Task Should_SetCorrelationContext_When_CorrelationHeaderExists()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var accessor = new CorrelationContextAccessor();
        CorrelationContext? capturedContext = null;

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[CorrelationHeaderNames.CorrelationId] =
            correlationId.ToString();

        var middleware = new CorrelationMiddleware(_ =>
        {
            capturedContext = accessor.Current;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext, accessor);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext!.CorrelationId.Should().Be(correlationId);
        httpContext.Response.Headers[CorrelationHeaderNames.CorrelationId]
            .ToString()
            .Should()
            .Be(correlationId.ToString());
    }

    [Fact]
    public async Task Should_GenerateCorrelationId_When_CorrelationHeaderDoesNotExist()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        CorrelationContext? capturedContext = null;

        var httpContext = new DefaultHttpContext();

        var middleware = new CorrelationMiddleware(_ =>
        {
            capturedContext = accessor.Current;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext, accessor);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext!.CorrelationId.Should().NotBeEmpty();
        httpContext.Response.Headers[CorrelationHeaderNames.CorrelationId]
            .ToString()
            .Should()
            .Be(capturedContext.CorrelationId.ToString());
    }

    [Fact]
    public async Task Should_ClearCorrelationContext_When_RequestCompleted()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        var httpContext = new DefaultHttpContext();

        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext, accessor);

        // Assert
        accessor.Current.Should().BeNull();
    }
}