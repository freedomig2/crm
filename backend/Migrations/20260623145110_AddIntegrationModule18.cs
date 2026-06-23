using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationModule18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntegrationConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DirectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EndpointUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ApiKeyReference = table.Column<string>(type: "TEXT", nullable: true),
                    LastSyncStatusId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationConnections_LookupValues_AuthTypeId",
                        column: x => x.AuthTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegrationConnections_LookupValues_DirectionId",
                        column: x => x.DirectionId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegrationConnections_LookupValues_LastSyncStatusId",
                        column: x => x.LastSyncStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IntegrationConnections_LookupValues_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationSyncRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IntegrationConnectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TriggerTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RecordsProcessed = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationSyncRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationSyncRuns_IntegrationConnections_IntegrationConnectionId",
                        column: x => x.IntegrationConnectionId,
                        principalTable: "IntegrationConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegrationSyncRuns_LookupValues_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegrationSyncRuns_LookupValues_TriggerTypeId",
                        column: x => x.TriggerTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConnections_AuthTypeId",
                table: "IntegrationConnections",
                column: "AuthTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConnections_Code",
                table: "IntegrationConnections",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConnections_DirectionId",
                table: "IntegrationConnections",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConnections_LastSyncStatusId",
                table: "IntegrationConnections",
                column: "LastSyncStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConnections_ProviderId",
                table: "IntegrationConnections",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSyncRuns_IntegrationConnectionId",
                table: "IntegrationSyncRuns",
                column: "IntegrationConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSyncRuns_StartedAt",
                table: "IntegrationSyncRuns",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSyncRuns_StatusId",
                table: "IntegrationSyncRuns",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationSyncRuns_TriggerTypeId",
                table: "IntegrationSyncRuns",
                column: "TriggerTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationSyncRuns");

            migrationBuilder.DropTable(
                name: "IntegrationConnections");
        }
    }
}
