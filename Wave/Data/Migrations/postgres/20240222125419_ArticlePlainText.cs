using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class ArticlePlainText : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.AddColumn<string>(
			name: "BodyPlain",
			table: "Articles",
			type: "text",
			nullable: false,
			defaultValue: "");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropColumn(
			name: "BodyPlain",
			table: "Articles");
	}
}