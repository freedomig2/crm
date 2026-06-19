using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SalesPipelineManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpportunityStageHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreviousStageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NewStageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityStageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityStageHistory_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OpportunityStageHistory_LookupValues_NewStageId",
                        column: x => x.NewStageId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityStageHistory_LookupValues_PreviousStageId",
                        column: x => x.PreviousStageId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityStageHistory_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevenueForecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ForecastDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ForecastPeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ForecastPeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ForecastTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalPipelineRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    WeightedPipelineRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    ForecastRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClosedRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    OpenRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenueForecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevenueForecasts_LookupValues_ForecastTypeId",
                        column: x => x.ForecastTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesPerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OpenOpportunities = table.Column<int>(type: "INTEGER", nullable: false),
                    WonOpportunities = table.Column<int>(type: "INTEGER", nullable: false),
                    LostOpportunities = table.Column<int>(type: "INTEGER", nullable: false),
                    PipelineRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    WeightedRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClosedRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    AverageDealSize = table.Column<decimal>(type: "TEXT", nullable: false),
                    AverageSalesCycleDays = table.Column<decimal>(type: "TEXT", nullable: false),
                    WinRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    ForecastAccuracy = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPerformanceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesPerformanceSnapshots_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesPerformanceSnapshots_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SalesTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TargetTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetPeriodId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    AchievementPercentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AssignedTeamId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OwnerTeamId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTargets_AspNetUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesTargets_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesTargets_LookupValues_TargetPeriodId",
                        column: x => x.TargetPeriodId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesTargets_LookupValues_TargetTypeId",
                        column: x => x.TargetTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesTargets_Teams_AssignedTeamId",
                        column: x => x.AssignedTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesTargets_Teams_OwnerTeamId",
                        column: x => x.OwnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStageHistory_ChangedByUserId",
                table: "OpportunityStageHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStageHistory_NewStageId",
                table: "OpportunityStageHistory",
                column: "NewStageId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStageHistory_OpportunityId_ChangedAt",
                table: "OpportunityStageHistory",
                columns: new[] { "OpportunityId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStageHistory_PreviousStageId",
                table: "OpportunityStageHistory",
                column: "PreviousStageId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueForecasts_ForecastDate",
                table: "RevenueForecasts",
                column: "ForecastDate");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueForecasts_ForecastPeriodStart_ForecastPeriodEnd",
                table: "RevenueForecasts",
                columns: new[] { "ForecastPeriodStart", "ForecastPeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_RevenueForecasts_ForecastTypeId",
                table: "RevenueForecasts",
                column: "ForecastTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPerformanceSnapshots_SnapshotDate",
                table: "SalesPerformanceSnapshots",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPerformanceSnapshots_TeamId",
                table: "SalesPerformanceSnapshots",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPerformanceSnapshots_UserId",
                table: "SalesPerformanceSnapshots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_AssignedTeamId",
                table: "SalesTargets",
                column: "AssignedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_AssignedUserId",
                table: "SalesTargets",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_OwnerTeamId",
                table: "SalesTargets",
                column: "OwnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_OwnerUserId",
                table: "SalesTargets",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_TargetPeriodId",
                table: "SalesTargets",
                column: "TargetPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_TargetTypeId",
                table: "SalesTargets",
                column: "TargetTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityStageHistory");

            migrationBuilder.DropTable(
                name: "RevenueForecasts");

            migrationBuilder.DropTable(
                name: "SalesPerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "SalesTargets");
        }
    }
}
