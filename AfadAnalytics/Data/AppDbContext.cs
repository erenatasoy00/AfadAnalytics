using Microsoft.EntityFrameworkCore;
using AfadAnalytics.Models;

namespace AfadAnalytics.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
 
        public DbSet<PropertyListing> PropertyListings { get; set; }
        public DbSet<DistrictRisk> DistrictRisks { get; set; }
        public DbSet<EriScore> EriScores { get; set; }
        public DbSet<ProvinceMetadata> ProvinceMetadata { get; set; }
 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EriScore>()
                .HasKey(e => new { e.Province, e.ScenarioId });
        }
    }
}
