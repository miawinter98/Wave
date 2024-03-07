using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class ApiKeys : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.CreateTable(
			name: "ApiKey",
			columns: table => new {
				Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
				OwnerName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
			},
			constraints: table => { table.PrimaryKey("PK_ApiKey", x => x.Key); });

		migrationBuilder.CreateTable(
			name: "ApiClaim",
			columns: table => new {
				Id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy",
						NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
				Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
				ApiKeyKey = table.Column<string>(type: "character varying(128)", nullable: true)
			},
			constraints: table => {
				table.PrimaryKey("PK_ApiClaim", x => x.Id);
				table.ForeignKey(
					name: "FK_ApiClaim_ApiKey_ApiKeyKey",
					column: x => x.ApiKeyKey,
					principalTable: "ApiKey",
					principalColumn: "Key",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_ApiClaim_ApiKeyKey",
			table: "ApiClaim",
			column: "ApiKeyKey");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropTable(
			name: "ApiClaim");

		migrationBuilder.DropTable(
			name: "ApiKey");
	}
}