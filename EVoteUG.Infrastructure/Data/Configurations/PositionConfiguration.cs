using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        // One Election has many Positions (Cascade on delete)
        builder.HasOne(p => p.Election)
            .WithMany(e => e.Positions)
            .HasForeignKey(p => p.ElectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
