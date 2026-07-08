using Hector.ArchitectureTests.Framework;

namespace Hector.ArchitectureTests.Documentation;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0001")]
[Trait("Validation", "Documentation")]
public sealed class AdrStructureTests
{
    [Fact(DisplayName = "ADR-0001 | TC-01..03 | Documentation files should adhere to defined structure")]
    public void Should_AdhereToDefinedStructure_When_EvaluatingAdrFiles()
    {
        // Arrange, Act & Assert
        ArchitectureRule
            .ForDocumentation()
            .ForADRs()
            .InDirectory("docs/adr")
            .Should()
                // TC-01: ADR files must follow the '{number}-{kebab-case-title}.md' naming convention
                .FollowNamingConvention()
            .And()
                // TC-02: Every ADR file must contain the mandatory sections
                .ContainMandatorySections("Status", "Context", "Decision", "Consequences")
            .And()
                // TC-03: ADR numbers must be unique and sequential
                .HaveUniqueAndSequentialNumbers(except: 42) // ADR-0042 is intentionally reserved
            .Check();
    }
}
