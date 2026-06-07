using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Domain;

public sealed class ProjectId : StronglyTypedId<ProjectId>
{
    private ProjectId(Guid value) : base(value) { }

    /// <summary>
    /// Domain creation — only valid way to generate a new ID.
    /// </summary>
    public static ProjectId New() => CreateNew(value => new ProjectId(value));

    /// <summary>
    /// Infrastructure rehydration.
    /// </summary>
    internal static ProjectId From(Guid value) => FromExisting(value, v => new ProjectId(v));
}
