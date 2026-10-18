using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchYouthAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Attendances",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Attendances");
        }
    }
}
