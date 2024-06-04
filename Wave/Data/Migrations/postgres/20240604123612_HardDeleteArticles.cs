using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class HardDeleteArticles : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropForeignKey(
			name: "FK_ArticleCategories_Articles_ArticleId",
			table: "ArticleCategories");

		migrationBuilder.DropForeignKey(
			name: "FK_ArticleCategories_Categories_CategoryId",
			table: "ArticleCategories");

		migrationBuilder.DropPrimaryKey(
			name: "PK_ArticleHeading",
			table: "ArticleHeading");

		migrationBuilder.AlterColumn<Guid>(
			name: "ArticleId",
			table: "ArticleHeading",
			type: "uuid",
			nullable: true,
			oldClrType: typeof(Guid),
			oldType: "uuid");

		migrationBuilder.AddPrimaryKey(
			name: "PK_ArticleHeading",
			table: "ArticleHeading",
			column: "Id");

		migrationBuilder.CreateIndex(
			name: "IX_ArticleHeading_ArticleId",
			table: "ArticleHeading",
			column: "ArticleId");

		migrationBuilder.AddForeignKey(
			name: "FK_ArticleCategories_Articles_ArticleId",
			table: "ArticleCategories",
			column: "ArticleId",
			principalTable: "Articles",
			principalColumn: "Id",
			onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
			name: "FK_ArticleCategories_Categories_CategoryId",
			table: "ArticleCategories",
			column: "CategoryId",
			principalTable: "Categories",
			principalColumn: "Id",
			onDelete: ReferentialAction.Restrict);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropForeignKey(
			name: "FK_ArticleCategories_Articles_ArticleId",
			table: "ArticleCategories");

		migrationBuilder.DropForeignKey(
			name: "FK_ArticleCategories_Categories_CategoryId",
			table: "ArticleCategories");

		migrationBuilder.DropPrimaryKey(
			name: "PK_ArticleHeading",
			table: "ArticleHeading");

		migrationBuilder.DropIndex(
			name: "IX_ArticleHeading_ArticleId",
			table: "ArticleHeading");

		migrationBuilder.AlterColumn<Guid>(
			name: "ArticleId",
			table: "ArticleHeading",
			type: "uuid",
			nullable: false,
			defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
			oldClrType: typeof(Guid),
			oldType: "uuid",
			oldNullable: true);

		migrationBuilder.AddPrimaryKey(
			name: "PK_ArticleHeading",
			table: "ArticleHeading",
			columns: new[] {"ArticleId", "Id"});

		migrationBuilder.AddForeignKey(
			name: "FK_ArticleCategories_Articles_ArticleId",
			table: "ArticleCategories",
			column: "ArticleId",
			principalTable: "Articles",
			principalColumn: "Id");

		migrationBuilder.AddForeignKey(
			name: "FK_ArticleCategories_Categories_CategoryId",
			table: "ArticleCategories",
			column: "CategoryId",
			principalTable: "Categories",
			principalColumn: "Id");
	}
}