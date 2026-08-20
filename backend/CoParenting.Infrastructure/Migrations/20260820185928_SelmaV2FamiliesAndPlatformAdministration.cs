using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoParenting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SelmaV2FamiliesAndPlatformAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Selma v2 is an intentionally clean tenant-model transition. The rollout is guarded
            // by Database:AllowDestructiveV2Reset and must only run after a verified backup.
            migrationBuilder.Sql(
                "TRUNCATE TABLE \"Comments\", \"DayAssignments\", \"Configurations\", " +
                "changerequests, externalidentities, invitations, sessions, users RESTART IDENTITY CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "FK_changerequests_users_requestedbyuserid",
                table: "changerequests");

            migrationBuilder.DropForeignKey(
                name: "FK_changerequests_users_reviewedbyuserid",
                table: "changerequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_DayAssignments_DayAssignmentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_externalidentities_users_userid",
                table: "externalidentities");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "DayAssignments");

            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comments",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_DayAssignmentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_Parent",
                table: "Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_externalidentities",
                table: "externalidentities");

            migrationBuilder.DropIndex(
                name: "IX_externalidentities_userid",
                table: "externalidentities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_changerequests",
                table: "changerequests");

            migrationBuilder.DropIndex(
                name: "IX_changerequests_requestedbyuserid",
                table: "changerequests");

            migrationBuilder.DropIndex(
                name: "IX_changerequests_requestedfordate",
                table: "changerequests");

            migrationBuilder.DropIndex(
                name: "IX_changerequests_reviewedbyuserid",
                table: "changerequests");

            migrationBuilder.DropIndex(
                name: "IX_changerequests_status",
                table: "changerequests");

            migrationBuilder.DropColumn(
                name: "CommentText",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DayAssignmentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Parent",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "userid",
                table: "externalidentities");

            migrationBuilder.DropColumn(
                name: "comment",
                table: "changerequests");

            migrationBuilder.DropColumn(
                name: "currentparent",
                table: "changerequests");

            migrationBuilder.DropColumn(
                name: "requestedbyuserid",
                table: "changerequests");

            migrationBuilder.DropColumn(
                name: "requestedparent",
                table: "changerequests");

            migrationBuilder.DropColumn(
                name: "reviewedbyuserid",
                table: "changerequests");

            migrationBuilder.RenameTable(
                name: "Comments",
                newName: "comments");

            migrationBuilder.RenameTable(
                name: "externalidentities",
                newName: "external_identities");

            migrationBuilder.RenameTable(
                name: "changerequests",
                newName: "change_requests");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "comments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "external_identities",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "normalizedsubject",
                table: "external_identities",
                newName: "NormalizedSubject");

            migrationBuilder.RenameColumn(
                name: "normalizedissuer",
                table: "external_identities",
                newName: "NormalizedIssuer");

            migrationBuilder.RenameColumn(
                name: "lastloginat",
                table: "external_identities",
                newName: "LastLoginAt");

            migrationBuilder.RenameColumn(
                name: "issuer",
                table: "external_identities",
                newName: "Issuer");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "external_identities",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "external_identities",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_externalidentities_normalizedissuer_normalizedsubject",
                table: "external_identities",
                newName: "IX_external_identities_NormalizedIssuer_NormalizedSubject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "change_requests",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reviewedat",
                table: "change_requests",
                newName: "ReviewedAt");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "change_requests",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "change_requests",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "requestedfordate",
                table: "change_requests",
                newName: "ToDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "comments",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "comments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorMemberId",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CalendarId",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "comments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "comments",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "external_identities",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "external_identities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "external_identities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "change_requests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "change_requests",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "change_requests",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "CalendarId",
                table: "change_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "change_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateOnly>(
                name: "FromDate",
                table: "change_requests",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "change_requests",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedByMemberId",
                table: "change_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RequestedSide",
                table: "change_requests",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByMemberId",
                table: "change_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_comments",
                table: "comments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_external_identities",
                table: "external_identities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_change_requests",
                table: "change_requests",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDisabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SideALabel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SideBLabel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ExchangeDetailLevel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CsrfTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_sessions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_admin_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthenticationMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CsrfTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_admin_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_admin_sessions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_events_accounts_ActorAccountId",
                        column: x => x.ActorAccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_audit_events_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "family_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_family_members_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_family_members_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "residence_calendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_residence_calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_residence_calendars_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "family_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmailHint = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Permission = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    CreatedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RedeemedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_family_invitations_accounts_RedeemedByAccountId",
                        column: x => x.RedeemedByAccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_invitations_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_family_invitations_family_members_CreatedByMemberId",
                        column: x => x.CreatedByMemberId,
                        principalTable: "family_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "children",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResidenceCalendarId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_children", x => x.Id);
                    table.ForeignKey(
                        name: "FK_children_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_children_residence_calendars_ResidenceCalendarId",
                        column: x => x.ResidenceCalendarId,
                        principalTable: "residence_calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "day_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Side = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    IsVab = table.Column<bool>(type: "boolean", nullable: false),
                    SpecialStatus = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ChangeoverTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ChangeoverPlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_day_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_day_overrides_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_day_overrides_family_members_UpdatedByMemberId",
                        column: x => x.UpdatedByMemberId,
                        principalTable: "family_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_day_overrides_residence_calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "residence_calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedule_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    AnchorDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AnchorSide = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Template = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: false),
                    ChangeoverTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ChangeoverPlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_schedule_versions_families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_schedule_versions_family_members_CreatedByMemberId",
                        column: x => x.CreatedByMemberId,
                        principalTable: "family_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_schedule_versions_residence_calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "residence_calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comments_AuthorMemberId",
                table: "comments",
                column: "AuthorMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_CalendarId_Date",
                table: "comments",
                columns: new[] { "CalendarId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_comments_FamilyId",
                table: "comments",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_external_identities_AccountId",
                table: "external_identities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_change_requests_CalendarId",
                table: "change_requests",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_change_requests_FamilyId_Status",
                table: "change_requests",
                columns: new[] { "FamilyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_change_requests_RequestedByMemberId",
                table: "change_requests",
                column: "RequestedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_change_requests_ReviewedByMemberId",
                table: "change_requests",
                column: "ReviewedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_account_sessions_AccountId_RevokedAt_ExpiresAt",
                table: "account_sessions",
                columns: new[] { "AccountId", "RevokedAt", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_sessions_TokenHash",
                table: "account_sessions",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_NormalizedEmail",
                table: "accounts",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_ActorAccountId",
                table: "audit_events",
                column: "ActorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_CreatedAt",
                table: "audit_events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_FamilyId_CreatedAt",
                table: "audit_events",
                columns: new[] { "FamilyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_children_FamilyId_IsActive",
                table: "children",
                columns: new[] { "FamilyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_children_ResidenceCalendarId",
                table: "children",
                column: "ResidenceCalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_day_overrides_CalendarId_Date",
                table: "day_overrides",
                columns: new[] { "CalendarId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_day_overrides_FamilyId",
                table: "day_overrides",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_day_overrides_UpdatedByMemberId",
                table: "day_overrides",
                column: "UpdatedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_families_Status",
                table: "families",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_CodeHash",
                table: "family_invitations",
                column: "CodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_CreatedByMemberId",
                table: "family_invitations",
                column: "CreatedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_FamilyId_ExpiresAt",
                table: "family_invitations",
                columns: new[] { "FamilyId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_LinkTokenHash",
                table: "family_invitations",
                column: "LinkTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_family_invitations_RedeemedByAccountId",
                table: "family_invitations",
                column: "RedeemedByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_family_members_AccountId",
                table: "family_members",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_family_members_FamilyId",
                table: "family_members",
                column: "FamilyId",
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"Permission\" = 'Owner'");

            migrationBuilder.CreateIndex(
                name: "IX_family_members_FamilyId_AccountId",
                table: "family_members",
                columns: new[] { "FamilyId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_admin_sessions_AccountId_RevokedAt_ExpiresAt",
                table: "platform_admin_sessions",
                columns: new[] { "AccountId", "RevokedAt", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_admin_sessions_TokenHash",
                table: "platform_admin_sessions",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_residence_calendars_FamilyId_Name",
                table: "residence_calendars",
                columns: new[] { "FamilyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_versions_CalendarId_EffectiveFrom",
                table: "schedule_versions",
                columns: new[] { "CalendarId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_versions_CreatedByMemberId",
                table: "schedule_versions",
                column: "CreatedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_versions_FamilyId",
                table: "schedule_versions",
                column: "FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_change_requests_families_FamilyId",
                table: "change_requests",
                column: "FamilyId",
                principalTable: "families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_change_requests_family_members_RequestedByMemberId",
                table: "change_requests",
                column: "RequestedByMemberId",
                principalTable: "family_members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_change_requests_family_members_ReviewedByMemberId",
                table: "change_requests",
                column: "ReviewedByMemberId",
                principalTable: "family_members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_change_requests_residence_calendars_CalendarId",
                table: "change_requests",
                column: "CalendarId",
                principalTable: "residence_calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_families_FamilyId",
                table: "comments",
                column: "FamilyId",
                principalTable: "families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_family_members_AuthorMemberId",
                table: "comments",
                column: "AuthorMemberId",
                principalTable: "family_members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_comments_residence_calendars_CalendarId",
                table: "comments",
                column: "CalendarId",
                principalTable: "residence_calendars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_external_identities_accounts_AccountId",
                table: "external_identities",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_change_requests_families_FamilyId",
                table: "change_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_change_requests_family_members_RequestedByMemberId",
                table: "change_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_change_requests_family_members_ReviewedByMemberId",
                table: "change_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_change_requests_residence_calendars_CalendarId",
                table: "change_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_families_FamilyId",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_family_members_AuthorMemberId",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_comments_residence_calendars_CalendarId",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "FK_external_identities_accounts_AccountId",
                table: "external_identities");

            migrationBuilder.DropTable(
                name: "account_sessions");

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "children");

            migrationBuilder.DropTable(
                name: "day_overrides");

            migrationBuilder.DropTable(
                name: "family_invitations");

            migrationBuilder.DropTable(
                name: "platform_admin_sessions");

            migrationBuilder.DropTable(
                name: "schedule_versions");

            migrationBuilder.DropTable(
                name: "family_members");

            migrationBuilder.DropTable(
                name: "residence_calendars");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "families");

            migrationBuilder.DropPrimaryKey(
                name: "PK_comments",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_AuthorMemberId",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_CalendarId_Date",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_FamilyId",
                table: "comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_external_identities",
                table: "external_identities");

            migrationBuilder.DropIndex(
                name: "IX_external_identities_AccountId",
                table: "external_identities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_change_requests",
                table: "change_requests");

            migrationBuilder.DropIndex(
                name: "IX_change_requests_CalendarId",
                table: "change_requests");

            migrationBuilder.DropIndex(
                name: "IX_change_requests_FamilyId_Status",
                table: "change_requests");

            migrationBuilder.DropIndex(
                name: "IX_change_requests_RequestedByMemberId",
                table: "change_requests");

            migrationBuilder.DropIndex(
                name: "IX_change_requests_ReviewedByMemberId",
                table: "change_requests");

            migrationBuilder.DropColumn(
                name: "AuthorMemberId",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "external_identities");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "change_requests");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "change_requests");

            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "change_requests");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "change_requests");

            migrationBuilder.DropColumn(
                name: "RequestedByMemberId",
                table: "change_requests");

            migrationBuilder.DropColumn(
                name: "RequestedSide",
                table: "change_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedByMemberId",
                table: "change_requests");

            migrationBuilder.RenameTable(
                name: "comments",
                newName: "Comments");

            migrationBuilder.RenameTable(
                name: "external_identities",
                newName: "externalidentities");

            migrationBuilder.RenameTable(
                name: "change_requests",
                newName: "changerequests");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Comments",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "externalidentities",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "NormalizedSubject",
                table: "externalidentities",
                newName: "normalizedsubject");

            migrationBuilder.RenameColumn(
                name: "NormalizedIssuer",
                table: "externalidentities",
                newName: "normalizedissuer");

            migrationBuilder.RenameColumn(
                name: "LastLoginAt",
                table: "externalidentities",
                newName: "lastloginat");

            migrationBuilder.RenameColumn(
                name: "Issuer",
                table: "externalidentities",
                newName: "issuer");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "externalidentities",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "externalidentities",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_external_identities_NormalizedIssuer_NormalizedSubject",
                table: "externalidentities",
                newName: "IX_externalidentities_normalizedissuer_normalizedsubject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "changerequests",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "changerequests",
                newName: "reviewedat");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "changerequests",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "changerequests",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "changerequests",
                newName: "requestedfordate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Comments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "CommentText",
                table: "Comments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DayAssignmentId",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Parent",
                table: "Comments",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdat",
                table: "externalidentities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "externalidentities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "userid",
                table: "externalidentities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "changerequests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdat",
                table: "changerequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "changerequests",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "changerequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currentparent",
                table: "changerequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "requestedbyuserid",
                table: "changerequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "requestedparent",
                table: "changerequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "reviewedbyuserid",
                table: "changerequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comments",
                table: "Comments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_externalidentities",
                table: "externalidentities",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_changerequests",
                table: "changerequests",
                column: "id");

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DayAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsVAB = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Parent = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    SpecialStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    displayname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    islocaladmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    lastloginat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    createdbyuserid = table.Column<int>(type: "integer", nullable: false),
                    redeemedbyuserid = table.Column<int>(type: "integer", nullable: true),
                    concurrencytoken = table.Column<Guid>(type: "uuid", nullable: false),
                    consumedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    emailhint = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tokenhash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_invitations_users_createdbyuserid",
                        column: x => x.createdbyuserid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_users_redeemedbyuserid",
                        column: x => x.redeemedbyuserid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tokenhash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_sessions_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Id", "CreatedAt", "Key", "ModifiedAt", "Value" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 8, 18, 37, 6, 87, DateTimeKind.Utc).AddTicks(8927), "ParentAName", null, "Tomas" },
                    { 2, new DateTime(2026, 2, 8, 18, 37, 6, 87, DateTimeKind.Utc).AddTicks(8929), "ParentBName", null, "Carro" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DayAssignmentId",
                table: "Comments",
                column: "DayAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Parent",
                table: "Comments",
                column: "Parent");

            migrationBuilder.CreateIndex(
                name: "IX_externalidentities_userid",
                table: "externalidentities",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_changerequests_requestedbyuserid",
                table: "changerequests",
                column: "requestedbyuserid");

            migrationBuilder.CreateIndex(
                name: "IX_changerequests_requestedfordate",
                table: "changerequests",
                column: "requestedfordate");

            migrationBuilder.CreateIndex(
                name: "IX_changerequests_reviewedbyuserid",
                table: "changerequests",
                column: "reviewedbyuserid");

            migrationBuilder.CreateIndex(
                name: "IX_changerequests_status",
                table: "changerequests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_Key",
                table: "Configurations",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DayAssignments_Date",
                table: "DayAssignments",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invitations_createdbyuserid",
                table: "invitations",
                column: "createdbyuserid");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_redeemedbyuserid",
                table: "invitations",
                column: "redeemedbyuserid");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_tokenhash",
                table: "invitations",
                column: "tokenhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_tokenhash",
                table: "sessions",
                column: "tokenhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_userid",
                table: "sessions",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_changerequests_users_requestedbyuserid",
                table: "changerequests",
                column: "requestedbyuserid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_changerequests_users_reviewedbyuserid",
                table: "changerequests",
                column: "reviewedbyuserid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_DayAssignments_DayAssignmentId",
                table: "Comments",
                column: "DayAssignmentId",
                principalTable: "DayAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_externalidentities_users_userid",
                table: "externalidentities",
                column: "userid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
