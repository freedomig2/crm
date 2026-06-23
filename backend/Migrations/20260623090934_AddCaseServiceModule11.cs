using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseServiceModule11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CaseNumber = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CaseStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PriorityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SeverityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EscalatedToUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DueAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolutionSummary = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_ServiceCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceCases_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceCases_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceCases_AspNetUsers_EscalatedToUserId",
                        column: x => x.EscalatedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceCases_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceCases_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceCases_LookupValues_CaseStatusId",
                        column: x => x.CaseStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceCases_LookupValues_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceCases_LookupValues_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceCases_LookupValues_SeverityId",
                        column: x => x.SeverityId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceCases_LookupValues_SourceId",
                        column: x => x.SourceId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceCases_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceCases_Teams_OwnerTeamId",
                        column: x => x.OwnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActivityTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_CaseActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseActivities_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CaseActivities_LookupValues_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CaseActivities_LookupValues_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CaseActivities_LookupValues_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CaseActivities_ServiceCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ServiceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommentText = table.Column<string>(type: "TEXT", nullable: false),
                    IsInternal = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseComments_ServiceCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ServiceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivities_ActivityTypeId",
                table: "CaseActivities",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivities_AssignedToUserId",
                table: "CaseActivities",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivities_CaseId_ActivityDate",
                table: "CaseActivities",
                columns: new[] { "CaseId", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivities_PriorityId",
                table: "CaseActivities",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivities_StatusId",
                table: "CaseActivities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseComments_CaseId_CreatedAt",
                table: "CaseComments",
                columns: new[] { "CaseId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_AccountId",
                table: "ServiceCases",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_AssignedToUserId",
                table: "ServiceCases",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_CaseNumber",
                table: "ServiceCases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_CaseStatusId",
                table: "ServiceCases",
                column: "CaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_CategoryId",
                table: "ServiceCases",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_ContactId",
                table: "ServiceCases",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_DueAt",
                table: "ServiceCases",
                column: "DueAt");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_EscalatedToUserId",
                table: "ServiceCases",
                column: "EscalatedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_OpportunityId",
                table: "ServiceCases",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_OwnerTeamId",
                table: "ServiceCases",
                column: "OwnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_OwnerUserId",
                table: "ServiceCases",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_PriorityId",
                table: "ServiceCases",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_SeverityId",
                table: "ServiceCases",
                column: "SeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCases_SourceId",
                table: "ServiceCases",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseActivities");

            migrationBuilder.DropTable(
                name: "CaseComments");

            migrationBuilder.DropTable(
                name: "ServiceCases");
        }
    }
}
