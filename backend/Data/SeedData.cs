using backend.Entities;
using backend.Services;
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
            "Dashboard.View", "Activities.View", "Security.View", "Configuration.View",
            "Reports.View",
            "SecurityPolicies.View", "SecurityPolicies.Create", "SecurityPolicies.Update", "SecurityPolicies.Delete", "SecurityPolicies.ViewSensitiveFields",
            "Settings.View", "Settings.Update",
            "AuditLogs.View",
            "ReferenceData.View", "ReferenceData.Create", "ReferenceData.Update", "ReferenceData.Delete",
            "Accounts.View", "Accounts.Create", "Accounts.Update", "Accounts.Delete",
            "Contacts.View", "Contacts.Create", "Contacts.Update", "Contacts.Delete", "Contacts.SetPrimary",
            "ContactCommunications.View", "ContactCommunications.Create", "ContactCommunications.Update", "ContactCommunications.Delete",
            "ContactInteractions.View", "ContactInteractions.Create", "ContactInteractions.Update", "ContactInteractions.Delete",
            "Leads.View", "Leads.Create", "Leads.Update", "Leads.Delete", "Leads.Assign", "Leads.Qualify", "Leads.Disqualify", "Leads.Convert", "Leads.Score", "Leads.ViewTimeline",
            "LeadActivities.View", "LeadActivities.Create", "LeadActivities.Update", "LeadActivities.Delete", "LeadActivities.Complete",
            "LeadScoreRules.View", "LeadScoreRules.Create", "LeadScoreRules.Update", "LeadScoreRules.Delete", "LeadScoreRules.Run",
            "NumberSequences.View", "NumberSequences.Create", "NumberSequences.Update", "NumberSequences.Delete", "NumberSequences.Preview", "NumberSequences.Reset",
            "Opportunities.View", "Opportunities.Create", "Opportunities.Update", "Opportunities.Delete", "Opportunities.AssignOwner", "Opportunities.ChangeStage", "Opportunities.MarkWon", "Opportunities.MarkLost", "Opportunities.ViewPipeline", "Opportunities.ViewTimeline",
            "OpportunityProducts.View", "OpportunityProducts.Create", "OpportunityProducts.Update", "OpportunityProducts.Delete",
            "OpportunityCompetitors.View", "OpportunityCompetitors.Create", "OpportunityCompetitors.Update", "OpportunityCompetitors.Delete", "OpportunityCompetitors.SetPrimary",
            "OpportunityActivities.View", "OpportunityActivities.Create", "OpportunityActivities.Update", "OpportunityActivities.Delete", "OpportunityActivities.Complete",
            "Pipeline.View", "Pipeline.Manage", "Pipeline.MoveStage",
            "Forecasts.View", "Forecasts.Create", "Forecasts.Update", "Forecasts.Delete",
            "SalesTargets.View", "SalesTargets.Create", "SalesTargets.Update", "SalesTargets.Delete",
            "SalesPerformance.View",
            "Products.View", "Products.Create", "Products.Update", "Products.Delete",
            "ProductCategories.View", "ProductCategories.Create", "ProductCategories.Update", "ProductCategories.Delete",
            "PriceLists.View", "PriceLists.Create", "PriceLists.Update", "PriceLists.Delete",
            "ProductBundles.View", "ProductBundles.Create", "ProductBundles.Update", "ProductBundles.Delete",
            "UnitOfMeasures.View", "UnitOfMeasures.Create", "UnitOfMeasures.Update", "UnitOfMeasures.Delete",
            "Discounts.View", "Discounts.Create", "Discounts.Update", "Discounts.Delete",
            "Quotes.View", "Quotes.Create", "Quotes.Update", "Quotes.Delete", "Quotes.Approve", "Quotes.ConvertToOrder",
            "QuoteLines.View", "QuoteLines.Create", "QuoteLines.Update", "QuoteLines.Delete",
            "Orders.View", "Orders.Create", "Orders.Update", "Orders.Delete", "Orders.Approve", "Orders.GenerateInvoice",
            "OrderLines.View", "OrderLines.Create", "OrderLines.Update", "OrderLines.Delete",
            "Invoices.View", "Invoices.Create", "Invoices.Update", "Invoices.Delete", "Invoices.MarkPaid",
            "InvoiceLines.View", "InvoiceLines.Create", "InvoiceLines.Update", "InvoiceLines.Delete",
            "Cases.View", "Cases.Create", "Cases.Update", "Cases.Delete", "Cases.Resolve", "Cases.Close", "Cases.Reopen",
            "CaseComments.View", "CaseComments.Create", "CaseComments.Delete",
            "Activities.Create", "Activities.Update", "Activities.Delete", "Activities.Complete",
            "ActivityComments.View", "ActivityComments.Create", "ActivityComments.Delete",
            "Documents.View", "Documents.Create", "Documents.Update", "Documents.Delete", "Documents.Download",
            "DocumentVersions.View", "DocumentVersions.Create",
            "Workflows.View", "Workflows.Create", "Workflows.Update", "Workflows.Delete",
            "NotificationTemplates.View", "NotificationTemplates.Create", "NotificationTemplates.Update", "NotificationTemplates.Delete",
            "Notifications.View", "Notifications.Create", "Notifications.Update", "Notifications.Delete", "Notifications.MarkRead",
            "Integrations.View", "Integrations.Create", "Integrations.Update", "Integrations.Delete", "Integrations.TestConnection", "Integrations.RunSync",
            "IntegrationSyncRuns.View",
            "CustomFields.View", "CustomFields.Create", "CustomFields.Update", "CustomFields.Delete",
            "RecordStatuses.View", "RecordStatuses.Create", "RecordStatuses.Update", "RecordStatuses.Delete",
            "AI.View", "AITemplates.View", "AITemplates.Create", "AITemplates.Update", "AITemplates.Delete",
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

        await EnsureLookupCategoryAsync(db, "Lead Source", "LEAD_SOURCE", new[]
        {
            ("Website", "WEBSITE"), ("Referral", "REFERRAL"), ("Email Campaign", "EMAIL_CAMPAIGN"), ("Social Media", "SOCIAL_MEDIA"),
            ("Event", "EVENT"), ("Phone Inquiry", "PHONE_INQUIRY"), ("Partner", "PARTNER"), ("Imported List", "IMPORTED_LIST"), ("Other", "OTHER")
        });

        await EnsureLookupCategoryAsync(db, "Lead Status", "LEAD_STATUS", new[]
        {
            ("New", "NEW"), ("Open", "OPEN"), ("Contacted", "CONTACTED"), ("Qualified", "QUALIFIED"), ("Disqualified", "DISQUALIFIED"), ("Converted", "CONVERTED")
        });

        await EnsureLookupCategoryAsync(db, "Lead Qualification Status", "LEAD_QUALIFICATION_STATUS", new[]
        {
            ("Unqualified", "UNQUALIFIED"), ("In Review", "IN_REVIEW"), ("Qualified", "QUALIFIED"), ("Disqualified", "DISQUALIFIED")
        });

        await EnsureLookupCategoryAsync(db, "Lead Rating", "LEAD_RATING", new[]
        {
            ("Hot", "HOT"), ("Warm", "WARM"), ("Cold", "COLD")
        });

        await EnsureLookupCategoryAsync(db, "Lead Disqualification Reason", "LEAD_DISQUALIFICATION_REASON", new[]
        {
            ("No Budget", "NO_BUDGET"), ("Not Interested", "NOT_INTERESTED"), ("No Authority", "NO_AUTHORITY"), ("Duplicate", "DUPLICATE"),
            ("Unable To Contact", "UNABLE_TO_CONTACT"), ("Invalid Data", "INVALID_DATA"), ("Competitor Selected", "COMPETITOR_SELECTED"), ("Other", "OTHER")
        });

        await EnsureLookupCategoryAsync(db, "Lead Score Rule Type", "LEAD_SCORE_RULE_TYPE", new[]
        {
            ("Field Completeness", "FIELD_COMPLETENESS"), ("Field Value", "FIELD_VALUE"), ("Activity", "ACTIVITY"), ("Source", "SOURCE"), ("Engagement", "ENGAGEMENT")
        });

        await EnsureLookupCategoryAsync(db, "Industry", "INDUSTRY", new[]
        {
            ("Technology", "TECHNOLOGY"), ("Financial Services", "FINANCIAL_SERVICES"), ("Healthcare", "HEALTHCARE"), ("Manufacturing", "MANUFACTURING"),
            ("Retail", "RETAIL"), ("Education", "EDUCATION"), ("Government", "GOVERNMENT"), ("Professional Services", "PROFESSIONAL_SERVICES"), ("Other", "OTHER")
        });

        await EnsureLookupCategoryAsync(db, "Priority", "PRIORITY", new[]
        {
            ("Low", "LOW"), ("Normal", "NORMAL"), ("High", "HIGH"), ("Urgent", "URGENT")
        });

        await EnsureLookupCategoryAsync(db, "Activity Type", "ACTIVITY_TYPE", new[]
        {
            ("Phone Call", "PHONE_CALL"), ("Email", "EMAIL"), ("Meeting", "MEETING"), ("Task", "TASK"), ("Demo", "DEMO"), ("Follow-up", "FOLLOW_UP")
        });

        await EnsureLookupCategoryAsync(db, "Activity Status", "ACTIVITY_STATUS", new[]
        {
            ("Open", "OPEN"), ("In Progress", "IN_PROGRESS"), ("Completed", "COMPLETED"), ("Cancelled", "CANCELLED"), ("Deferred", "DEFERRED")
        });

        await EnsureLookupCategoryAsync(db, "Activity Outcome", "ACTIVITY_OUTCOME", new[]
        {
            ("Completed Successfully", "COMPLETED_SUCCESS"), ("Completed Partially", "COMPLETED_PARTIAL"), ("No Response", "NO_RESPONSE"),
            ("Rescheduled", "RESCHEDULED"), ("Cancelled", "CANCELLED"), ("Blocked", "BLOCKED")
        });

        await EnsureLookupCategoryAsync(db, "Document Category", "DOCUMENT_CATEGORY", new[]
        {
            ("Contract", "CONTRACT"), ("Proposal", "PROPOSAL"), ("Statement Of Work", "SOW"), ("Invoice Attachment", "INVOICE_ATTACHMENT"),
            ("Case Attachment", "CASE_ATTACHMENT"), ("General", "GENERAL")
        });

        await EnsureLookupCategoryAsync(db, "Document Status", "DOCUMENT_STATUS", new[]
        {
            ("Draft", "DRAFT"), ("Active", "ACTIVE"), ("Archived", "ARCHIVED"), ("Expired", "EXPIRED"), ("Superseded", "SUPERSEDED")
        });

        await EnsureLookupCategoryAsync(db, "Case Status", "CASE_STATUS", new[]
        {
            ("Open", "OPEN"), ("In Progress", "IN_PROGRESS"), ("Pending Customer", "PENDING_CUSTOMER"), ("Resolved", "RESOLVED"), ("Closed", "CLOSED")
        });

        await EnsureLookupCategoryAsync(db, "Case Priority", "CASE_PRIORITY", new[]
        {
            ("Low", "LOW"), ("Normal", "NORMAL"), ("High", "HIGH"), ("Urgent", "URGENT")
        });

        await EnsureLookupCategoryAsync(db, "Case Severity", "CASE_SEVERITY", new[]
        {
            ("Minor", "MINOR"), ("Major", "MAJOR"), ("Critical", "CRITICAL")
        });

        await EnsureLookupCategoryAsync(db, "Case Category", "CASE_CATEGORY", new[]
        {
            ("Question", "QUESTION"), ("Incident", "INCIDENT"), ("Problem", "PROBLEM"), ("Request", "REQUEST")
        });

        await EnsureLookupCategoryAsync(db, "Case Source", "CASE_SOURCE", new[]
        {
            ("Email", "EMAIL"), ("Phone", "PHONE"), ("Web", "WEB"), ("Chat", "CHAT"), ("Portal", "PORTAL")
        });

        await EnsureLookupCategoryAsync(db, "Workflow Type", "WORKFLOW_TYPE", new[]
        {
            ("Approval", "APPROVAL"), ("Automation", "AUTOMATION"), ("Notification", "NOTIFICATION"), ("Escalation", "ESCALATION")
        });

        await EnsureLookupCategoryAsync(db, "Workflow Status", "WORKFLOW_STATUS", new[]
        {
            ("Draft", "DRAFT"), ("Active", "ACTIVE"), ("Inactive", "INACTIVE"), ("Archived", "ARCHIVED")
        });

        await EnsureLookupCategoryAsync(db, "Security Scope Type", "SECURITY_SCOPE_TYPE", new[]
        {
            ("All Records", "ALL"), ("Owner Records", "OWNER"), ("Team Records", "TEAM"), ("Owner Or Team Records", "OWNER_OR_TEAM")
        });

        await EnsureLookupCategoryAsync(db, "Notification Channel", "NOTIFICATION_CHANNEL", new[]
        {
            ("In-App", "IN_APP"), ("Email", "EMAIL"), ("SMS", "SMS"), ("Push", "PUSH")
        });

        await EnsureLookupCategoryAsync(db, "Notification Status", "NOTIFICATION_STATUS", new[]
        {
            ("Unread", "UNREAD"), ("Read", "READ"), ("Archived", "ARCHIVED")
        });

        await EnsureLookupCategoryAsync(db, "Integration Provider", "INTEGRATION_PROVIDER", new[]
        {
            ("API Management", "API_MANAGEMENT"), ("Webhook", "WEBHOOK")
        });

        await EnsureLookupCategoryAsync(db, "Integration Direction", "INTEGRATION_DIRECTION", new[]
        {
            ("Inbound", "INBOUND"), ("Outbound", "OUTBOUND"), ("Bidirectional", "BIDIRECTIONAL")
        });

        await EnsureLookupCategoryAsync(db, "Integration Auth Type", "INTEGRATION_AUTH_TYPE", new[]
        {
            ("None", "NONE"), ("API Key", "API_KEY"), ("OAuth2", "OAUTH2")
        });

        await EnsureLookupCategoryAsync(db, "Integration Sync Status", "INTEGRATION_SYNC_STATUS", new[]
        {
            ("Running", "RUNNING"), ("Success", "SUCCESS"), ("Failed", "FAILED")
        });

        await EnsureLookupCategoryAsync(db, "Integration Trigger Type", "INTEGRATION_TRIGGER_TYPE", new[]
        {
            ("Manual", "MANUAL"), ("Schedule", "SCHEDULE"), ("Webhook", "WEBHOOK")
        });

        await EnsureLookupCategoryAsync(db, "Custom Field Data Type", "CUSTOM_FIELD_DATA_TYPE", new[]
        {
            ("Text", "TEXT"), ("Number", "NUMBER"), ("Date", "DATE"), ("Boolean", "BOOLEAN")
        });

        await EnsureLookupCategoryAsync(db, "Opportunity Stage", "OPPORTUNITY_STAGE", new[]
        {
            ("Qualify", "QUALIFY"), ("Develop", "DEVELOP"), ("Propose", "PROPOSE"), ("Negotiate", "NEGOTIATE"), ("Close", "CLOSE")
        });

        await EnsureLookupCategoryAsync(db, "Opportunity Status", "OPPORTUNITY_STATUS", new[]
        {
            ("Open", "OPEN"), ("Won", "WON"), ("Lost", "LOST"), ("On Hold", "ON_HOLD"), ("Cancelled", "CANCELLED")
        });

        await EnsureLookupCategoryAsync(db, "Sales Process Stage", "SALES_PROCESS_STAGE", new[]
        {
            ("Identify Need", "IDENTIFY_NEED"), ("Confirm Interest", "CONFIRM_INTEREST"), ("Build Solution", "BUILD_SOLUTION"),
            ("Present Proposal", "PRESENT_PROPOSAL"), ("Negotiate Terms", "NEGOTIATE_TERMS"), ("Final Decision", "FINAL_DECISION"), ("Closed", "CLOSED")
        });

        await EnsureLookupCategoryAsync(db, "Opportunity Rating", "OPPORTUNITY_RATING", new[]
        {
            ("Hot", "HOT"), ("Warm", "WARM"), ("Cold", "COLD")
        });

        await EnsureLookupCategoryAsync(db, "Opportunity Source", "OPPORTUNITY_SOURCE", new[]
        {
            ("Lead Conversion", "LEAD_CONVERSION"), ("Existing Customer", "EXISTING_CUSTOMER"), ("Referral", "REFERRAL"),
            ("Campaign", "CAMPAIGN"), ("Partner", "PARTNER"), ("Website", "WEBSITE"), ("Direct Sales", "DIRECT_SALES"), ("Other", "OTHER")
        });

        await EnsureLookupCategoryAsync(db, "Win Reason", "WIN_REASON", new[]
        {
            ("Best Price", "BEST_PRICE"), ("Best Solution", "BEST_SOLUTION"), ("Strong Relationship", "STRONG_RELATIONSHIP"),
            ("Existing Customer", "EXISTING_CUSTOMER"), ("Fast Delivery", "FAST_DELIVERY"), ("Competitor Weakness", "COMPETITOR_WEAKNESS"), ("Other", "OTHER")
        });

        await EnsureLookupCategoryAsync(db, "Loss Reason", "LOSS_REASON", new[]
        {
            ("Price Too High", "PRICE_TOO_HIGH"), ("Competitor Selected", "COMPETITOR_SELECTED"), ("No Budget", "NO_BUDGET"),
            ("No Decision", "NO_DECISION"), ("Poor Fit", "POOR_FIT"), ("Lost Contact", "LOST_CONTACT"), ("Timeline Changed", "TIMELINE_CHANGED"), ("Other", "OTHER")
        });

        await EnsureLookupCategoryAsync(db, "Competitor Threat Level", "COMPETITOR_THREAT_LEVEL", new[]
        {
            ("Low", "LOW"), ("Medium", "MEDIUM"), ("High", "HIGH"), ("Critical", "CRITICAL")
        });

        await EnsureLookupCategoryAsync(db, "Currency", "CURRENCY", new[]
        {
            ("US Dollar", "USD"), ("Euro", "EUR"), ("British Pound", "GBP"), ("South African Rand", "ZAR")
        });

        await EnsureLookupCategoryAsync(db, "Sales Target Type", "SALES_TARGET_TYPE", new[]
        {
            ("Revenue", "REVENUE"), ("Opportunity Count", "OPPORTUNITY_COUNT"), ("New Customers", "NEW_CUSTOMERS"), ("Activities Completed", "ACTIVITIES_COMPLETED")
        });

        await EnsureLookupCategoryAsync(db, "Sales Target Period", "SALES_TARGET_PERIOD", new[]
        {
            ("Monthly", "MONTHLY"), ("Quarterly", "QUARTERLY"), ("Yearly", "YEARLY")
        });

        await EnsureLookupCategoryAsync(db, "Forecast Type", "FORECAST_TYPE", new[]
        {
            ("Conservative", "CONSERVATIVE"), ("Expected", "EXPECTED"), ("Aggressive", "AGGRESSIVE")
        });

        await EnsureLookupCategoryAsync(db, "Product Type", "PRODUCT_TYPE", new[]
        {
            ("Product", "PRODUCT"), ("Service", "SERVICE"), ("Subscription", "SUBSCRIPTION"), ("Bundle", "BUNDLE")
        });

        await EnsureLookupCategoryAsync(db, "Product Status", "PRODUCT_STATUS", new[]
        {
            ("Draft", "DRAFT"), ("Active", "ACTIVE"), ("Inactive", "INACTIVE"), ("Discontinued", "DISCONTINUED")
        });

        await EnsureLookupCategoryAsync(db, "Unit Of Measure", "UNIT_OF_MEASURE", new[]
        {
            ("Each", "EACH"), ("Hour", "HOUR"), ("Day", "DAY"), ("Week", "WEEK"), ("Month", "MONTH"), ("Year", "YEAR"),
            ("Kilogram", "KILOGRAM"), ("Gram", "GRAM"), ("Meter", "METER"), ("Liter", "LITER"), ("Package", "PACKAGE")
        });

        await EnsureLookupCategoryAsync(db, "Discount Type", "DISCOUNT_TYPE", new[]
        {
            ("Percentage", "PERCENTAGE"), ("Fixed Amount", "FIXED_AMOUNT")
        });

        await EnsureLookupCategoryAsync(db, "Quote Status", "QUOTE_STATUS", new[]
        {
            ("Draft", "DRAFT"), ("Sent", "SENT"), ("Accepted", "ACCEPTED"), ("Rejected", "REJECTED"), ("Expired", "EXPIRED"), ("Cancelled", "CANCELLED")
        });

        await EnsureLookupCategoryAsync(db, "Quote Approval Status", "QUOTE_APPROVAL_STATUS", new[]
        {
            ("Not Required", "NOT_REQUIRED"), ("Pending", "PENDING"), ("Approved", "APPROVED"), ("Rejected", "REJECTED")
        });

        await EnsureLookupCategoryAsync(db, "Order Status", "ORDER_STATUS", new[]
        {
            ("Draft", "DRAFT"), ("Pending Approval", "PENDING_APPROVAL"), ("Approved", "APPROVED"), ("Confirmed", "CONFIRMED"),
            ("Partially Delivered", "PARTIALLY_DELIVERED"), ("Delivered", "DELIVERED"), ("Cancelled", "CANCELLED")
        });

        await EnsureLookupCategoryAsync(db, "Order Approval Status", "ORDER_APPROVAL_STATUS", new[]
        {
            ("Not Required", "NOT_REQUIRED"), ("Pending", "PENDING"), ("Approved", "APPROVED"), ("Rejected", "REJECTED")
        });

        await EnsureLookupCategoryAsync(db, "Order Delivery Status", "ORDER_DELIVERY_STATUS", new[]
        {
            ("Pending", "PENDING"), ("In Fulfillment", "IN_FULFILLMENT"), ("Partially Delivered", "PARTIALLY_DELIVERED"), ("Delivered", "DELIVERED"), ("Returned", "RETURNED")
        });

        await EnsureLookupCategoryAsync(db, "Order Billing Status", "ORDER_BILLING_STATUS", new[]
        {
            ("Not Billed", "NOT_BILLED"), ("Partially Billed", "PARTIALLY_BILLED"), ("Billed", "BILLED"), ("Paid", "PAID"), ("Write Off", "WRITE_OFF")
        });

        await EnsureLookupCategoryAsync(db, "Invoice Status", "INVOICE_STATUS", new[]
        {
            ("Draft", "DRAFT"), ("Issued", "ISSUED"), ("Sent", "SENT"), ("Overdue", "OVERDUE"), ("Cancelled", "CANCELLED")
        });

        await EnsureLookupCategoryAsync(db, "Invoice Payment Status", "INVOICE_PAYMENT_STATUS", new[]
        {
            ("Unpaid", "UNPAID"), ("Partially Paid", "PARTIALLY_PAID"), ("Paid", "PAID"), ("Overpaid", "OVERPAID"), ("Written Off", "WRITTEN_OFF")
        });

        await EnsureLookupCategoryAsync(db, "Tax Type", "TAX_TYPE", new[]
        {
            ("Exclusive", "EXCLUSIVE"), ("Inclusive", "INCLUSIVE"), ("Exempt", "EXEMPT")
        });

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

        await EnsureLookupCategoryAsync(db, "Number Sequence Reset Frequency", "NUMBER_SEQUENCE_RESET_FREQUENCY", new[]
        {
            ("Never", "NEVER"), ("Daily", "DAILY"), ("Monthly", "MONTHLY"), ("Yearly", "YEARLY")
        });

        await db.SaveChangesAsync();

        await EnsureNumberSequenceAsync(db, "Account", "ACCOUNT", "ACC", false, "NEVER");
        await EnsureNumberSequenceAsync(db, "Contact", "CONTACT", "CON", false, "NEVER");
        await EnsureNumberSequenceAsync(db, "Lead", "LEAD", "LEAD", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Opportunity", "OPPORTUNITY", "OPP", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Quote", "QUOTE", "QTE", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Order", "ORDER", "ORD", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Invoice", "INVOICE", "INV", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Case", "CASE", "CAS", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Activity", "ACTIVITY", "ACT", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Document", "DOCUMENT", "DOC", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Product", "PRODUCT", "PRD", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Price List", "PRICE_LIST", "PL", true, "YEARLY");
        await EnsureNumberSequenceAsync(db, "Product Bundle", "PRODUCT_BUNDLE", "BND", true, "YEARLY");

        await EnsureLeadScoreRuleAsync(db, "Email exists", "EMAIL_EXISTS", "FIELD_COMPLETENESS", "Email", "exists", null, 10, 10);
        await EnsureLeadScoreRuleAsync(db, "Company name exists", "COMPANY_NAME_EXISTS", "FIELD_COMPLETENESS", "CompanyName", "exists", null, 10, 20);
        await EnsureLeadScoreRuleAsync(db, "Estimated value over 10000", "ESTIMATED_VALUE_OVER_10000", "FIELD_VALUE", "EstimatedValue", "greater than", "10000", 20, 30);
        await EnsureLeadScoreRuleAsync(db, "Referral source", "REFERRAL_SOURCE", "SOURCE", "LeadSource", "equals", "REFERRAL", 15, 40);
        await EnsureLeadScoreRuleAsync(db, "Has completed activity", "HAS_COMPLETED_ACTIVITY", "ACTIVITY", "CompletedActivityCount", "greater than", "0", 10, 50);

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

    private static async Task EnsureLeadScoreRuleAsync(
        AppDbContext db,
        string name,
        string code,
        string ruleTypeCode,
        string? fieldName,
        string? ruleOperator,
        string? compareValue,
        int scoreValue,
        int sortOrder)
    {
        if (await db.LeadScoreRules.AnyAsync(x => x.Code == code))
        {
            return;
        }

        var ruleTypeId = await db.LookupValues
            .Where(x => x.LookupCategory.Code == "LEAD_SCORE_RULE_TYPE" && x.Code == ruleTypeCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (!ruleTypeId.HasValue)
        {
            return;
        }

        db.LeadScoreRules.Add(new LeadScoreRule
        {
            Name = name,
            Code = code,
            RuleTypeId = ruleTypeId.Value,
            FieldName = fieldName,
            Operator = ruleOperator,
            CompareValue = compareValue,
            ScoreValue = scoreValue,
            SortOrder = sortOrder,
            IsActive = true
        });
    }

    private static async Task EnsureNumberSequenceAsync(
        AppDbContext db,
        string entityName,
        string sequenceCode,
        string prefix,
        bool includeYear,
        string resetFrequencyCode)
    {
        if (await db.NumberSequences.IgnoreQueryFilters().AnyAsync(x => x.SequenceCode == sequenceCode))
        {
            return;
        }

        var resetFrequencyId = await db.LookupValues
            .Where(x => x.LookupCategory.Code == "NUMBER_SEQUENCE_RESET_FREQUENCY" && x.Code == resetFrequencyCode)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        var sequence = new NumberSequence
        {
            EntityName = entityName,
            SequenceCode = sequenceCode,
            Prefix = prefix,
            Separator = "-",
            CurrentNumber = 0,
            NextNumber = 1,
            MinimumDigits = 6,
            ResetFrequencyId = resetFrequencyId,
            IncludeYear = includeYear,
            IsActive = true
        };

        NumberSequenceFormatter.RefreshFormatPreview(sequence, DateTime.UtcNow);
        db.NumberSequences.Add(sequence);
    }
}
