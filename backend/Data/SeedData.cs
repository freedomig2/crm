using backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        await db.Database.MigrateAsync();

        var permissionNames = new[]
        {
            "Users.View", "Users.Create", "Users.Update", "Users.Delete",
            "Roles.View", "Roles.Create", "Roles.Update", "Roles.Delete",
            "Teams.View", "Teams.Create", "Teams.Update", "Teams.Delete",
            "Departments.View", "Departments.Create", "Departments.Update", "Departments.Delete",
            "Settings.View", "Settings.Update",
            "AuditLogs.View",
            "ReferenceData.View", "ReferenceData.Create", "ReferenceData.Update", "ReferenceData.Delete",
            "Accounts.View", "Accounts.Create", "Accounts.Update", "Accounts.Delete",
            "Contacts.View", "Contacts.Create", "Contacts.Update", "Contacts.Delete",
            "AccountAddresses.View", "AccountAddresses.Create", "AccountAddresses.Update", "AccountAddresses.Delete",
            "CustomerProfiles.View", "CustomerProfiles.Create", "CustomerProfiles.Update", "CustomerProfiles.Delete",
            "AccountRelationships.View", "AccountRelationships.Create", "AccountRelationships.Update", "AccountRelationships.Delete",
            "AccountActivities.View", "AccountActivities.Create", "AccountActivities.Update", "AccountActivities.Delete"
        };

        foreach (var fullName in permissionNames)
        {
            if (await db.Permissions.AnyAsync(x => x.Name == fullName))
            {
                continue;
            }

            var split = fullName.Split('.');
            db.Permissions.Add(new Permission
            {
                Name = fullName,
                Module = split[0],
                Action = split[1]
            });
        }

        var defaultRoles = new[]
        {
            "System Administrator",
            "Sales Manager",
            "Sales User",
            "Customer Service Manager",
            "Customer Service User",
            "Read Only User"
        };

        foreach (var roleName in defaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Description = null
                });
            }
        }

        await db.SaveChangesAsync();

        var adminRole = await roleManager.FindByNameAsync("System Administrator");
        if (adminRole is not null)
        {
            var allPermissions = await db.Permissions.ToListAsync();
            foreach (var permission in allPermissions)
            {
                if (!await db.RolePermissions.AnyAsync(x => x.RoleId == adminRole.Id && x.PermissionId == permission.Id))
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        var adminEmail = "admin@crm.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsEnabled = true
            };

            await userManager.CreateAsync(adminUser, "Admin@12345");
        }

        if (adminRole is not null && !await userManager.IsInRoleAsync(adminUser, adminRole.Name!))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
        }

        var lookupDefaults = new[]
        {
            ("Lead Source", "LEAD_SOURCE"),
            ("Industry", "INDUSTRY"),
            ("Priority", "PRIORITY"),
            ("Case Status", "CASE_STATUS"),
            ("Opportunity Stage", "OPPORTUNITY_STAGE")
        };

        foreach (var (name, code) in lookupDefaults)
        {
            if (!await db.LookupCategories.AnyAsync(x => x.Code == code))
            {
                db.LookupCategories.Add(new LookupCategory
                {
                    Name = name,
                    Code = code
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
