using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class VoteReceiptConfiguration : IEntityTypeConfiguration<VoteReceipt>
{
    public void Configure(EntityTypeBuilder<VoteReceipt> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReceiptHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(r => r.ReceiptHash)
            .IsUnique();

        builder.HasOne(r => r.Student)
            .WithMany(s => s.Receipts)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Election)
            .WithMany()
            .HasForeignKey(r => r.ElectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
