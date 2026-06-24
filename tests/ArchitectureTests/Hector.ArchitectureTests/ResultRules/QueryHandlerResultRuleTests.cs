using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class QueryHandlerResultRuleTests
{
    [Fact]
    public void QueryHandlers_Should_Reside_In_ApplicationLayer()
    {
        // Arrange
        var result = Types.InCurrentDomain()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .ResideInNamespace("Hector.Modules")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}