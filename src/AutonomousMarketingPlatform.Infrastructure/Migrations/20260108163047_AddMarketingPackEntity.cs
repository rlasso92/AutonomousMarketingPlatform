using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutonomousMarketingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingPackEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MarketingPackId",
                table: "Content",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarketingPacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingPacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Content_MarketingPackId",
                table: "Content",
                column: "MarketingPackId");

            migrationBuilder.AddForeignKey(
                name: "FK_Content_MarketingPacks_MarketingPackId",
                table: "Content",
                column: "MarketingPackId",
                principalTable: "MarketingPacks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Content_MarketingPacks_MarketingPackId",
                table: "Content");

            migrationBuilder.DropTable(
                name: "MarketingPacks");

            migrationBuilder.DropIndex(
                name: "IX_Content_MarketingPackId",
                table: "Content");

            migrationBuilder.DropColumn(
                name: "MarketingPackId",
                table: "Content");
        }
    }
}
