using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.Modules.Projects.Application;

public sealed class ProjectsModuleIdentity : IModuleIdentity
{
    public string ModuleName => "projects";
}