using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteManagementModule8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuoteNumber = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PriceListId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuoteStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApprovalStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubtotalAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    TermsAndConditions = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConvertedOrderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_AspNetUsers_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Quotes_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_LookupValues_ApprovalStatusId",
                        column: x => x.ApprovalStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_LookupValues_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_LookupValues_QuoteStatusId",
                        column: x => x.QuoteStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_Teams_OwnerTeamId",
                        column: x => x.OwnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuoteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProductBundleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    UnitOfMeasureId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    TaxRate = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    LineTotal = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
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
                    table.PrimaryKey("PK_QuoteLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteLines_ProductBundles_ProductBundleId",
                        column: x => x.ProductBundleId,
                        principalTable: "ProductBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteLines_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteLines_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_ProductBundleId",
                table: "QuoteLines",
                column: "ProductBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_ProductId",
                table: "QuoteLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_QuoteId_SortOrder",
                table: "QuoteLines",
                columns: new[] { "QuoteId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_UnitOfMeasureId",
                table: "QuoteLines",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_AccountId",
                table: "Quotes",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ApprovalStatusId",
                table: "Quotes",
                column: "ApprovalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ApprovedById",
                table: "Quotes",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ContactId",
                table: "Quotes",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CurrencyId",
                table: "Quotes",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_OpportunityId",
                table: "Quotes",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_OwnerTeamId",
                table: "Quotes",
                column: "OwnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_OwnerUserId",
                table: "Quotes",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_PriceListId",
                table: "Quotes",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_QuoteNumber",
                table: "Quotes",
                column: "QuoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_QuoteStatusId",
                table: "Quotes",
                column: "QuoteStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteLines");

            migrationBuilder.DropTable(
                name: "Quotes");
        }
    }
}
