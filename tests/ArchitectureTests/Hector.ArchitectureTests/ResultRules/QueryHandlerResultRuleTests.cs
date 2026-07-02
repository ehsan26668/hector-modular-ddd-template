using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.ArchitectureTests.Common;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class QueryHandlerResultRuleTests : ArchitectureTestBase
{
    [Fact]
    public void Should_ResideInApplicationLayer_When_ImplementingQueryHandler()
    {
        var result = Types
            .InAssemblies(ProductionAssemblies)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .ResideInNamespace("Hector.Modules")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
