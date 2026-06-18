using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class CommandHandlerArchitectureTests
{
    [Fact]
    public void CommandHandlers_Should_DependOnTheirModuleDomainLayer()
    {
        // Arrange
        var applicationAssemblies = LoadApplicationAssemblies();

        // Act
        var failures = applicationAssemblies
            .SelectMany(FindCommandHandlersWithoutDomainDependency)
            .OrderBy(static failure => failure)
            .ToList();

        // Assert
        applicationAssemblies.Should().NotBeEmpty(
            "architecture tests must discover at least one module Application assembly");

        failures.Should().BeEmpty(
            "command handlers must depend on their module domain layer.{0}{1}",
            Environment.NewLine,
            FormatFailures(failures));
    }

    [Fact]
    public void CommandHandlers_Should_NotDependOnTheirModuleInfrastructureLayer()
    {
        // Arrange
        var applicationAssemblies = LoadApplicationAssemblies();

        // Act
        var failures = applicationAssemblies
            .SelectMany(FindCommandHandlersDependingOnInfrastructure)
            .OrderBy(static failure => failure)
            .ToList();

        // Assert
        applicationAssemblies.Should().NotBeEmpty(
            "architecture tests must discover at least one module Application assembly");

        failures.Should().BeEmpty(
            "command handlers must not depend on their module infrastructure layer.{0}{1}",
            Environment.NewLine,
            FormatFailures(failures));
    }

    private static IEnumerable<string> FindCommandHandlersWithoutDomainDependency(Assembly applicationAssembly)
    {
        var domainAssemblyName = GetSiblingAssemblyName(
            applicationAssembly,
            sourceLayer: "Application",
            targetLayer: "Domain");

        var result = Types.InAssembly(applicationAssembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .HaveDependencyOn(domainAssemblyName)
            .GetResult();

        return ToFailureMessages(
            result,
            failureReason: $"expected dependency on {domainAssemblyName}");
    }

    private static IEnumerable<string> FindCommandHandlersDependingOnInfrastructure(Assembly applicationAssembly)
    {
        var infrastructureAssemblyName = GetSiblingAssemblyName(
            applicationAssembly,
            sourceLayer: "Application",
            targetLayer: "Infrastructure");

        var result = Types.InAssembly(applicationAssembly)
            .That()
            .AreClasses()
            .And()
            .HaveNameEndingWith("CommandHandler")
            .ShouldNot()
            .HaveDependencyOn(infrastructureAssemblyName)
            .GetResult();

        return ToFailureMessages(
            result,
            failureReason: $"must not depend on {infrastructureAssemblyName}");
    }

    private static IReadOnlyList<Assembly> LoadApplicationAssemblies()
    {
        return Directory
            .EnumerateFiles(
                AppContext.BaseDirectory,
                "Hector.Modules.*.Application.dll",
                SearchOption.TopDirectoryOnly)
            .OrderBy(static path => path)
            .Select(Assembly.LoadFrom)
            .ToList();
    }

    private static string GetSiblingAssemblyName(
        Assembly assembly,
        string sourceLayer,
        string targetLayer)
    {
        var assemblyName = assembly.GetName().Name!;

        return assemblyName.Replace(
            $".{sourceLayer}",
            $".{targetLayer}",
            StringComparison.Ordinal);
    }

    private static IEnumerable<string> ToFailureMessages(
        TestResult result,
        string failureReason)
    {
        return (result.FailingTypes ?? [])
            .Select(type => $"{type.FullName} -> {failureReason}")
            .Distinct();
    }

    private static string FormatFailures(IReadOnlyCollection<string> failures)
    {
        return failures.Count == 0
            ? string.Empty
            : string.Join(
                Environment.NewLine,
                failures.Select(static failure => $"  - {failure}"));
    }
}
