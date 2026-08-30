using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class VoterParticipationConfig : IEntityTypeConfiguration<VoterParticipation>
{
    public void Configure(EntityTypeBuilder<VoterParticipation> builder)
    {
        builder.HasKey(vp => vp.Id);

        // COMPOSITE UNIQUE INDEX: Strictly enforces 1-vote-per-student per election
        builder.HasIndex(vp => new { vp.StudentId, vp.ElectionId })
            .IsUnique();

        builder.Property(vp => vp.IpAddress)
            .HasMaxLength(45);

        builder.Property(vp => vp.DeviceInfo)
            .HasMaxLength(255);

        builder.HasOne(vp => vp.Student)
            .WithMany(s => s.Participations)
            .HasForeignKey(vp => vp.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vp => vp.Election)
            .WithMany(e => e.VoterParticipations)
            .HasForeignKey(vp => vp.ElectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
