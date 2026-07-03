using Hector.ArchitectureTests.Framework.Dsl;

namespace Hector.ArchitectureTests.Framework;

public partial class ArchitectureRule
{
    public static ITypesSelection Types() => new TypesSelection();
}