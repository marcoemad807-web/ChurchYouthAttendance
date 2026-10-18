using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchYouthAttendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeAllUserFieldsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "College",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Servants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // ملء أسماء المستخدمين للخادم والشباب المسجلين مسبقاً بقيم فريدة لمنع خطأ الـ Unique Index
            migrationBuilder.Sql(@"
                UPDATE Servants SET Username = 'abanoub.nagi' WHERE PhoneNumber LIKE '%01000000001%';
                UPDATE Servants SET Username = 'mina.samir' WHERE PhoneNumber LIKE '%01100000001%';
                UPDATE Servants SET Username = 'kyrollos.adel' WHERE PhoneNumber LIKE '%01100000002%';
                UPDATE Servants SET Username = 'george.maher' WHERE PhoneNumber LIKE '%01100000003%';
                UPDATE Servants SET Username = 'mary.nabil' WHERE PhoneNumber LIKE '%01100000004%';
                UPDATE Servants SET Username = 'servant_' + CAST(Id as varchar(20)) WHERE Username = '' OR Username IS NULL;

                UPDATE Students SET Username = 'andrew.sami' WHERE PhoneNumber LIKE '%01200000001%';
                UPDATE Students SET Username = 'boulos.maged' WHERE PhoneNumber LIKE '%01200000002%';
                UPDATE Students SET Username = 'marina.emad' WHERE PhoneNumber LIKE '%01200000003%';
                UPDATE Students SET Username = 'youssef.atef' WHERE PhoneNumber LIKE '%01200000004%';
                UPDATE Students SET Username = 'veronica.tharwat' WHERE PhoneNumber LIKE '%01200000005%';
                UPDATE Students SET Username = 'student_' + CAST(Id as varchar(20)) WHERE Username = '' OR Username IS NULL;

                UPDATE Students SET College = N'غير محدد' WHERE College IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Username",
                table: "Students",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servants_Username",
                table: "Servants",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Username",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Servants_Username",
                table: "Servants");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Servants");

            migrationBuilder.AlterColumn<string>(
                name: "College",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
