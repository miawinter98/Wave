using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class EmailSubscriberCaseInsensitiveUnique : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.AlterColumn<string>(
			name: "Email",
			table: "NewsletterSubscribers",
			type: "character varying(256)",
			maxLength: 256,
			nullable: false,
			collation: "default-case-insensitive",
			oldClrType: typeof(string),
			oldType: "character varying(256)",
			oldMaxLength: 256);

		migrationBuilder.CreateIndex(
			name: "IX_NewsletterSubscribers_Email",
			table: "NewsletterSubscribers",
			column: "Email",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropIndex(
			name: "IX_NewsletterSubscribers_Email",
			table: "NewsletterSubscribers");

		migrationBuilder.AlterColumn<string>(
			name: "Email",
			table: "NewsletterSubscribers",
			type: "character varying(256)",
			maxLength: 256,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "character varying(256)",
			oldMaxLength: 256,
			oldCollation: "default-case-insensitive");
	}
}