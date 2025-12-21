using Microsoft.EntityFrameworkCore;

namespace Smpsp.Server.Data
{
    public class AppDbContext(PathService ps) : DbContext
    {
        public DbSet<DataRecord> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Path.Join(ps.BasePath, "app.db")}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataRecord>().HasKey(x => x.Id);
            modelBuilder.Entity<DataRecord>().HasIndex(x => new { x.RecordType, x.UserId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
