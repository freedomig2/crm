using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class BaseEntityInheritanceRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_OwnerUserId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserTeams");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserDepartments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RolePermissions");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "UserProfiles",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "UserProfiles",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Teams",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Teams",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "SystemSettings",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "SystemSettings",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "RefreshTokens",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "RefreshTokens",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Permissions",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Permissions",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "PasswordResetTokens",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "PasswordResetTokens",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LookupValues",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "LookupValues",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LookupCategories",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "LookupCategories",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Departments",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Departments",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AuditLogs",
                newName: "UpdatedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "AuditLogs",
                newName: "TenantId");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Teams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerTeamId",
                table: "Teams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "SystemSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "RefreshTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Permissions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "PasswordResetTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "LookupValues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "LookupCategories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Departments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("UPDATE UserProfiles SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE Teams SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE SystemSettings SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE RefreshTokens SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE Permissions SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE PasswordResetTokens SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE LookupValues SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE LookupCategories SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE Departments SET UpdatedById = NULL, TenantId = NULL");
            migrationBuilder.Sql("UPDATE AuditLogs SET UpdatedById = NULL, TenantId = NULL");

            migrationBuilder.CreateTable(
                name: "AccountActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActivityType = table.Column<string>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AccountActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Line1 = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationshipType = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRelationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OwnerTeamId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OwnerTeamId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Segment = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerProfiles", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_OwnerUserId",
                table: "Teams",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_OwnerUserId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "AccountActivities");

            migrationBuilder.DropTable(
                name: "AccountAddresses");

            migrationBuilder.DropTable(
                name: "AccountRelationships");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "OwnerTeamId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "LookupValues");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "LookupCategories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "UserProfiles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "UserProfiles",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Teams",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Teams",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "SystemSettings",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "SystemSettings",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "RefreshTokens",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "RefreshTokens",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Permissions",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Permissions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "PasswordResetTokens",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "PasswordResetTokens",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "LookupValues",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "LookupValues",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "LookupCategories",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "LookupCategories",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "Departments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Departments",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedById",
                table: "AuditLogs",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "AuditLogs",
                newName: "CreatedBy");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserTeams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserDepartments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RolePermissions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_OwnerUserId",
                table: "Teams",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
