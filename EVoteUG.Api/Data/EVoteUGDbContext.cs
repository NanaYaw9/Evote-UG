using Microsoft.EntityFrameworkCore;
using EVoteUG.Shared.Models;

namespace EVoteUG.Api.Data;

public class EVoteUGDbContext : DbContext
{
    public EVoteUGDbContext(DbContextOptions<EVoteUGDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Election> Elections { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Enforce: a Student can only vote once per Position
    modelBuilder.Entity<Vote>()
        .HasIndex(v => new { v.StudentId, v.PositionId })
        .IsUnique();

    // Avoid multiple cascade paths: only Student→Vote cascades on delete.
    // Candidate and Position relationships won't auto-delete Votes.
    modelBuilder.Entity<Vote>()
        .HasOne(v => v.Candidate)
        .WithMany(c => c.Votes)
        .HasForeignKey(v => v.CandidateId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Vote>()
        .HasOne(v => v.Position)
        .WithMany()
        .HasForeignKey(v => v.PositionId)
        .OnDelete(DeleteBehavior.Restrict);
}
}