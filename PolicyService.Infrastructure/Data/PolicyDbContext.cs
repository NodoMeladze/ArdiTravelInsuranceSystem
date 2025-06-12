using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Entities;
using PolicyService.Infrastructure.Data.Configurations;

namespace PolicyService.Infrastructure.Data
{
    public class PolicyDbContext(DbContextOptions<PolicyDbContext> options) : DbContext(options)
    {
        public DbSet<Policy> Policies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new PolicyConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback for demo purposes
                optionsBuilder.UseInMemoryDatabase("PolicyDb");
            }

            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }
}