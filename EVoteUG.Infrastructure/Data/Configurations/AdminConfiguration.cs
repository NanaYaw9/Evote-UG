using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.PasswordHash)
            .IsRequired();

        // Unique indexes
        builder.HasIndex(a => a.Username)
            .IsUnique();

        builder.HasIndex(a => a.Email)
            .IsUnique();
    }
}
