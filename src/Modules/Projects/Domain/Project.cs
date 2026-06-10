using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Domain;

public sealed class Project : AggregateRoot<ProjectId>
{
    public string Name { get; private set; }

    private Project(ProjectId id, string name)
        : base(Ensure.NotDefault(id))
    {
        Name = Ensure.NotEmpty(name);

        RaiseDomainEvent(new ProjectCreatedDomainEvent(id, name));
    }

    public static Project Create(string name)
    {
        Ensure.NotEmpty(name);

        return new Project(ProjectId.New(), name);
    }
}
