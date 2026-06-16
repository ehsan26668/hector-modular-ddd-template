using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using Hector.Modules.Projects.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hector.Modules.Projects.Infrastructure.Persistence;

public sealed class ProjectsDbContext(
    DbContextOptions<ProjectsDbContext> options,
    IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
    IDomainEventDispatcher domainEventDispatcher)
    : HectorDbContext(options, stronglyTypedIdAssemblyProvider, domainEventDispatcher)
{
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>().HasKey(project => project.Id);
    }
}
