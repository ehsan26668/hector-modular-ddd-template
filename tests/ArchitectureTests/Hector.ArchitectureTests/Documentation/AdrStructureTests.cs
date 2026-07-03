using System.Text.RegularExpressions;
using FluentAssertions;

namespace Hector.ArchitectureTests.Documentation;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0001")]
[Trait("Validation", "Documentation")]
public sealed class AdrStructureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string AdrDirectory = Path.Combine(RepositoryRoot, "docs", "adr");

    private static readonly Regex AdrFileNamePattern =
        new(@"^\d{4}-[a-z0-9-]+\.md$", RegexOptions.Compiled);

    private static readonly Regex NumberPattern =
        new(@"^(\d{4})-", RegexOptions.Compiled);

    private static readonly Regex StatusHeaderPattern =
        new(@"^##\s+Status\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex ContextHeaderPattern =
        new(@"^##\s+Context\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex DecisionHeaderPattern =
        new(@"^##\s+Decision\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex ConsequencesHeaderPattern =
        new(@"^##\s+Consequences\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    // ADR-0042 is intentionally reserved.
    // No architectural decision exists yet for this slot.
    // The next real ADR should use this number.
    private static readonly int[] IntentionallyReservedAdrNumbers = [42];

    [Fact(DisplayName = "ADR-0001 | TC-01 | ADR files should follow naming convention")]
    public void Should_FollowNamingConvention_When_ADRFileIsCreated()
    {
        // Arrange
        var adrFiles = Directory.GetFiles(AdrDirectory, "*.md", SearchOption.TopDirectoryOnly);

        // Act
        var invalidFileNames = adrFiles
            .Select(Path.GetFileName)
            .Where(fileName => fileName is not null && !AdrFileNamePattern.IsMatch(fileName))
            .ToArray();

        // Assert
        invalidFileNames.Should().BeEmpty(
            "ADR files must follow the '{number}-{kebab-case-title}.md' naming convention");
    }

    [Fact(DisplayName = "ADR-0001 | TC-02 | ADR files should contain mandatory sections")]
    public void Should_ContainMandatorySections_When_ADRFileExists()
    {
        // Arrange
        var adrFiles = Directory.GetFiles(AdrDirectory, "*.md", SearchOption.TopDirectoryOnly);

        // Act
        var filesWithMissingSections = adrFiles
            .Select(filePath =>
            {
                var content = File.ReadAllText(filePath);
                var missingSections = new List<string>();

                if (!StatusHeaderPattern.IsMatch(content))
                {
                    missingSections.Add("Status");
                }

                if (!ContextHeaderPattern.IsMatch(content))
                {
                    missingSections.Add("Context");
                }

                if (!DecisionHeaderPattern.IsMatch(content))
                {
                    missingSections.Add("Decision");
                }

                if (!ConsequencesHeaderPattern.IsMatch(content))
                {
                    missingSections.Add("Consequences");
                }

                return new
                {
                    FileName = Path.GetFileName(filePath),
                    MissingSections = missingSections
                };
            })
            .Where(result => result.MissingSections.Count > 0)
            .Select(result => $"{result.FileName}: missing [{string.Join(", ", result.MissingSections)}]")
            .ToArray();

        // Assert
        filesWithMissingSections.Should().BeEmpty(
            "every ADR file must contain the mandatory sections '## Status', '## Context', '## Decision', and '## Consequences'");
    }

    [Fact(DisplayName = "ADR-0001 | TC-03 | ADR numbers should be unique and sequential")]
    public void Should_HaveUniqueAndSequentialNumbers_When_ScanningADRDirectory()
    {
        // Arrange
        var adrFiles = Directory.GetFiles(AdrDirectory, "*.md", SearchOption.TopDirectoryOnly);

        var numbers = adrFiles
            .Select(Path.GetFileNameWithoutExtension)
            .Select(fileName => NumberPattern.Match(fileName!))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value))
            .OrderBy(number => number)
            .ToArray();

        // Act
        var duplicateNumbers = numbers
            .GroupBy(number => number)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        var minNumber = numbers.Min();
        var maxNumber = numbers.Max();

        var missingNumbers = Enumerable.Range(minNumber, maxNumber - minNumber + 1)
            .Except(numbers)
            .ToArray();

        var undocumentedMissingNumbers = missingNumbers
            .Except(IntentionallyReservedAdrNumbers)
            .ToArray();

        // Assert
        duplicateNumbers.Should().BeEmpty("ADR numbers must be unique");

        undocumentedMissingNumbers.Should().BeEmpty(
            "ADR numbers must be sequential without undocumented gaps");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionFile = Path.Combine(directory.FullName, "Hector.slnx");

            if (File.Exists(solutionFile))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing Hector.slnx.");
    }
}
