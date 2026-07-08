namespace Hector.ArchitectureTests.Framework.Documentation;

public sealed class DocumentationRuleBuilder
{
    public AdrRuleBuilder ForADRs()
    {
        return new AdrRuleBuilder();
    }
}