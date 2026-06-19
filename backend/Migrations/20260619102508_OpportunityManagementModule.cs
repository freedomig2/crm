using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LeadId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OpportunityStageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SalesProcessStageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RatingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PriorityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EstimatedRevenue = table.Column<decimal>(type: "TEXT", nullable: true),
                    EstimatedCloseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Probability = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    WeightedRevenue = table.Column<decimal>(type: "TEXT", nullable: true),
                    ActualRevenue = table.Column<decimal>(type: "TEXT", nullable: true),
                    ActualCloseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CurrencyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WinReasonId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LossReasonId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LostToCompetitorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Opportunities_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_LossReasonId",
                        column: x => x.LossReasonId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_OpportunityStageId",
                        column: x => x.OpportunityStageId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_OpportunityStatusId",
                        column: x => x.OpportunityStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_RatingId",
                        column: x => x.RatingId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_SalesProcessStageId",
                        column: x => x.SalesProcessStageId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_SourceId",
                        column: x => x.SourceId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LookupValues_WinReasonId",
                        column: x => x.WinReasonId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_Teams_OwnerTeamId",
                        column: x => x.OwnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActivityTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PriorityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityActivities_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OpportunityActivities_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OpportunityActivities_LookupValues_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityActivities_LookupValues_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityActivities_LookupValues_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityActivities_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityCompetitors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompetitorName = table.Column<string>(type: "TEXT", nullable: false),
                    Strengths = table.Column<string>(type: "TEXT", nullable: true),
                    Weaknesses = table.Column<string>(type: "TEXT", nullable: true),
                    ThreatLevelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsPrimaryCompetitor = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_OpportunityCompetitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityCompetitors_LookupValues_ThreatLevelId",
                        column: x => x.ThreatLevelId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityCompetitors_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "TEXT", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    LineTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityProducts_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AccountId",
                table: "Opportunities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ContactId",
                table: "Opportunities",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CurrencyId",
                table: "Opportunities",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_EstimatedCloseDate",
                table: "Opportunities",
                column: "EstimatedCloseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_LeadId",
                table: "Opportunities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_LossReasonId",
                table: "Opportunities",
                column: "LossReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_LostToCompetitorId",
                table: "Opportunities",
                column: "LostToCompetitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_OpportunityNumber",
                table: "Opportunities",
                column: "OpportunityNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_OpportunityStageId",
                table: "Opportunities",
                column: "OpportunityStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_OpportunityStatusId",
                table: "Opportunities",
                column: "OpportunityStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_OwnerTeamId",
                table: "Opportunities",
                column: "OwnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_OwnerUserId",
                table: "Opportunities",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_PriorityId",
                table: "Opportunities",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_RatingId",
                table: "Opportunities",
                column: "RatingId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_SalesProcessStageId",
                table: "Opportunities",
                column: "SalesProcessStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_SourceId",
                table: "Opportunities",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_WinReasonId",
                table: "Opportunities",
                column: "WinReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityActivities_ActivityTypeId",
                table: "OpportunityActivities",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityActivities_AssignedToUserId",
                table: "OpportunityActivities",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityActivities_ContactId",
                table: "OpportunityActivities",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityActivities_OpportunityId",
                table: "OpportunityActivities",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityActivities_PriorityId",
                table: "OpportunityActivities",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityActivities_StatusId",
                table: "OpportunityActivities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCompetitors_OpportunityId",
                table: "OpportunityCompetitors",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCompetitors_ThreatLevelId",
                table: "OpportunityCompetitors",
                column: "ThreatLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityProducts_OpportunityId",
                table: "OpportunityProducts",
                column: "OpportunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_OpportunityCompetitors_LostToCompetitorId",
                table: "Opportunities",
                column: "LostToCompetitorId",
                principalTable: "OpportunityCompetitors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_OpportunityCompetitors_LostToCompetitorId",
                table: "Opportunities");

            migrationBuilder.DropTable(
                name: "OpportunityActivities");

            migrationBuilder.DropTable(
                name: "OpportunityProducts");

            migrationBuilder.DropTable(
                name: "OpportunityCompetitors");

            migrationBuilder.DropTable(
                name: "Opportunities");
        }
    }
}
