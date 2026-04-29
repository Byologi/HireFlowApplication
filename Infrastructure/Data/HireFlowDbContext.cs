using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Data
{
    public class HireFlowDbContext : DbContext
    {
        public HireFlowDbContext(DbContextOptions<HireFlowDbContext> options)
            : base(options)
        {
        }

        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationNote> ApplicationNotes { get; set; }
        public DbSet<StageHistory> StageHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint: one application per email per job
            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.JobId, a.CandidateEmail })
                .IsUnique();

            // Relationships (optional but clean)
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Applications)
                .WithOne(a => a.Job)
                .HasForeignKey(a => a.JobId);

            modelBuilder.Entity<Application>()
                .HasMany<ApplicationNote>()
                .WithOne(n => n.Application)
                .HasForeignKey(n => n.ApplicationId);

            modelBuilder.Entity<Application>()
                .HasMany<StageHistory>()
                .WithOne(s => s.Application)
                .HasForeignKey(s => s.ApplicationId);
        }
    }
}