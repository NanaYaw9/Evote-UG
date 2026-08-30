using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EVoteUG.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StudentId)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.PasswordHash)
            .IsRequired();

        builder.Property(s => s.College)
            .HasMaxLength(100);

        builder.Property(s => s.Faculty)
            .HasMaxLength(100);

        builder.Property(s => s.Department)
            .HasMaxLength(100);

        builder.Property(s => s.HallOfResidence)
            .HasMaxLength(100);

        // Unique indexes
        builder.HasIndex(s => s.StudentId)
            .IsUnique();

        builder.HasIndex(s => s.Email)
            .IsUnique();
    }
}
