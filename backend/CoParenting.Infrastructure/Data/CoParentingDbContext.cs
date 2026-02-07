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
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<MagicLinkToken> MagicLinkTokens { get; set; }
    public DbSet<ChangeRequest> ChangeRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DayAssignment configuration
        modelBuilder.Entity<DayAssignment>(entity =>
        {
            entity.ToTable("DayAssignments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date).IsUnique();
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Parent).HasMaxLength(1);
            entity.Property(e => e.SpecialStatus).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationship to Comments
            entity.HasMany(e => e.Comments)
                .WithOne(c => c.DayAssignment)
                .HasForeignKey(c => c.DayAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Comment configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DayAssignmentId);
            entity.HasIndex(e => e.Parent);
            entity.Property(e => e.Parent).HasMaxLength(1).IsRequired();
            entity.Property(e => e.CommentText).HasMaxLength(1000).IsRequired();
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
                Value = "Tomas",
                CreatedAt = DateTime.UtcNow
            },
            new Configuration
            {
                Id = 2,
                Key = "ParentBName",
                Value = "Carro",
                CreatedAt = DateTime.UtcNow
            }
        );

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DisplayName).HasColumnName("displayname").HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastLoginAt).HasColumnName("lastloginat");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(512).IsRequired();
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ExpiresAt).HasColumnName("expiresat");
            entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MagicLinkToken configuration
        modelBuilder.Entity<MagicLinkToken>(entity =>
        {
            entity.ToTable("magiclinktokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(512).IsRequired();
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ExpiresAt).HasColumnName("expiresat");
            entity.Property(e => e.IsUsed).HasColumnName("isused").HasDefaultValue(false);
            entity.Property(e => e.UsedAt).HasColumnName("usedat");
            entity.HasIndex(e => e.Token).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChangeRequest configuration
        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.ToTable("changerequests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RequestedByUserId).HasColumnName("requestedbyuserid");
            entity.Property(e => e.RequestedForDate).HasColumnName("requestedfordate").HasColumnType("date");
            entity.Property(e => e.CurrentParent).HasColumnName("currentparent").HasMaxLength(10).IsRequired();
            entity.Property(e => e.RequestedParent).HasColumnName("requestedparent").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewedat");
            entity.Property(e => e.ReviewedByUserId).HasColumnName("reviewedbyuserid");
            entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(1000);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedForDate);

            entity.HasOne(e => e.RequestedByUser)
                .WithMany()
                .HasForeignKey(e => e.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
