using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wave.Data.Migrations.postgres;

/// <inheritdoc />
public partial class EmailSubscriberMetrics : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.AddColumn<string>(
			name: "Language",
			table: "NewsletterSubscribers",
			type: "character varying(8)",
			maxLength: 8,
			nullable: false,
			defaultValue: "en-US");

		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "LastMailOpened",
			table: "NewsletterSubscribers",
			type: "timestamp with time zone",
			nullable: true);

		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "LastMailReceived",
			table: "NewsletterSubscribers",
			type: "timestamp with time zone",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "UnsubscribeReason",
			table: "NewsletterSubscribers",
			type: "character varying(256)",
			maxLength: 256,
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropColumn(
			name: "Language",
			table: "NewsletterSubscribers");

		migrationBuilder.DropColumn(
			name: "LastMailOpened",
			table: "NewsletterSubscribers");

		migrationBuilder.DropColumn(
			name: "LastMailReceived",
			table: "NewsletterSubscribers");

		migrationBuilder.DropColumn(
			name: "UnsubscribeReason",
			table: "NewsletterSubscribers");
	}
}