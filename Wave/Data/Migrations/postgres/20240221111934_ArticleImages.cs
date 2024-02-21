using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class ArticleImages : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.CreateTable(
			name: "Images",
			columns: table => new {
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				ImageDescription =
					table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
				ArticleId = table.Column<Guid>(type: "uuid", nullable: true)
			},
			constraints: table => {
				table.PrimaryKey("PK_Images", x => x.Id);
				table.ForeignKey(
					name: "FK_Images_Articles_ArticleId",
					column: x => x.ArticleId,
					principalTable: "Articles",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_Images_ArticleId",
			table: "Images",
			column: "ArticleId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropTable(
			name: "Images");
	}
}