using System;
using IUT_ACM_Solution.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IUT_ACM_Solution.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261013120000_AddPendingRegistrations")]
    public partial class AddPendingRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentNumber = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    EntryYear = table.Column<int>(type: "int", nullable: false),
                    EducationLevel = table.Column<int>(type: "int", nullable: false),
                    TelegramId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRegistrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_Email",
                table: "PendingRegistrations",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_ExpiresAt",
                table: "PendingRegistrations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRegistrations_TokenHash",
                table: "PendingRegistrations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingRegistrations");
        }
    }
}
