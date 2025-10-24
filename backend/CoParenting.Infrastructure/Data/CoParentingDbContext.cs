using CoParenting.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for the Co-Parenting Calendar application
/// </summary>
public class CoParentingDbContext : DbContext
{
    public CoParentingDbContext(DbContextOptions<CoParentingDbContext> options)
        : base(options)
    {
    }

    public DbSet<DayAssignment> DayAssignments { get; set; }
    public DbSet<Configuration> Configurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DayAssignment configuration
        modelBuilder.Entity<DayAssignment>(entity =>
        {
            entity.ToTable("DayAssignments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date).IsUnique();
            entity.Property(e => e.Parent).HasMaxLength(1);
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configuration configuration
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.ToTable("Configurations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Seed default configuration
        modelBuilder.Entity<Configuration>().HasData(
            new Configuration 
            { 
                Id = 1, 
                Key = "ParentAName", 
                Value = "Parent A", 
                CreatedAt = DateTime.UtcNow 
            },
            new Configuration 
            { 
                Id = 2, 
                Key = "ParentBName", 
                Value = "Parent B", 
                CreatedAt = DateTime.UtcNow 
            }
        );
    }
}
