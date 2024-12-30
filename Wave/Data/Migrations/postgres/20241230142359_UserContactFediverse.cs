using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class UserContactFediverse : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.AddColumn<string>(
			name: "ContactFediverse",
			table: "AspNetUsers",
			type: "character varying(4096)",
			maxLength: 4096,
			nullable: false,
			defaultValue: "");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropColumn(
			name: "ContactFediverse",
			table: "AspNetUsers");
	}
}