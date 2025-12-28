using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutonomousMarketingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId1",
                table: "MarketingPacks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Campaigns",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Budget",
                table: "Campaigns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Campaigns",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Objectives",
                table: "Campaigns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetAudience",
                table: "Campaigns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetChannels",
                table: "Campaigns",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Impressions = table.Column<long>(type: "bigint", nullable: false),
                    Clicks = table.Column<long>(type: "bigint", nullable: false),
                    Engagement = table.Column<long>(type: "bigint", nullable: false),
                    Likes = table.Column<long>(type: "bigint", nullable: false),
                    Comments = table.Column<long>(type: "bigint", nullable: false),
                    Shares = table.Column<long>(type: "bigint", nullable: false),
                    ActivePosts = table.Column<int>(type: "integer", nullable: false),
                    IsManualEntry = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignMetrics_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PublishingJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketingPackId = table.Column<Guid>(type: "uuid", nullable: true),
                    GeneratedCopyId = table.Column<Guid>(type: "uuid", nullable: true),
                    MarketingAssetPromptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    DownloadUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExternalPostId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Hashtags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MediaUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishingJobs_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PublishingJobs_GeneratedCopies_GeneratedCopyId",
                        column: x => x.GeneratedCopyId,
                        principalTable: "GeneratedCopies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PublishingJobs_MarketingAssetPrompts_MarketingAssetPromptId",
                        column: x => x.MarketingAssetPromptId,
                        principalTable: "MarketingAssetPrompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PublishingJobs_MarketingPacks_MarketingPackId",
                        column: x => x.MarketingPackId,
                        principalTable: "MarketingPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PublishingJobMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishingJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Impressions = table.Column<long>(type: "bigint", nullable: false),
                    Clicks = table.Column<long>(type: "bigint", nullable: false),
                    Engagement = table.Column<long>(type: "bigint", nullable: false),
                    Likes = table.Column<long>(type: "bigint", nullable: false),
                    Comments = table.Column<long>(type: "bigint", nullable: false),
                    Shares = table.Column<long>(type: "bigint", nullable: false),
                    ClickThroughRate = table.Column<decimal>(type: "numeric", nullable: true),
                    EngagementRate = table.Column<decimal>(type: "numeric", nullable: true),
                    IsManualEntry = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishingJobMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishingJobMetrics_PublishingJobs_PublishingJobId",
                        column: x => x.PublishingJobId,
                        principalTable: "PublishingJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingPacks_CampaignId1",
                table: "MarketingPacks",
                column: "CampaignId1");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_TenantId_Status",
                table: "Campaigns",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMetrics_CampaignId",
                table: "CampaignMetrics",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMetrics_TenantId",
                table: "CampaignMetrics",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMetrics_TenantId_CampaignId_MetricDate",
                table: "CampaignMetrics",
                columns: new[] { "TenantId", "CampaignId", "MetricDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMetrics_TenantId_MetricDate",
                table: "CampaignMetrics",
                columns: new[] { "TenantId", "MetricDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobMetrics_PublishingJobId",
                table: "PublishingJobMetrics",
                column: "PublishingJobId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobMetrics_TenantId",
                table: "PublishingJobMetrics",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobMetrics_TenantId_MetricDate",
                table: "PublishingJobMetrics",
                columns: new[] { "TenantId", "MetricDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobMetrics_TenantId_PublishingJobId_MetricDate",
                table: "PublishingJobMetrics",
                columns: new[] { "TenantId", "PublishingJobId", "MetricDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobs_CampaignId",
                table: "PublishingJobs",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobs_GeneratedCopyId",
                table: "PublishingJobs",
                column: "GeneratedCopyId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobs_MarketingAssetPromptId",
                table: "PublishingJobs",
                column: "MarketingAssetPromptId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobs_MarketingPackId",
                table: "PublishingJobs",
                column: "MarketingPackId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobs_TenantId",
                table: "PublishingJobs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobs_TenantId_CampaignId_Status",
                table: "PublishingJobs",
                columns: new[] { "TenantId", "CampaignId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PublishingJobs_TenantId_ScheduledDate",
                table: "PublishingJobs",
                columns: new[] { "TenantId", "ScheduledDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_MarketingPacks_Campaigns_CampaignId1",
                table: "MarketingPacks",
                column: "CampaignId1",
                principalTable: "Campaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarketingPacks_Campaigns_CampaignId1",
                table: "MarketingPacks");

            migrationBuilder.DropTable(
                name: "CampaignMetrics");

            migrationBuilder.DropTable(
                name: "PublishingJobMetrics");

            migrationBuilder.DropTable(
                name: "PublishingJobs");

            migrationBuilder.DropIndex(
                name: "IX_MarketingPacks_CampaignId1",
                table: "MarketingPacks");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_TenantId_Status",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "CampaignId1",
                table: "MarketingPacks");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Objectives",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TargetAudience",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TargetChannels",
                table: "Campaigns");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Campaigns",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Budget",
                table: "Campaigns",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
