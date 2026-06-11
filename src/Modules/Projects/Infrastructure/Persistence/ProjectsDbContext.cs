using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hector.Modules.Projects.Infrastructure.Persistence;

public class ProjectsDbContext : HectorDbContext
{
    public ProjectsDbContext(
        DbContextOptions<ProjectsDbContext> options,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
        IOutboxEventSerializer outboxSerializer,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, stronglyTypedIdAssemblyProvider, outboxSerializer, domainEventDispatcher)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("projects");

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(builder =>
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
        });
    }
}
