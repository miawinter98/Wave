using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class UserProfiles : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.AddColumn<string>(
			name: "AboutTheAuthor",
			table: "AspNetUsers",
			type: "character varying(512)",
			maxLength: 512,
			nullable: false,
			defaultValue: "");

		migrationBuilder.AddColumn<string>(
			name: "Biography",
			table: "AspNetUsers",
			type: "character varying(4096)",
			maxLength: 4096,
			nullable: false,
			defaultValue: "");

		migrationBuilder.AddColumn<string>(
			name: "BiographyHtml",
			table: "AspNetUsers",
			type: "text",
			nullable: false,
			defaultValue: "");

		migrationBuilder.AddColumn<string>(
			name: "FullName",
			table: "AspNetUsers",
			type: "character varying(64)",
			maxLength: 64,
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropColumn(
			name: "AboutTheAuthor",
			table: "AspNetUsers");

		migrationBuilder.DropColumn(
			name: "Biography",
			table: "AspNetUsers");

		migrationBuilder.DropColumn(
			name: "BiographyHtml",
			table: "AspNetUsers");

		migrationBuilder.DropColumn(
			name: "FullName",
			table: "AspNetUsers");
	}
}