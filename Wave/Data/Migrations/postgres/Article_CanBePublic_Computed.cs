using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class Article_CanBePublic_Computed : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.AddColumn<bool>(
			name: "CanBePublic",
			table: "Articles",
			type: "boolean",
			nullable: false,
			computedColumnSql: "\"IsDeleted\" = false AND \"Status\" = 2",
			stored: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropColumn(
			name: "CanBePublic",
			table: "Articles");
	}
}