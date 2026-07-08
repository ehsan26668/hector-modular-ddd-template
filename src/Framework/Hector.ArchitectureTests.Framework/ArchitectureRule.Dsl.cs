using Hector.ArchitectureTests.Framework.Documentation;
using Hector.ArchitectureTests.Framework.Dsl;

namespace Hector.ArchitectureTests.Framework;

public partial class ArchitectureRule
{
    public static ITypesSelection Types() => new TypesSelection();

    public static IModuleBoundarySelection Modules() => new ModuleBoundarySelection();

    public static DocumentationRuleBuilder ForDocumentation() => new();
}