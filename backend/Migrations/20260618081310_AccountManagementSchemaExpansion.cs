using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AccountManagementSchemaExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelationshipType",
                table: "AccountRelationships");

            migrationBuilder.RenameColumn(
                name: "Segment",
                table: "CustomerProfiles",
                newName: "TimeZoneId");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Accounts",
                newName: "Website");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "AccountActivities",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "ActivityType",
                table: "AccountActivities",
                newName: "Subject");

            migrationBuilder.AddColumn<decimal>(
                name: "ChurnRiskScore",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerSince",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReviewDate",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LifecycleStageId",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewDate",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentTermsId",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredCurrencyId",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredLanguageId",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RiskRatingId",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SatisfactionScore",
                table: "CustomerProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ContactRoleId",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryContact",
                table: "Contacts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredCommunicationId",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalutationId",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPhone",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountTypeId",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualRevenue",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerSegmentId",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerStatusId",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IndustryId",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainPhone",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfEmployees",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnershipTypeId",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentAccountId",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryContactId",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradingName",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "AccountRelationships",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AccountRelationships",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelationshipTypeId",
                table: "AccountRelationships",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "AccountRelationships",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StrengthId",
                table: "AccountRelationships",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AddressTypeId",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttentionTo",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBilling",
                table: "AccountAddresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "AccountAddresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShipping",
                table: "AccountAddresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Landmark",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Line2",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateProvince",
                table: "AccountAddresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivityDate",
                table: "AccountActivities",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityTypeId",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToUserId",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUpDate",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FollowUpRequired",
                table: "AccountActivities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "AccountActivities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OutcomeId",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PriorityId",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedEntityId",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "AccountActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("UPDATE CustomerProfiles SET TimeZoneId = NULL;");
            migrationBuilder.Sql("UPDATE AccountActivities SET StatusId = NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChurnRiskScore",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "CustomerSince",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "LastReviewDate",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "LifecycleStageId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "NextReviewDate",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "PaymentTermsId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredCurrencyId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredLanguageId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "RiskRatingId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "SatisfactionScore",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ContactRoleId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "IsPrimaryContact",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "MobilePhone",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "PreferredCommunicationId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "SalutationId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "WorkPhone",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountTypeId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AnnualRevenue",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CustomerSegmentId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CustomerStatusId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MainPhone",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NumberOfEmployees",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "OwnershipTypeId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ParentAccountId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PrimaryContactId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TradingName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "AccountRelationships");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AccountRelationships");

            migrationBuilder.DropColumn(
                name: "RelationshipTypeId",
                table: "AccountRelationships");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "AccountRelationships");

            migrationBuilder.DropColumn(
                name: "StrengthId",
                table: "AccountRelationships");

            migrationBuilder.DropColumn(
                name: "AddressTypeId",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "AttentionTo",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "IsBilling",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "IsShipping",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "Landmark",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "Line2",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "StateProvince",
                table: "AccountAddresses");

            migrationBuilder.DropColumn(
                name: "ActivityDate",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "ActivityTypeId",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "FollowUpRequired",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "OutcomeId",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "AccountActivities");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "AccountActivities");

            migrationBuilder.RenameColumn(
                name: "TimeZoneId",
                table: "CustomerProfiles",
                newName: "Segment");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "Accounts",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "AccountActivities",
                newName: "ActivityType");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "AccountActivities",
                newName: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "RelationshipType",
                table: "AccountRelationships",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
