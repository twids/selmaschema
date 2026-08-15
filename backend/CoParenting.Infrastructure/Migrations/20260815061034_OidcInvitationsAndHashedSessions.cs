using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoParenting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OidcInvitationsAndHashedSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Session tokens were previously stored in plaintext. A clean cut-over avoids
            // both reusing them and creating duplicate empty hashes during the column change.
            migrationBuilder.Sql("DELETE FROM sessions;");

            migrationBuilder.DropTable(
                name: "magiclinktokens");

            migrationBuilder.DropIndex(
                name: "IX_sessions_token",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "token",
                table: "sessions");

            migrationBuilder.AddColumn<bool>(
                name: "islocaladmin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "tokenhash",
                table: "sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "externalidentities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    issuer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    normalizedissuer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    normalizedsubject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    lastloginat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_externalidentities", x => x.id);
                    table.ForeignKey(
                        name: "FK_externalidentities_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tokenhash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    emailhint = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    createdbyuserid = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    redeemedbyuserid = table.Column<int>(type: "integer", nullable: true),
                    consumedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    concurrencytoken = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_sessions_tokenhash",
                table: "sessions",
                column: "tokenhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_externalidentities_normalizedissuer_normalizedsubject",
                table: "externalidentities",
                columns: new[] { "normalizedissuer", "normalizedsubject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_externalidentities_userid",
                table: "externalidentities",
                column: "userid");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "externalidentities");

            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropIndex(
                name: "IX_sessions_tokenhash",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "islocaladmin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "tokenhash",
                table: "sessions");

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "sessions",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "magiclinktokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isused = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    usedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_magiclinktokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_magiclinktokens_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_token",
                table: "sessions",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_magiclinktokens_token",
                table: "magiclinktokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_magiclinktokens_userid",
                table: "magiclinktokens",
                column: "userid");
        }
    }
}
