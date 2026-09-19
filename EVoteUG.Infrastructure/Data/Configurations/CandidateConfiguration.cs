using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Nickname)
            .HasMaxLength(50);

        builder.Property(c => c.StudentId)
            .HasMaxLength(20);

        builder.Property(c => c.ManifestoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.RunningMateName)
            .HasMaxLength(150);

        builder.Property(c => c.RunningMatePhotoUrl)
            .HasMaxLength(500);

        // One Position has many Candidates (Restrict delete to preserve audit history)
        builder.HasOne(c => c.Position)
            .WithMany(p => p.Candidates)
            .HasForeignKey(c => c.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
