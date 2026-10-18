using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchYouthAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementsAndDeviceTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TargetAcademicYear = table.Column<int>(type: "int", nullable: true),
                    SenderServantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_Servants_SenderServantId",
                        column: x => x.SenderServantId,
                        principalTable: "Servants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_SenderServantId",
                table: "Announcements",
                column: "SenderServantId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_SentAt",
                table: "Announcements",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_TargetAcademicYear",
                table: "Announcements",
                column: "TargetAcademicYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "Students");
        }
    }
}
