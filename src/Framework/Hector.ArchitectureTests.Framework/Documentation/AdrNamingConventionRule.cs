using System.Text.RegularExpressions;

namespace Hector.ArchitectureTests.Framework.Documentation;

internal sealed class AdrNamingConventionRule : IAdrRule
{
    private static readonly Regex AdrFileNamePattern = new(@"^\d{4}-[a-z0-9-]+\.md$", RegexOptions.Compiled);

    public EvaluationResult Evaluate(string[] adrFilePaths)
    {
        var invalidFileNames = adrFilePaths
            .Select(Path.GetFileName)
            .Where(fileName => fileName is not null && !AdrFileNamePattern.IsMatch(fileName!))
            .ToList();

        if (invalidFileNames.Count == 0)
        {
            return EvaluationResult.Success();
        }

        var errorMessage = $"The following ADR files do not follow the '{{number}}-{{kebab-case-title}}.md' naming convention: {string.Join(", ", invalidFileNames)}";
        return EvaluationResult.Failure([errorMessage]);
    }
}