namespace Hector.ArchitectureTests.Framework.Dsl;

public interface IConstraintBuilder
{
    IConstraintBuilder NotDependOn(string @namespace);
    ArchitectureRule Build(string id, string name);
}