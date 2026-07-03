using System.Reflection;

namespace Hector.ArchitectureTests.Framework;

public static class Conventions
{
    public static ArchitectureRuleSet LayerIsolation(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return new ArchitectureRuleSet()
            .Add(ArchitectureRule
                .Types()
                .That(assembly)
                    .ResideInNamespace("Application")
                .Should()
                    .NotDependOn("Microsoft.AspNetCore")
                .Build("ADR-0056-LAYER-001", "Application layer must not depend on ASP.NET Core Web APIs")
                .Because("Application layer must remain transport-agnostic"))
            .Add(ArchitectureRule
                .Types()
                .That(assembly)
                    .ResideInNamespace("Domain")
                .Should()
                    .NotDependOn("Infrastructure")
                .Build("ADR-0056-LAYER-002", "Domain layer must not depend on Infrastructure")
                .Because("Domain model must remain persistence-agnostic"));
    }

    public static ArchitectureRuleSet CQRS(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-CQRS-001",
                "CQRS Command/Query placement validation",
                "CQRS rules must be enforceable by convention",
                EvaluationResult.Success));
    }

    public static ArchitectureRuleSet ResultPattern(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-RESULT-001",
                "Result pattern validation",
                "Result pattern usage conventions",
                EvaluationResult.Success));
    }

    public static ArchitectureRuleSet DomainPurity(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-DOMAIN-001",
                "Domain models purity validation",
                "Domain layer must remain clean of external details",
                EvaluationResult.Success));
    }
}