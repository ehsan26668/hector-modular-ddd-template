using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public class QueryAbstractionsTests
{
    [Fact]
    public void Should_InheritFromIRequest_When_QueryImplementsIQuery()
    {
        // Arrange
        var queryType = typeof(TestQuery);

        // Act
        var interfaces = queryType.GetInterfaces();

        // Assert
        interfaces.Should().Contain(typeof(IRequest<Result<string>>));
    }

    [Fact]
    public void Should_InheritFromIRequestHandler_When_HandlerImplementsIQueryHandler()
    {
        // Arrange
        var handlerType = typeof(TestQueryHandler);

        // Act
        var interfaces = handlerType.GetInterfaces();

        // Assert
        interfaces.Should().Contain(typeof(IRequestHandler<TestQuery, Result<string>>));
    }

    private sealed record TestQuery : IQuery<string>;

    private sealed class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<Result<string>> Handle(
            TestQuery request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<string>.Success("ok"));
        }
    }
}
