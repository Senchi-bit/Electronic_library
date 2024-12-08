using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Entities
{
    /// <inheritdoc />
    public partial class Delete_ExhibitionsBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExhibitionsBooks");

            migrationBuilder.AddColumn<int>(
                name: "ExhibitionId",
                table: "Books",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_ExhibitionId",
                table: "Books",
                column: "ExhibitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Exhibitions_ExhibitionId",
                table: "Books",
                column: "ExhibitionId",
                principalTable: "Exhibitions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Exhibitions_ExhibitionId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_ExhibitionId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ExhibitionId",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "ExhibitionsBooks",
                columns: table => new
                {
                    exhibitionId = table.Column<int>(type: "integer", nullable: false),
                    bookId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ExhibitionsBooks_pkey", x => new { x.exhibitionId, x.bookId });
                });
        }
    }
}
