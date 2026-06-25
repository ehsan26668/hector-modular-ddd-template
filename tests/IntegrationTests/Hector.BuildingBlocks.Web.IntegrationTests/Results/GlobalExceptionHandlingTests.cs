using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hector.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Web.IntegrationTests.Results;

public sealed class GlobalExceptionTestFactory : TestApplicationFactory
{
    public GlobalExceptionTestFactory() : base(null) { }
}

[Trait("Category", "WebInfrastructure")]
public sealed class GlobalExceptionHandlingTests(
    GlobalExceptionTestFactory factory)
    : IClassFixture<GlobalExceptionTestFactory>
{
    private readonly HttpClient _client = factory.WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IStartupFilter, UnhandledExceptionEndpointStartupFilter>();
        });
    }).CreateClient();

    [Fact(DisplayName = "ADR-0055 | TC-07 | Should return problem details response")]
    public async Task Should_ReturnProblemDetailsResponse_When_EndpointThrowsUnhandledException()
    {
        // Arrange
        const string requestUri = "/test/unhandled-exception";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be((int)HttpStatusCode.InternalServerError);
        problemDetails.Title.Should().Be("InternalServerError");

        // Assert (Sanitization Check) - TC-03
        problemDetails.Detail.Should().Be("An unexpected error occurred on the server.");
        problemDetails.Detail.Should().NotContain("Critical system failure!");

        // Assert (Traceability Check) - TC-04
        problemDetails.Extensions.Should().ContainKey("traceId");

        var traceId = problemDetails.Extensions["traceId"]?.ToString();
        traceId.Should().NotBeNullOrWhiteSpace();

    }

    [Fact(DisplayName = "ADR-0055 | TC-08 | Should not override result based failure")]
    public async Task Should_NotOverrideResultBasedFailure_When_EndpointReturnsFailureResult()
    {
        // Arrange
        const string requestUri = "/test/failure-result";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Domain/Application Error Title");
        problemDetails.Extensions.Should().ContainKey("errorCode");
    }

    [Fact(DisplayName = "ADR-0055 | TC-09 | Should catch exceptions thrown after exception handler registration")]
    public async Task Should_CatchExceptionsThrownAfterExceptionHandlerRegistration_When_RequestPipelineExecutes()
    {
        // Arrange
        const string requestUri = "/test/exception-after-middleware";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("Exception after registration"); // Sanitization
    }

    [Fact(DisplayName = "ADR-0055 | TC-10 | Should preserve unified error contract")]
    public async Task Should_PreserveUnifiedErrorContract_When_ExceptionResponseIsReturned()
    {
        // Arrange
        const string requestUri = "/test/throwing-endpoint";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Validate Contract
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().Be("InternalServerError");
        problemDetails.Detail.Should().Be("An unexpected error occurred on the server.");
        problemDetails.Extensions.Should().ContainKey("traceId");
    }
}
