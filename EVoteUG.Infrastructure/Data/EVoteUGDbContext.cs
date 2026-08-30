using System.Reflection;
using EVoteUG.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Data;

public class EVoteUGDbContext : DbContext
{
    public EVoteUGDbContext(DbContextOptions<EVoteUGDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Election> Elections => Set<Election>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<VoterParticipation> VoterParticipations => Set<VoterParticipation>();
    public DbSet<CastVoteRecord> CastVoteRecords => Set<CastVoteRecord>();
    public DbSet<VoteReceipt> VoteReceipts => Set<VoteReceipt>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically register all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
