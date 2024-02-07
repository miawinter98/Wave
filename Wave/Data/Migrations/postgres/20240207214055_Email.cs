using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class Email : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.CreateTable(
			name: "Newsletter",
			columns: table => new {
				Id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy",
						NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				IsSend = table.Column<bool>(type: "boolean", nullable: false),
				DistributionDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
				ArticleId = table.Column<Guid>(type: "uuid", nullable: false)
			},
			constraints: table => {
				table.PrimaryKey("PK_Newsletter", x => x.Id);
				table.ForeignKey(
					name: "FK_Newsletter_Articles_ArticleId",
					column: x => x.ArticleId,
					principalTable: "Articles",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateTable(
			name: "NewsletterSubscribers",
			columns: table => new {
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
				Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
				Unsubscribed = table.Column<bool>(type: "boolean", nullable: false)
			},
			constraints: table => { table.PrimaryKey("PK_NewsletterSubscribers", x => x.Id); });

		migrationBuilder.CreateIndex(
			name: "IX_Newsletter_ArticleId",
			table: "Newsletter",
			column: "ArticleId",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_NewsletterSubscribers_Unsubscribed",
			table: "NewsletterSubscribers",
			column: "Unsubscribed");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropTable(
			name: "Newsletter");

		migrationBuilder.DropTable(
			name: "NewsletterSubscribers");
	}
}