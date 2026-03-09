using Microsoft.EntityFrameworkCore;
using examencsharp.Models;
using examencsharp.Models.Views;

namespace examencsharp
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Fokontany> Fokontany { get; set; }

        public DbSet<Citoyen> Citoyens { get; set; }

        public DbSet<CinRequest> CinRequests { get; set; }

        public DbSet<Election> Elections { get; set; }

        public DbSet<Candidat> Candidats { get; set; }

        public DbSet<Vote> Votes { get; set; }

        public DbSet<VoteLog> VoteLogs { get; set; }

        public DbSet<VCandidate> VCandidates { get; set; }

        public DbSet<VResult> VResults { get; set; }

        public DbSet<VTotalVoters> VTotalVoters { get; set; }

        public DbSet<VParticipationRate> VParticipationRates { get; set; }

        public DbSet<VDashboard> VDashboard { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VCandidate>().ToView("V_candidates").HasNoKey();

            modelBuilder.Entity<VResult>().ToView("V_results").HasNoKey();

            modelBuilder.Entity<VTotalVoters>().ToView("V_total_voters").HasNoKey();

            modelBuilder.Entity<VParticipationRate>().ToView("V_participation_rate").HasNoKey();

            modelBuilder.Entity<VDashboard>().ToView("V_dashboard").HasNoKey();
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}