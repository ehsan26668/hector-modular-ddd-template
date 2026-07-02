using FluentAssertions;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.Common;

internal static class NetArchTestExtensions
{
    public static void AssertSuccessful(this TestResult result, string because)
    {
        result.IsSuccessful.Should().BeTrue(because);
    }
}
