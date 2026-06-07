using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Domain;

public sealed class Project : AggregateRoot<ProjectId>
{
    public string Name { get; private set; }

    private Project(ProjectId id, string name) : base(id)
    {
        Name = name;

        RaiseDomainEvent(new ProjectCreatedDomainEvent(id, name));
    }

    public static Project Create(string name)
    {
        Ensure.NotEmpty(name, "Project name cannot be empty");

        var id = ProjectId.New();

        return new Project(id, name);
    }
}
