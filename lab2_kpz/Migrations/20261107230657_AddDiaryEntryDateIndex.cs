using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lab2_kpz.Migrations
{
    /// <inheritdoc />
    public partial class AddDiaryEntryDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_EntryDate",
                table: "DiaryEntries",
                column: "EntryDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiaryEntries_EntryDate",
                table: "DiaryEntries");
        }
    }
}
