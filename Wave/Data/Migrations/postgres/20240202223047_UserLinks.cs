using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class UserLinks : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.CreateTable(
			name: "UserLink",
			columns: table => new {
				Id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy",
						NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				UrlString = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
				ApplicationUserId = table.Column<string>(type: "text", nullable: true)
			},
			constraints: table => {
				table.PrimaryKey("PK_UserLink", x => x.Id);
				table.ForeignKey(
					name: "FK_UserLink_AspNetUsers_ApplicationUserId",
					column: x => x.ApplicationUserId,
					principalTable: "AspNetUsers",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_UserLink_ApplicationUserId",
			table: "UserLink",
			column: "ApplicationUserId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropTable(
			name: "UserLink");
	}
}