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
    public DbSet<User> Users { get; set; }
    public DbSet<Child> Children { get; set; }
    public DbSet<Invitation> Invitations { get; set; }

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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configuration configuration
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.ToTable("Configurations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
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

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GoogleId).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.GoogleId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Child configuration
        modelBuilder.Entity<Child>(entity =>
        {
            entity.ToTable("Children");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.PrimaryParent)
                .WithMany(u => u.Children)
                .HasForeignKey(e => e.PrimaryParentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.SecondaryParent)
                .WithMany()
                .HasForeignKey(e => e.SecondaryParentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Invitation configuration
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.ToTable("Invitations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ChildId, e.InviteeEmail });
            entity.Property(e => e.InviteeEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.Inviter)
                .WithMany(u => u.SentInvitations)
                .HasForeignKey(e => e.InviterId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Child)
                .WithMany()
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
