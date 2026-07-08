using System.Text.RegularExpressions;

namespace Hector.ArchitectureTests.Framework.Documentation;

internal sealed class AdrMandatorySectionsRule : IAdrRule
{
    private readonly string[] _mandatorySections;
    private readonly Dictionary<string, Regex> _sectionPatterns;

    public AdrMandatorySectionsRule(string[] mandatorySections)
    {
        _mandatorySections = mandatorySections;
        _sectionPatterns = mandatorySections.ToDictionary(
            section => section,
            section => new Regex($"^##\\s+{section}\\s*$", RegexOptions.Multiline | RegexOptions.Compiled));
    }

    public EvaluationResult Evaluate(string[] adrFilePaths)
    {
        var filesWithMissingSections = adrFilePaths
            .Select(filePath =>
            {
                var content = File.ReadAllText(filePath);
                var missingSections = _mandatorySections
                    .Where(section => !_sectionPatterns[section].IsMatch(content))
                    .ToList();
                return new { FileName = Path.GetFileName(filePath), Missing = missingSections };
            })
            .Where(result => result.Missing.Any())
            .ToList();

        if (!filesWithMissingSections.Any())
        {
            return EvaluationResult.Success();
        }

        var errorDetails = filesWithMissingSections
            .Select(result => $"{result.FileName} is missing sections: [{string.Join(", ", result.Missing)}]");

        var errorMessage = $"Every ADR file must contain all mandatory sections. Violations found:\n  {string.Join("\n  ", errorDetails)}";
        return EvaluationResult.Failure([errorMessage]);
    }
}