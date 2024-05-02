using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class TableOfContents : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.CreateTable(
			name: "ArticleHeading",
			columns: table => new {
				ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
				Id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy",
						NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				Order = table.Column<int>(type: "integer", nullable: false),
				Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
				Anchor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
			},
			constraints: table => {
				table.PrimaryKey("PK_ArticleHeading", x => new {x.ArticleId, x.Id});
				table.ForeignKey(
					name: "FK_ArticleHeading_Articles_ArticleId",
					column: x => x.ArticleId,
					principalTable: "Articles",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropTable(
			name: "ArticleHeading");
	}
}