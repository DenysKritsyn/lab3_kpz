using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kpz.Lab2.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPhoneNumberAndDiaryEntriesIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserPhoneNumber",
                table: "Users",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_UserId_EntryUpdatedAt",
                table: "DiaryEntries",
                columns: new[] { "UserId", "EntryUpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiaryEntries_UserId_EntryUpdatedAt",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "UserPhoneNumber",
                table: "Users");
        }
    }
}
