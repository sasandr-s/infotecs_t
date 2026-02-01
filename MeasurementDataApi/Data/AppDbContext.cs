using Microsoft.EntityFrameworkCore;
using MeasurementDataApi.Models;

namespace MeasurementDataApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ValueRecord> Values { get; set; }
    public DbSet<ResultRecord> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Optimizing for query speed on common filters
        
        modelBuilder.Entity<ResultRecord>()
            .HasIndex(r => r.FileName);

        modelBuilder.Entity<ResultRecord>()
            .HasIndex(r => r.MinDate);
            
        modelBuilder.Entity<ResultRecord>()
            .HasIndex(r => r.AvgValue);
            
        modelBuilder.Entity<ResultRecord>()
            .HasIndex(r => r.AvgExecutionTime);

        modelBuilder.Entity<ValueRecord>()
            .HasIndex(v => v.FileName);

        modelBuilder.Entity<ValueRecord>()
            .HasIndex(v => v.Date);
    }
}
