using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.EntityType)
            .HasMaxLength(50);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);

        builder.HasIndex(a => a.Timestamp);

        builder.HasOne(a => a.Admin)
            .WithMany(adm => adm.AuditLogs)
            .HasForeignKey(a => a.AdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
