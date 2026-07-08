using System.Text.RegularExpressions;

namespace Hector.ArchitectureTests.Framework.Documentation;

internal sealed class AdrNumberingRule : IAdrRule
{
    private static readonly Regex NumberPattern = new(@"^(\d{4})-", RegexOptions.Compiled);
    private readonly HashSet<int> _intentionallyReservedNumbers; //  <-- ۱. تعریف صریح فیلد

    public AdrNumberingRule(int[] intentionallyReservedNumbers)
    {
        _intentionallyReservedNumbers = [.. intentionallyReservedNumbers];
    }

    public EvaluationResult Evaluate(string[] adrFilePaths)
    {
        var numbers = adrFilePaths
            .Select(Path.GetFileNameWithoutExtension)
            .Select(fileName => NumberPattern.Match(fileName!))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value))
            .OrderBy(number => number)
            .ToArray();

        if (numbers.Length < 2)
        {
            return EvaluationResult.Success();
        }

        var errors = new List<string>();

        var duplicateNumbers = numbers.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateNumbers.Count != 0)
        {
            errors.Add($"ADR numbers must be unique. Duplicates found: {string.Join(", ", duplicateNumbers)}");
        }

        var missingNumbers = new List<int>();
        for (int i = 0; i < numbers.Length - 1; i++)
        {
            int current = numbers[i];
            int next = numbers[i + 1];

            if (next - current > 1)
            {
                for (int missing = current + 1; missing < next; missing++)
                {
                    if (!_intentionallyReservedNumbers.Contains(missing))
                    {
                        missingNumbers.Add(missing);
                    }
                }
            }
        }

        if (missingNumbers.Count != 0)
        {
            errors.Add($"ADR numbers must be sequential. Undocumented gaps found for numbers: {string.Join(", ", missingNumbers)}");
        }

        return errors.Count != 0 ? EvaluationResult.Failure(errors.ToArray()) : EvaluationResult.Success();
    }
}
