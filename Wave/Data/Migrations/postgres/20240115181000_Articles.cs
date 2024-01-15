using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

// Add-Migration Articles -OutputDir Data/Migrations/postgres -Project Wave -StartupProject Wave -Context ApplicationDbContext -Args "ConnectionStrings:DefaultConnection=Host=localhost;Port=5432;AllowAnonymousConnections=true"
/// <inheritdoc />
public partial class Articles : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.CreateTable(
            name: "Article",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Body = table.Column<string>(type: "text", nullable: false),
                BodyHtml = table.Column<string>(type: "text", nullable: false),
                AuthorId = table.Column<string>(type: "text", nullable: false),
                ReviewerId = table.Column<string>(type: "text", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "now()"),
                PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "now()")
            },
            constraints: table => {
                table.PrimaryKey("PK_Article", x => x.Id);
                table.ForeignKey(
                    name: "FK_Article_AspNetUsers_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Article_AspNetUsers_ReviewerId",
                    column: x => x.ReviewerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Article_AuthorId",
            table: "Article",
            column: "AuthorId");

        migrationBuilder.CreateIndex(
            name: "IX_Article_ReviewerId",
            table: "Article",
            column: "ReviewerId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropTable(
            name: "Article");
    }
}