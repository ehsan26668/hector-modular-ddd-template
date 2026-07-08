using System.Reflection;
using FluentAssertions;
using Hector.ArchitectureTests.Common;
using Hector.ArchitectureTests.Framework;

namespace Hector.ArchitectureTests.LayerBoundaries;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "LayerBoundaries")]
public sealed class LayerDependencyTests : ArchitectureTestBase
{
    [Fact(DisplayName = "ADR-0002 | TC-01 | Domain layer must not depend on Application or Infrastructure layers")]
    public void Should_NotDependOnApplicationOrInfrastructure_When_ProjectIsDomainLayer()
    {
        // Arrange
        var forbiddenDependencies = ApplicationAssemblies
            .Concat(InfrastructureAssemblies)
            .Concat(ContractAssemblies)
            .Select(a => a.GetName().Name!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var ruleBuilder = ArchitectureRule
            .Types()
            .That(DomainAssemblies)
            .Should();

        foreach (var dependency in forbiddenDependencies)
        {
            ruleBuilder.NotDependOn(dependency);
        }

        var rule = ruleBuilder
            .Build("ADR-0002-TC-01", "Domain layer purity")
            .Because("domain logic must be isolated from application, infrastructure, and contract concerns.");

        // Act
        var result = rule.EvaluateWithResult();

        // Assert
        result.HasViolations.Should().BeFalse(
            $"Domain layer must not depend on Application, Infrastructure, or Contracts assemblies. " +
            $"Violations:{Environment.NewLine}{string.Join(Environment.NewLine, result.Diagnostics)}");
    }

    [Fact(DisplayName = "ADR-0002 | TC-02 | Application layers should not depend on Infrastructure")]
    public void Should_NotDependOnInfrastructure_When_ProjectIsApplicationLayer()
    {
        // Arrange
        var infrastructureAssemblyNames = InfrastructureAssemblies
            .Select(a => a.GetName().Name!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var ruleBuilder = ArchitectureRule
            .Types()
            .That(ApplicationAssemblies)
            .Should();

        foreach (var dependency in infrastructureAssemblyNames)
        {
            ruleBuilder.NotDependOn(dependency);
        }

        var rule = ruleBuilder
            .Build("ADR-0002-TC-02", "Application layer isolation")
            .Because("application logic must be decoupled from concrete infrastructure implementations.");

        // Act
        var result = rule.EvaluateWithResult();

        // Assert
        result.HasViolations.Should().BeFalse(
            $"Application layer must not depend on Infrastructure assemblies. " +
            $"Violations:{Environment.NewLine}{string.Join(Environment.NewLine, result.Diagnostics)}");
    }

    [Fact(DisplayName = "ADR-0002 | TC-03 | Infrastructure layer projects must depend on Domain and Application layers")]
    public void Should_DependOnDomainAndApplication_When_ProjectIsInfrastructureLayer()
    {
        // Arrange
        var rule = new ArchitectureRule(
            id: "ADR-0002-TC-03",
            name: "Infrastructure layer dependency direction",
            because: "Infrastructure is the outer layer and must reference the corresponding Domain and Application projects to implement technical concerns.",
            evaluator: EvaluateInfrastructureCoreDependencies);

        // Act
        var result = rule.EvaluateWithResult();

        // Assert
        result.HasViolations.Should().BeFalse(
            $"Infrastructure projects must depend on their corresponding Domain and Application projects. Violations:{Environment.NewLine}{string.Join(Environment.NewLine, result.Diagnostics)}");
    }

    private EvaluationResult EvaluateInfrastructureCoreDependencies()
    {
        var violations = new List<string>();

        foreach (var infraAssembly in InfrastructureAssemblies)
        {
            var infraName = infraAssembly.GetName().Name ?? string.Empty;
            var referencedAssemblies = infraAssembly
                .GetReferencedAssemblies()
                .Select(a => a.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToHashSet(StringComparer.Ordinal);

            var expected = ResolveExpectedCoreDependencies(infraAssembly);

            if (!referencedAssemblies.Contains(expected.DomainAssemblyName))
                violations.Add($"{infraName} does not reference expected Domain assembly: {expected.DomainAssemblyName}");

            if (!referencedAssemblies.Contains(expected.ApplicationAssemblyName))
                violations.Add($"{infraName} does not reference expected Application assembly: {expected.ApplicationAssemblyName}");
        }

        return violations.Count == 0 ? EvaluationResult.Success() : EvaluationResult.Failure(violations);
    }

    private static ExpectedCoreDependencies ResolveExpectedCoreDependencies(Assembly infraAssembly)
    {
        var name = infraAssembly.GetName().Name ?? throw new InvalidOperationException();
        if (name.Equals("Hector.BuildingBlocks.Persistence", StringComparison.Ordinal))
            return new ExpectedCoreDependencies("Hector.BuildingBlocks.Domain", "Hector.BuildingBlocks.Application");

        var moduleBase = name.Replace(".Infrastructure", "", StringComparison.Ordinal);
        return new ExpectedCoreDependencies($"{moduleBase}.Domain", $"{moduleBase}.Application");
    }

    private sealed record ExpectedCoreDependencies(string DomainAssemblyName, string ApplicationAssemblyName);
}
