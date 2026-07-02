using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.ArchitectureTests.Common;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class CommandHandlerResultRuleTests : ArchitectureTestBase
{
    [Fact]
    public void Should_ResideInApplicationLayer_When_ImplementingCommandHandler()
    {
        // Arrange
        var result = Types
            .InAssemblies(ProductionAssemblies)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .ResideInNamespace("Hector.Modules")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
