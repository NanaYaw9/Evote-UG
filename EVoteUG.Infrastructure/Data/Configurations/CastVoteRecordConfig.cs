using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class CastVoteRecordConfig : IEntityTypeConfiguration<CastVoteRecord>
{
    public void Configure(EntityTypeBuilder<CastVoteRecord> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.BallotBatchId)
            .HasMaxLength(50);

        builder.HasIndex(v => v.ElectionId);
        builder.HasIndex(v => v.PositionId);
        builder.HasIndex(v => v.CandidateId);

        builder.HasOne(v => v.Position)
            .WithMany()
            .HasForeignKey(v => v.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Candidate)
            .WithMany()
            .HasForeignKey(v => v.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
