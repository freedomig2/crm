using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ContactManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SalutationId",
                table: "Contacts",
                newName: "SalutationLookupId");

            migrationBuilder.RenameColumn(
                name: "PreferredCommunicationId",
                table: "Contacts",
                newName: "PreferredContactMethodId");

            migrationBuilder.RenameColumn(
                name: "DepartmentName",
                table: "Contacts",
                newName: "Department");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Contacts");

            migrationBuilder.AddColumn<string>(
                name: "AlternateEmail",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailOptIn",
                table: "Contacts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "GenderLookupId",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomePhone",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarketingConsent",
                table: "Contacts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneOptIn",
                table: "Contacts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredLanguageId",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredName",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredTimeZoneId",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SMSOptIn",
                table: "Contacts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE Contacts SET ContactNumber = 'CON-' || substr('000000' || rowid, -6, 6) WHERE ContactNumber = ''");

            migrationBuilder.Sql(
                "UPDATE Contacts SET FullName = trim(coalesce(FirstName, '') || ' ' || coalesce(MiddleName || ' ', '') || coalesce(LastName, '')) WHERE FullName = ''");

            migrationBuilder.CreateTable(
                name: "ContactCommunications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommunicationTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_ContactCommunications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactCommunications_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactCommunications_LookupValues_CommunicationTypeId",
                        column: x => x.CommunicationTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactInteractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContactId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InteractionTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    InteractionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Outcome = table.Column<string>(type: "TEXT", nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_ContactInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactInteractions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactInteractions_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactInteractions_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactInteractions_LookupValues_InteractionTypeId",
                        column: x => x.InteractionTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_AccountId",
                table: "Contacts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ContactNumber",
                table: "Contacts",
                column: "ContactNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ContactRoleId",
                table: "Contacts",
                column: "ContactRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_GenderLookupId",
                table: "Contacts",
                column: "GenderLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PreferredContactMethodId",
                table: "Contacts",
                column: "PreferredContactMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PreferredLanguageId",
                table: "Contacts",
                column: "PreferredLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PreferredTimeZoneId",
                table: "Contacts",
                column: "PreferredTimeZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_SalutationLookupId",
                table: "Contacts",
                column: "SalutationLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PrimaryContactId",
                table: "Accounts",
                column: "PrimaryContactId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountActivities_ContactId",
                table: "AccountActivities",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactCommunications_CommunicationTypeId",
                table: "ContactCommunications",
                column: "CommunicationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactCommunications_ContactId",
                table: "ContactCommunications",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInteractions_AccountId",
                table: "ContactInteractions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInteractions_AssignedToUserId",
                table: "ContactInteractions",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInteractions_ContactId",
                table: "ContactInteractions",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInteractions_InteractionTypeId",
                table: "ContactInteractions",
                column: "InteractionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountActivities_Contacts_ContactId",
                table: "AccountActivities",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Contacts_PrimaryContactId",
                table: "Accounts",
                column: "PrimaryContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Accounts_AccountId",
                table: "Contacts",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupValues_ContactRoleId",
                table: "Contacts",
                column: "ContactRoleId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupValues_GenderLookupId",
                table: "Contacts",
                column: "GenderLookupId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupValues_PreferredContactMethodId",
                table: "Contacts",
                column: "PreferredContactMethodId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupValues_PreferredLanguageId",
                table: "Contacts",
                column: "PreferredLanguageId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupValues_PreferredTimeZoneId",
                table: "Contacts",
                column: "PreferredTimeZoneId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_LookupValues_SalutationLookupId",
                table: "Contacts",
                column: "SalutationLookupId",
                principalTable: "LookupValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountActivities_Contacts_ContactId",
                table: "AccountActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Contacts_PrimaryContactId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Accounts_AccountId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupValues_ContactRoleId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupValues_GenderLookupId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupValues_PreferredContactMethodId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupValues_PreferredLanguageId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupValues_PreferredTimeZoneId",
                table: "Contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_LookupValues_SalutationLookupId",
                table: "Contacts");

            migrationBuilder.DropTable(
                name: "ContactCommunications");

            migrationBuilder.DropTable(
                name: "ContactInteractions");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_AccountId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_ContactNumber",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_ContactRoleId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_GenderLookupId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PreferredContactMethodId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PreferredLanguageId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_PreferredTimeZoneId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_SalutationLookupId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_PrimaryContactId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_AccountActivities_ContactId",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "AlternateEmail",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "EmailOptIn",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "GenderLookupId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "HomePhone",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "MarketingConsent",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PhoneOptIn",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PreferredLanguageId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PreferredName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PreferredTimeZoneId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "SMSOptIn",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "SalutationLookupId",
                table: "Contacts",
                newName: "SalutationId");

            migrationBuilder.RenameColumn(
                name: "PreferredContactMethodId",
                table: "Contacts",
                newName: "PreferredCommunicationId");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Contacts",
                newName: "DepartmentName");

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Contacts",
                type: "TEXT",
                nullable: true);
        }
    }
}
