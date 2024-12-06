using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Entities
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    fullName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Authors_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    releaseYear = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Books_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Exhibitions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    yearBased = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("exhibitions_pkey", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "AuthorBook",
                columns: table => new
                {
                    authorId = table.Column<int>(type: "integer", nullable: false),
                    bookId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AuthorBook_pkey", x => new { x.authorId, x.bookId });
                    table.ForeignKey(
                        name: "authorId_fk",
                        column: x => x.authorId,
                        principalTable: "Authors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "bookId_fk",
                        column: x => x.bookId,
                        principalTable: "Books",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorBook_bookId",
                table: "AuthorBook",
                column: "bookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorBook");

            migrationBuilder.DropTable(
                name: "Exhibitions");

            migrationBuilder.DropTable(
                name: "ExhibitionsBooks");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
