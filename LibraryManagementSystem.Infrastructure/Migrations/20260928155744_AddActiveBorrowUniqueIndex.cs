using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveBorrowUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Borrows_BookId",
                table: "Borrows");

            migrationBuilder.CreateIndex(
                name: "UX_Borrows_ActiveBook",
                table: "Borrows",
                column: "BookId",
                unique: true,
                filter: "[Status] IN (0, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Borrows_ActiveBook",
                table: "Borrows");

            migrationBuilder.CreateIndex(
                name: "IX_Borrows_BookId",
                table: "Borrows",
                column: "BookId");
        }
    }
}
