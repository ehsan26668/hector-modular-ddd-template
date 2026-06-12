namespace Hector.Modules.Projects.Infrastructure.Persistence;

using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Domain;
using Microsoft.EntityFrameworkCore;

public sealed class ProjectsDbContext(
    DbContextOptions<ProjectsDbContext> options,
    IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
    IOutboxEventSerializer outboxSerializer)
    : HectorDbContext(options, stronglyTypedIdAssemblyProvider, outboxSerializer)
{
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>().HasKey(project => project.Id);
    }
}
