using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class NumberSequenceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NumberSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", nullable: false),
                    SequenceCode = table.Column<string>(type: "TEXT", nullable: false),
                    Prefix = table.Column<string>(type: "TEXT", nullable: false),
                    Suffix = table.Column<string>(type: "TEXT", nullable: true),
                    Separator = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "-"),
                    CurrentNumber = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    NextNumber = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 1L),
                    MinimumDigits = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 6),
                    ResetFrequencyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastResetDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IncludeYear = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IncludeMonth = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IncludeDay = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    FormatPreview = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_NumberSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumberSequences_LookupValues_ResetFrequencyId",
                        column: x => x.ResetFrequencyId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_ResetFrequencyId",
                table: "NumberSequences",
                column: "ResetFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_SequenceCode",
                table: "NumberSequences",
                column: "SequenceCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NumberSequences");
        }
    }
}
