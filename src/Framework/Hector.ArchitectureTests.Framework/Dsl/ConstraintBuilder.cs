using NetArchTest.Rules;

namespace Hector.ArchitectureTests.Framework.Dsl;

internal sealed class ConstraintBuilder(
    Types types,
    IReadOnlyList<string> namespaces)
    : IConstraintBuilder
{
    private readonly List<string> _forbiddenDependencies = [];

    public IConstraintBuilder NotDependOn(string @namespace)
    {
        _forbiddenDependencies.Add(@namespace);
        return this;
    }

    public ArchitectureRule Build(string id, string name)
    {
        return new ArchitectureRule(
            id,
            name,
            string.Empty,
            () =>
            {
                var diagnostics = new List<string>();

                PredicateList predicates = BuildPredicateList();

                foreach (var dependency in _forbiddenDependencies.OrderBy(d => d, StringComparer.Ordinal))
                {
                    var result = predicates
                        .Should()
                        .NotHaveDependencyOn(dependency)
                        .GetResult();

                    if (!result.IsSuccessful && result.FailingTypes != null)
                    {
                        foreach (var ft in result.FailingTypes.OrderBy(t => t.FullName, StringComparer.Ordinal))
                        {
                            diagnostics.Add(
                                $"Type '{ft.FullName}' must not depend on '{dependency}'."
                            );
                        }
                    }
                }

                return diagnostics.Count == 0
                    ? EvaluationResult.Success()
                    : EvaluationResult.Failure(diagnostics);
            });
    }

    private PredicateList BuildPredicateList()
    {
        var predicates = types.That();

        if (namespaces.Count == 0)
        {
            return predicates.HaveNameStartingWith(string.Empty);
        }

        var list = predicates.ResideInNamespace(namespaces[0]);

        for (int i = 1; i < namespaces.Count; i++)
        {
            list = list.Or().ResideInNamespace(namespaces[i]);
        }

        return list;
    }
}
