using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zempler.Ticketing.Domain.Entities;

namespace Zempler.Ticketing.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.SeatNumber)
            .IsRequired()
            .HasMaxLength(20);

        // Standard decimal configuration for monetary values
        builder.Property(t => t.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.HolderName)
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .IsRequired();

        // Ignore computed properties so EF Core doesn't try to create a DB column for it
        builder.Ignore(t => t.ReservedUntil);

        // Optimistic Concurrency Token Setup
        builder.Property(t => t.RowVersion)
            .IsConcurrencyToken()
            .IsRequired();
    }
}