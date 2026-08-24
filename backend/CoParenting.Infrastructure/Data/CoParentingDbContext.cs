using CoParenting.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Infrastructure.Data;

public class CoParentingDbContext(DbContextOptions<CoParentingDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();
    public DbSet<AccountSession> AccountSessions => Set<AccountSession>();
    public DbSet<PlatformAdminSession> PlatformAdminSessions => Set<PlatformAdminSession>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<ResidenceCalendar> ResidenceCalendars => Set<ResidenceCalendar>();
    public DbSet<ScheduleVersion> ScheduleVersions => Set<ScheduleVersion>();
    public DbSet<DayOverride> DayOverrides => Set<DayOverride>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
    public DbSet<FamilyInvitation> FamilyInvitations => Set<FamilyInvitation>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<ExternalIdentity>(entity =>
        {
            entity.ToTable("external_identities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Issuer).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(500).IsRequired();
            entity.Property(x => x.NormalizedIssuer).HasMaxLength(500).IsRequired();
            entity.Property(x => x.NormalizedSubject).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => new { x.NormalizedIssuer, x.NormalizedSubject }).IsUnique();
            entity.HasOne(x => x.Account).WithMany(x => x.ExternalIdentities)
                .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccountSession>(entity =>
        {
            entity.ToTable("account_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CsrfTokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.AccountId, x.RevokedAt, x.ExpiresAt });
            entity.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlatformAdminSession>(entity =>
        {
            entity.ToTable("platform_admin_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Subject).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.AuthenticationMethod).HasMaxLength(30).IsRequired();
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CsrfTokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.AccountId, x.RevokedAt, x.ExpiresAt });
            entity.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Family>(entity =>
        {
            entity.ToTable("families");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SideALabel).HasMaxLength(80).IsRequired();
            entity.Property(x => x.SideBLabel).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ExchangeDetailLevel).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<FamilyMember>(entity =>
        {
            entity.ToTable("family_members");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Permission).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Side).HasConversion<string>().HasMaxLength(1);
            entity.Property(x => x.ConcurrencyToken).IsConcurrencyToken();
            entity.HasIndex(x => new { x.FamilyId, x.AccountId }).IsUnique();
            entity.HasIndex(x => x.FamilyId)
                .IsUnique()
                .HasFilter("\"IsActive\" = TRUE AND \"Permission\" = 'Owner'");
            entity.HasOne(x => x.Family).WithMany(x => x.Members).HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Account).WithMany(x => x.Memberships).HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResidenceCalendar>(entity =>
        {
            entity.ToTable("residence_calendars");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.FamilyId, x.Name }).IsUnique();
            entity.HasOne(x => x.Family).WithMany(x => x.Calendars).HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Child>(entity =>
        {
            entity.ToTable("children");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FamilyId, x.IsActive });
            entity.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ResidenceCalendar).WithMany(x => x.Children)
                .HasForeignKey(x => x.ResidenceCalendarId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ScheduleVersion>(entity =>
        {
            entity.ToTable("schedule_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EffectiveFrom).HasColumnType("date");
            entity.Property(x => x.AnchorDate).HasColumnType("date");
            entity.Property(x => x.AnchorSide).HasConversion<string>().HasMaxLength(1);
            entity.Property(x => x.Template).HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.ParametersJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ChangeoverTime).HasColumnType("time");
            entity.Property(x => x.ChangeoverPlace).HasMaxLength(200);
            entity.HasIndex(x => new { x.CalendarId, x.EffectiveFrom }).IsUnique();
            entity.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Calendar).WithMany(x => x.ScheduleVersions)
                .HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByMember).WithMany().HasForeignKey(x => x.CreatedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DayOverride>(entity =>
        {
            entity.ToTable("day_overrides");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Date).HasColumnType("date");
            entity.Property(x => x.Side).HasConversion<string>().HasMaxLength(1);
            entity.Property(x => x.SpecialStatus).HasMaxLength(80);
            entity.Property(x => x.ChangeoverTime).HasColumnType("time");
            entity.Property(x => x.ChangeoverPlace).HasMaxLength(200);
            entity.HasIndex(x => new { x.CalendarId, x.Date }).IsUnique();
            entity.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Calendar).WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.UpdatedByMember).WithMany().HasForeignKey(x => x.UpdatedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Date).HasColumnType("date");
            entity.Property(x => x.Text).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => new { x.CalendarId, x.Date });
            entity.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Calendar).WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.AuthorMember).WithMany().HasForeignKey(x => x.AuthorMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.ToTable("change_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FromDate).HasColumnType("date");
            entity.Property(x => x.ToDate).HasColumnType("date");
            entity.Property(x => x.RequestedSide).HasConversion<string>().HasMaxLength(1);
            entity.Property(x => x.Message).HasMaxLength(2000);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => new { x.FamilyId, x.Status });
            entity.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Calendar).WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RequestedByMember).WithMany().HasForeignKey(x => x.RequestedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReviewedByMember).WithMany().HasForeignKey(x => x.ReviewedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FamilyInvitation>(entity =>
        {
            entity.ToTable("family_invitations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LinkTokenHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CodeHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EmailHint).HasMaxLength(320);
            entity.Property(x => x.Permission).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Side).HasConversion<string>().HasMaxLength(1);
            entity.Property(x => x.ConcurrencyToken).IsConcurrencyToken();
            entity.HasIndex(x => x.LinkTokenHash).IsUnique();
            entity.HasIndex(x => x.CodeHash).IsUnique();
            entity.HasIndex(x => new { x.FamilyId, x.ExpiresAt });
            entity.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByMember).WithMany().HasForeignKey(x => x.CreatedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RedeemedByAccount).WithMany().HasForeignKey(x => x.RedeemedByAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActorType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            entity.Property(x => x.MetadataJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.FamilyId, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
            entity.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ActorAccount).WithMany().HasForeignKey(x => x.ActorAccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
