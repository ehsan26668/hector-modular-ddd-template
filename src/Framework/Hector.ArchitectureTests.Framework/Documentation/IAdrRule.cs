namespace Hector.ArchitectureTests.Framework.Documentation;

internal interface IAdrRule
{
    EvaluationResult Evaluate(string[] adrFilePaths);
}