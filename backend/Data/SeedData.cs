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
            "Contacts.View", "Contacts.Create", "Contacts.Update", "Contacts.Delete", "Contacts.SetPrimary",
            "ContactCommunications.View", "ContactCommunications.Create", "ContactCommunications.Update", "ContactCommunications.Delete",
            "ContactInteractions.View", "ContactInteractions.Create", "ContactInteractions.Update", "ContactInteractions.Delete",
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

        await EnsureLookupCategoryAsync(db, "Lead Source", "LEAD_SOURCE");
        await EnsureLookupCategoryAsync(db, "Industry", "INDUSTRY");
        await EnsureLookupCategoryAsync(db, "Priority", "PRIORITY");
        await EnsureLookupCategoryAsync(db, "Case Status", "CASE_STATUS");
        await EnsureLookupCategoryAsync(db, "Opportunity Stage", "OPPORTUNITY_STAGE");

        await EnsureLookupCategoryAsync(db, "Salutation", "SALUTATION", new[]
        {
            ("Mr", "MR"), ("Mrs", "MRS"), ("Ms", "MS"), ("Dr", "DR"), ("Prof", "PROF")
        });

        await EnsureLookupCategoryAsync(db, "Gender", "GENDER", new[]
        {
            ("Male", "MALE"), ("Female", "FEMALE"), ("Non-Binary", "NON_BINARY"), ("Prefer Not To Say", "PREFER_NOT_TO_SAY")
        });

        await EnsureLookupCategoryAsync(db, "Contact Method", "CONTACT_METHOD", new[]
        {
            ("Email", "EMAIL"), ("Phone", "PHONE"), ("Mobile", "MOBILE"), ("SMS", "SMS"), ("WhatsApp", "WHATSAPP"), ("Teams", "TEAMS"), ("In Person", "IN_PERSON")
        });

        await EnsureLookupCategoryAsync(db, "Contact Role", "CONTACT_ROLE", new[]
        {
            ("Decision Maker", "DECISION_MAKER"), ("Influencer", "INFLUENCER"), ("Technical Contact", "TECHNICAL_CONTACT"), ("Billing Contact", "BILLING_CONTACT"),
            ("Procurement Contact", "PROCUREMENT_CONTACT"), ("Executive Sponsor", "EXECUTIVE_SPONSOR"), ("End User", "END_USER")
        });

        await EnsureLookupCategoryAsync(db, "Communication Type", "COMMUNICATION_TYPE", new[]
        {
            ("Email", "EMAIL"), ("Mobile", "MOBILE"), ("Work Phone", "WORK_PHONE"), ("Home Phone", "HOME_PHONE"), ("LinkedIn", "LINKEDIN"),
            ("WhatsApp", "WHATSAPP"), ("Telegram", "TELEGRAM"), ("Skype", "SKYPE"), ("Website", "WEBSITE")
        });

        await EnsureLookupCategoryAsync(db, "Interaction Type", "INTERACTION_TYPE", new[]
        {
            ("Phone Call", "PHONE_CALL"), ("Email", "EMAIL"), ("SMS", "SMS"), ("Meeting", "MEETING"), ("Video Call", "VIDEO_CALL"), ("Site Visit", "SITE_VISIT"), ("Follow-up", "FOLLOW_UP")
        });

        await EnsureLookupCategoryAsync(db, "Language", "LANGUAGE", new[]
        {
            ("English", "ENGLISH"), ("French", "FRENCH"), ("German", "GERMAN"), ("Spanish", "SPANISH"), ("Portuguese", "PORTUGUESE")
        });

        await EnsureLookupCategoryAsync(db, "Time Zone", "TIME_ZONE", new[]
        {
            ("UTC", "UTC"),
            ("Greenwich Mean Time", "GMT"),
            ("Eastern Time", "EASTERN_TIME"),
            ("Central Time", "CENTRAL_TIME"),
            ("Mountain Time", "MOUNTAIN_TIME"),
            ("Pacific Time", "PACIFIC_TIME"),
            ("Central European Time", "CENTRAL_EUROPEAN_TIME"),
            ("South Africa Standard Time", "SOUTH_AFRICA_STANDARD_TIME"),
            ("India Standard Time", "INDIA_STANDARD_TIME"),
            ("China Standard Time", "CHINA_STANDARD_TIME"),
            ("Japan Standard Time", "JAPAN_STANDARD_TIME"),
            ("Australian Eastern Time", "AUSTRALIAN_EASTERN_TIME")
        });

        await db.SaveChangesAsync();
    }

    private static async Task EnsureLookupCategoryAsync(AppDbContext db, string name, string code, IEnumerable<(string Name, string Code)>? values = null)
    {
        var category = await db.LookupCategories.FirstOrDefaultAsync(x => x.Code == code);
        if (category is null)
        {
            category = new LookupCategory
            {
                Name = name,
                Code = code,
                IsActive = true
            };
            db.LookupCategories.Add(category);
            await db.SaveChangesAsync();
        }

        if (values is null)
        {
            return;
        }

        var sortOrder = 10;
        foreach (var (valueName, valueCode) in values)
        {
            if (await db.LookupValues.AnyAsync(x => x.LookupCategoryId == category.Id && x.Code == valueCode))
            {
                sortOrder += 10;
                continue;
            }

            db.LookupValues.Add(new LookupValue
            {
                LookupCategoryId = category.Id,
                Name = valueName,
                Code = valueCode,
                SortOrder = sortOrder,
                IsActive = true
            });
            sortOrder += 10;
        }
    }
}
