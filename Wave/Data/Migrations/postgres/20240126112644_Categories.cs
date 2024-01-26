using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class Categories : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropForeignKey(
            name: "FK_Article_AspNetUsers_AuthorId",
            table: "Article");

        migrationBuilder.DropForeignKey(
            name: "FK_Article_AspNetUsers_ReviewerId",
            table: "Article");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Article",
            table: "Article");

        migrationBuilder.RenameTable(
            name: "Article",
            newName: "Articles");

        migrationBuilder.RenameIndex(
            name: "IX_Article_ReviewerId",
            table: "Articles",
            newName: "IX_Articles_ReviewerId");

        migrationBuilder.RenameIndex(
            name: "IX_Article_AuthorId",
            table: "Articles",
            newName: "IX_Articles_AuthorId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Articles",
            table: "Articles",
            column: "Id");

        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Color = table.Column<int>(type: "integer", nullable: false, defaultValue: 25)
            },
            constraints: table => { table.PrimaryKey("PK_Categories", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "ArticleCategories",
            columns: table => new {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table => {
                table.PrimaryKey("PK_ArticleCategories", x => x.Id);
                table.ForeignKey(
                    name: "FK_ArticleCategories_Articles_ArticleId",
                    column: x => x.ArticleId,
                    principalTable: "Articles",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ArticleCategories_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_ArticleCategories_ArticleId",
            table: "ArticleCategories",
            column: "ArticleId");

        migrationBuilder.CreateIndex(
            name: "IX_ArticleCategories_CategoryId",
            table: "ArticleCategories",
            column: "CategoryId");

        migrationBuilder.AddForeignKey(
            name: "FK_Articles_AspNetUsers_AuthorId",
            table: "Articles",
            column: "AuthorId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Articles_AspNetUsers_ReviewerId",
            table: "Articles",
            column: "ReviewerId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropForeignKey(
            name: "FK_Articles_AspNetUsers_AuthorId",
            table: "Articles");

        migrationBuilder.DropForeignKey(
            name: "FK_Articles_AspNetUsers_ReviewerId",
            table: "Articles");

        migrationBuilder.DropTable(
            name: "ArticleCategories");

        migrationBuilder.DropTable(
            name: "Categories");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Articles",
            table: "Articles");

        migrationBuilder.RenameTable(
            name: "Articles",
            newName: "Article");

        migrationBuilder.RenameIndex(
            name: "IX_Articles_ReviewerId",
            table: "Article",
            newName: "IX_Article_ReviewerId");

        migrationBuilder.RenameIndex(
            name: "IX_Articles_AuthorId",
            table: "Article",
            newName: "IX_Article_AuthorId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Article",
            table: "Article",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Article_AspNetUsers_AuthorId",
            table: "Article",
            column: "AuthorId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Article_AspNetUsers_ReviewerId",
            table: "Article",
            column: "ReviewerId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }
}