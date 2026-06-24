using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class CommandHandlerResultRuleTests
{
    [Fact]
    public void CommandHandlers_Should_Reside_In_ApplicationLayer()
    {
        // Arrange
        var result = Types.InCurrentDomain()
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .ResideInNamespace("Hector.Modules")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}