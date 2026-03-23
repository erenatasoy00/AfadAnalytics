using Microsoft.EntityFrameworkCore;
using AfadAnalytics.Models;

namespace AfadAnalytics.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<PropertyListing> PropertyListings { get; set; }
        public DbSet<DistrictRisk> DistrictRisks { get; set; }
    }
}