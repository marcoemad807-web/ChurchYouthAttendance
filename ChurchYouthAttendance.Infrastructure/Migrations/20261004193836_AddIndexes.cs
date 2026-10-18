using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchYouthAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Students_FullName",
                table: "Students",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Servants_FullName",
                table: "Servants",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AttendedAt",
                table: "Attendances",
                column: "AttendedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_FullName",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Servants_FullName",
                table: "Servants");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_AttendedAt",
                table: "Attendances");
        }
    }
}
