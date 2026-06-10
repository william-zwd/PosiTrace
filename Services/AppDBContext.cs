using Microsoft.EntityFrameworkCore;
using PosiTrace.Models;

namespace PosiTrace.Services
{
    public class AppDBContext : DbContext
    {
        public DbSet<StreetAddress> StreetAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StreetAddress>(entity =>
            {
                // Set the string field as the primary key
                entity.HasKey(e => e.Address);

                // Tell EF Core that the database does not auto-generate this string
                entity.Property(e => e.Address)
                      .ValueGeneratedNever();
            });
        }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }
    }

}
