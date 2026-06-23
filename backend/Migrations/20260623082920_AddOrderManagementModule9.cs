using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderManagementModule9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderNumber = table.Column<string>(type: "TEXT", nullable: false),
                    QuoteId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OpportunityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CurrencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApprovalStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeliveryStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BillingStatusId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BillingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubtotalAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConvertedInvoiceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConvertedInvoiceAt = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_LookupValues_ApprovalStatusId",
                        column: x => x.ApprovalStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_LookupValues_BillingStatusId",
                        column: x => x.BillingStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_LookupValues_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_LookupValues_DeliveryStatusId",
                        column: x => x.DeliveryStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_LookupValues_OrderStatusId",
                        column: x => x.OrderStatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_Teams_OwnerTeamId",
                        column: x => x.OwnerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_OrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLines_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLines_ProductBundles_ProductBundleId",
                        column: x => x.ProductBundleId,
                        principalTable: "ProductBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLines_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_OrderId_SortOrder",
                table: "OrderLines",
                columns: new[] { "OrderId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_ProductBundleId",
                table: "OrderLines",
                column: "ProductBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_ProductId",
                table: "OrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_UnitOfMeasureId",
                table: "OrderLines",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AccountId",
                table: "Orders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApprovalStatusId",
                table: "Orders",
                column: "ApprovalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApprovedById",
                table: "Orders",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BillingStatusId",
                table: "Orders",
                column: "BillingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ContactId",
                table: "Orders",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CurrencyId",
                table: "Orders",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryStatusId",
                table: "Orders",
                column: "DeliveryStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OpportunityId",
                table: "Orders",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OwnerTeamId",
                table: "Orders",
                column: "OwnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OwnerUserId",
                table: "Orders",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_QuoteId",
                table: "Orders",
                column: "QuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderLines");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
