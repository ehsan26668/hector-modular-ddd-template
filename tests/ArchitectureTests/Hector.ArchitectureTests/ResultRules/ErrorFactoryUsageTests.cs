using FluentAssertions;
using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Application.Results;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class ErrorFactoryUsageTests
{
    [Fact]
    public void Should_OnlyDefineErrorsInCentralizedCatalogs_When_InApplicationLayer()
    {
        // Arrange
        var applicationAssemblies = new[]
        {
            typeof(ApplicationAssemblyMarker).Assembly,
            // در صورت نیاز اسمبلی‌های ماژول‌ها را هم اینجا اضافه کن یا از اسکنر استفاده کن
        };

        // Act
        var result = Types
            .InAssemblies(applicationAssemblies)
            .That()
            .DoNotHaveNameMatching("^Errors.*")
            .And()
            .AreNotClasses()
            .Should()
            .NotHaveDependencyOn(typeof(Error).FullName)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "direct creation or definition of 'Error' objects outside of centralized catalogs (Errors.*) is forbidden. " +
            "Please use the existing catalogs in Errors.Commands.*, Errors.Queries.* or Errors.Shared.*");
    }

    [Fact]
    public void Should_BeStaticAndPublic_When_DefiningErrorCatalogs()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(ApplicationAssemblyMarker).Assembly)
            .That()
            .HaveNameMatching("^Errors.*")
            .Should()
            .BeStatic()
            .And()
            .BePublic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Error catalogs must be public static classes to be accessible across the module.");
    }
}
