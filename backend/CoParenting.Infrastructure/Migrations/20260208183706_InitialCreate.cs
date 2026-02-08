using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoParenting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Parent = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    IsVAB = table.Column<bool>(type: "boolean", nullable: false),
                    SpecialStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    displayname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    lastloginat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayAssignmentId = table.Column<int>(type: "integer", nullable: false),
                    Parent = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    CommentText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_DayAssignments_DayAssignmentId",
                        column: x => x.DayAssignmentId,
                        principalTable: "DayAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "changerequests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    requestedbyuserid = table.Column<int>(type: "integer", nullable: false),
                    requestedfordate = table.Column<DateOnly>(type: "date", nullable: false),
                    currentparent = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    requestedparent = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    reviewedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewedbyuserid = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_changerequests", x => x.id);
                    table.ForeignKey(
                        name: "FK_changerequests_users_requestedbyuserid",
                        column: x => x.requestedbyuserid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_changerequests_users_reviewedbyuserid",
                        column: x => x.reviewedbyuserid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "magiclinktokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isused = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
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
                name: "IX_Comments_DayAssignmentId",
                table: "Comments",
                column: "DayAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Parent",
                table: "Comments",
                column: "Parent");

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
                name: "IX_magiclinktokens_token",
                table: "magiclinktokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_magiclinktokens_userid",
                table: "magiclinktokens",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_token",
                table: "sessions",
                column: "token",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "changerequests");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "magiclinktokens");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "DayAssignments");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
