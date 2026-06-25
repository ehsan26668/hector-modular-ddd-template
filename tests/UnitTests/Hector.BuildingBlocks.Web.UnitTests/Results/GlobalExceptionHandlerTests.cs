using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Hector.BuildingBlocks.Web.Results;

namespace Hector.BuildingBlocks.Web.UnitTests.Results;

[Trait("Category", "WebInfrastructure")]
public sealed class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _loggerMock;
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        // Arrange
        _loggerMock = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _sut = new GlobalExceptionHandler(_loggerMock);
    }

    [Fact(DisplayName = "ADR-0055 | TC-01 | Should return true and set status code 500")]
    public async Task Should_ReturnTrueAndSetStatusCode500_When_UnhandledExceptionIsHandled()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Critical system failure!");

        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact(DisplayName = "ADR-0055 | TC-02 | Should write standard problem details contract")]
    public async Task Should_WriteStandardProblemDetailsContract_When_UnhandledExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("System crash");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var body = await ReadResponseAsync(context);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("InternalServerError");
        problemDetails.Status.Should().Be(500);
        problemDetails.Type.Should().Be("https://hector/errors/internal-server-error");
        problemDetails.Detail.Should().Be("An unexpected error occurred on the server.");
    }

    [Fact(DisplayName = "ADR-0055 | TC-03 | Should not expose sensitive exception message")]
    public async Task Should_NotExposeSensitiveExceptionMessage_When_ExceptionContainsInternalDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var sensitiveMessage = "Database connection string 'Server=prod-db;User Id=sa;Password=123' failed";
        var exception = new Exception(sensitiveMessage);

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var body = await ReadResponseAsync(context);
        body.Should().NotContain("sa;Password=123");
        body.Should().Contain("An unexpected error occurred on the server.");
    }

    [Fact(DisplayName = "ADR-0055 | TC-04 | Should include trace Id in problem details extensions")]
    public async Task Should_IncludeTraceIdInProblemDetailsExtensions_When_ExceptionIsHandled()
    {
        // Arrange
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-id-123";
        var exception = new Exception("Error with trace");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var body = await ReadResponseAsync(context);
        body.Should().Contain("test-trace-id-123");
    }

    [Fact(DisplayName = "ADR-0055 | TC-05 | Should log error")]
    public async Task Should_LogError_When_UnhandledExceptionIsCaptured()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Logged error");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<Arg.AnyType>(),
            exception,
            Arg.Any<Func<Arg.AnyType, Exception?, string>>());
    }

    [Fact(DisplayName = "ADR-0055 | TC-06 | Should set problem details content type")]
    public async Task Should_SetProblemDetailsContentType_When_ResponseIsWritten()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Format check");

        // Act
        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.ContentType.Should().Contain("application/problem+json");
    }

    // --- Helpers ---
    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
