using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutonomousMarketingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    ExceptionType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InnerException = table.Column<string>(type: "text", nullable: true),
                    RequestId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Level",
                table: "ApplicationLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_TenantId",
                table: "ApplicationLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_UserId",
                table: "ApplicationLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_CreatedAt",
                table: "ApplicationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Source",
                table: "ApplicationLogs",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_RequestId",
                table: "ApplicationLogs",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogs");
        }
    }
}

