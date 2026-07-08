using Xunit;

namespace Hector.ArchitectureTests.Framework.Documentation;

public sealed class AdrRuleBuilder
{
    private readonly string _repositoryRoot;
    private readonly List<IAdrRule> _rules = [];
    private string? _adrDirectory;

    internal AdrRuleBuilder()
    {
        _repositoryRoot = FindRepositoryRoot();
    }

    public AdrRuleBuilder InDirectory(string relativePath)
    {
        _adrDirectory = Path.Combine(_repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return this;
    }

    public AdrRuleBuilder Should() => this;

    public AdrRuleBuilder And() => this;

    public AdrRuleBuilder FollowNamingConvention()
    {
        _rules.Add(new AdrNamingConventionRule());
        return this;
    }

    public AdrRuleBuilder ContainMandatorySections(params string[] sections)
    {
        _rules.Add(new AdrMandatorySectionsRule(sections));
        return this;
    }

    public AdrRuleBuilder HaveUniqueAndSequentialNumbers(params int[] except)
    {
        _rules.Add(new AdrNumberingRule(except));
        return this;
    }

    public void Check()
    {
        if (_adrDirectory is null)
        {
            Assert.Fail("ADR directory was not specified. Use .InDirectory() to set the path.");
            return;
        }

        if (!Directory.Exists(_adrDirectory))
        {
            Assert.Fail($"The specified ADR directory does not exist: '{_adrDirectory}'");
            return;
        }

        var adrFilePaths = Directory.GetFiles(_adrDirectory, "*.md", SearchOption.TopDirectoryOnly);
        var errors = new List<string>();

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(adrFilePaths);
            if (result.HasViolations)
            {
                errors.AddRange(result.Diagnostics);
            }
        }

        if (errors.Count != 0)
        {
            var aggregateErrorMessage = string.Join(Environment.NewLine + "- ", errors);
            Assert.Fail("ADR structure validation failed with the following errors:" + Environment.NewLine + "- " + aggregateErrorMessage);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Hector.slnx")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Could not locate repository root containing Hector.slnx.");
    }
}