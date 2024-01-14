using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wave.Data.Migrations.postgres;

// Add-Migration ProfilePictures -OutputDir Data/Migrations/postgres -Project Wave -StartupProject Wave -Context ApplicationDbContext -Args "ConnectionStrings:DefaultConnection=Host=localhost;Port=5432;AllowAnonymousConnections=true"
/// <inheritdoc />
public partial class ProfilePictures : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.CreateTable(
            name: "ProfilePictures",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationUserId = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => {
                table.PrimaryKey("PK_ProfilePictures", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProfilePictures_AspNetUsers_ApplicationUserId",
                    column: x => x.ApplicationUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProfilePictures_ApplicationUserId",
            table: "ProfilePictures",
            column: "ApplicationUserId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropTable(
            name: "ProfilePictures");
    }
}