using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hector.BuildingBlocks.Persistence.Inbox;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId)
            .IsRequired();

        builder.Property(x => x.Consumer)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ProcessedOn)
            .IsRequired();

        builder.HasIndex(x => new { x.MessageId, x.Consumer })
            .IsUnique();
    }
}