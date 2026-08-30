using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class ElectionConfiguration : IEntityTypeConfiguration<Election>
{
    public void Configure(EntityTypeBuilder<Election> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.AcademicYear)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.ScopeTarget)
            .HasMaxLength(100);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.StartDate, e.EndDate });
    }
}
