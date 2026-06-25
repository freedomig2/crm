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
            ("Website", "WEBSITE"), ("Referral", "REFERRAL"), ("Cold Call", "COLD_CALL"), ("Email Campaign", "EMAIL_CAMPAIGN"),
            ("Social Media", "SOCIAL_MEDIA"), ("Trade Show", "TRADE_SHOW"), ("Advertisement", "ADVERTISEMENT"), ("Partner", "PARTNER"),
            ("Existing Customer", "EXISTING_CUSTOMER"), ("Walk-In", "WALK_IN"), ("Direct Inquiry", "DIRECT_INQUIRY"), ("Other", "OTHER")
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
            ("Agriculture", "AGRICULTURE"), ("Banking", "BANKING"), ("Construction", "CONSTRUCTION"), ("Education", "EDUCATION"),
            ("Energy", "ENERGY"), ("Engineering", "ENGINEERING"), ("Financial Services", "FINANCIAL_SERVICES"), ("Government", "GOVERNMENT"),
            ("Healthcare", "HEALTHCARE"), ("Hospitality", "HOSPITALITY"), ("Insurance", "INSURANCE"), ("IT & Technology", "IT_TECHNOLOGY"),
            ("Logistics", "LOGISTICS"), ("Manufacturing", "MANUFACTURING"), ("Mining", "MINING"), ("Retail", "RETAIL"),
            ("Telecommunications", "TELECOMMUNICATIONS"), ("Transport", "TRANSPORT"), ("Utilities", "UTILITIES")
        });

        await EnsureLookupCategoryAsync(db, "Account Type", "ACCOUNT_TYPE", new[]
        {
            ("Customer", "CUSTOMER"), ("Partner", "PARTNER"), ("Vendor", "VENDOR"), ("Reseller", "RESELLER"), ("Distributor", "DISTRIBUTOR")
        });

        await EnsureLookupCategoryAsync(db, "Ownership Type", "OWNERSHIP_TYPE", new[]
        {
            ("Private Company", "PRIVATE_COMPANY"), ("Public Company", "PUBLIC_COMPANY"), ("Government", "GOVERNMENT"), ("NGO", "NGO"),
            ("Partnership", "PARTNERSHIP"), ("Sole Proprietorship", "SOLE_PROPRIETORSHIP"), ("Trust", "TRUST"), ("Cooperative", "COOPERATIVE"),
            ("Joint Venture", "JOINT_VENTURE"), ("Subsidiary", "SUBSIDIARY")
        });

        await EnsureLookupCategoryAsync(db, "Customer Status", "CUSTOMER_STATUS", new[]
        {
            ("Prospect", "PROSPECT"), ("Lead", "LEAD"), ("Active Customer", "ACTIVE_CUSTOMER"), ("Inactive Customer", "INACTIVE_CUSTOMER"),
            ("On Hold", "ON_HOLD"), ("Lost Customer", "LOST_CUSTOMER"), ("Blacklisted", "BLACKLISTED"), ("Partner", "PARTNER"),
            ("Vendor", "VENDOR"), ("Former Customer", "FORMER_CUSTOMER")
        });

        await EnsureLookupCategoryAsync(db, "Customer Segment", "CUSTOMER_SEGMENT", new[]
        {
            ("Enterprise", "ENTERPRISE"), ("Large Business", "LARGE_BUSINESS"), ("Mid-Market", "MID_MARKET"), ("Small Business", "SMALL_BUSINESS"),
            ("Startup", "STARTUP"), ("Government", "GOVERNMENT"), ("Non-Profit", "NON_PROFIT"), ("Education", "EDUCATION"),
            ("Healthcare", "HEALTHCARE"), ("Retail", "RETAIL"), ("Manufacturing", "MANUFACTURING")
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
            ("Critical", "CRITICAL"), ("High", "HIGH"), ("Medium", "MEDIUM"), ("Low", "LOW")
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
            ("Qualify", "QUALIFY"), ("Develop", "DEVELOP"), ("Propose", "PROPOSE"), ("Negotiate", "NEGOTIATE"),
            ("Close", "CLOSE"), ("Won", "WON"), ("Lost", "LOST")
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
            ("Mr", "MR"), ("Mrs", "MRS"), ("Ms", "MS"), ("Dr", "DR"), ("Prof", "PROF"), ("Adv", "ADV"), ("Hon", "HON"), ("Rev", "REV")
        });

        await EnsureLookupCategoryAsync(db, "Gender", "GENDER", new[]
        {
            ("Male", "MALE"), ("Female", "FEMALE"), ("Non-Binary", "NON_BINARY"), ("Prefer Not To Say", "PREFER_NOT_TO_SAY")
        });

        await EnsureLookupCategoryAsync(db, "Contact Method", "CONTACT_METHOD", new[]
        {
            ("Email", "EMAIL"), ("Phone", "PHONE"), ("SMS", "SMS"), ("WhatsApp", "WHATSAPP"), ("Teams", "TEAMS"), ("In Person", "IN_PERSON")
        });

        await EnsureLookupCategoryAsync(db, "Contact Role", "CONTACT_ROLE", new[]
        {
            ("Decision Maker", "DECISION_MAKER"), ("Influencer", "INFLUENCER"), ("Executive Sponsor", "EXECUTIVE_SPONSOR"), ("End User", "END_USER"),
            ("Billing Contact", "BILLING_CONTACT"), ("Technical Contact", "TECHNICAL_CONTACT"), ("Procurement Contact", "PROCUREMENT_CONTACT"),
            ("Legal Contact", "LEGAL_CONTACT"), ("Operations Contact", "OPERATIONS_CONTACT")
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

        await EnforceLookupCategoryAsync(
            db,
            "CUSTOMER_STATUS",
            [
                ("Prospect", "PROSPECT", ["PROSPECT"]),
                ("Lead", "LEAD", ["LEAD"]),
                ("Active Customer", "ACTIVE_CUSTOMER", ["ACTIVE", "CUSTOMER"]),
                ("Inactive Customer", "INACTIVE_CUSTOMER", ["INACTIVE"]),
                ("On Hold", "ON_HOLD", ["HOLD"]),
                ("Lost Customer", "LOST_CUSTOMER", ["LOST"]),
                ("Blacklisted", "BLACKLISTED", ["BLACKLIST"]),
                ("Partner", "PARTNER", ["PARTNER"]),
                ("Vendor", "VENDOR", ["SUPPLIER"]),
                ("Former Customer", "FORMER_CUSTOMER", ["FORMER"])
            ],
            "PROSPECT");

        await EnforceLookupCategoryAsync(
            db,
            "OWNERSHIP_TYPE",
            [
                ("Private Company", "PRIVATE_COMPANY", ["PRIVATE"]),
                ("Public Company", "PUBLIC_COMPANY", ["PUBLIC"]),
                ("Government", "GOVERNMENT", ["STATE"]),
                ("NGO", "NGO", ["NON_PROFIT", "NONPROFIT"]),
                ("Partnership", "PARTNERSHIP", ["PARTNERHIP"]),
                ("Sole Proprietorship", "SOLE_PROPRIETORSHIP", ["SOLE_PROPRIETOR"]),
                ("Trust", "TRUST", ["TRUST"]),
                ("Cooperative", "COOPERATIVE", ["COOP"]),
                ("Joint Venture", "JOINT_VENTURE", ["JV"]),
                ("Subsidiary", "SUBSIDIARY", ["SUBSIDIARY"])
            ],
            "PRIVATE_COMPANY");

        await EnforceLookupCategoryAsync(
            db,
            "CUSTOMER_SEGMENT",
            [
                ("Enterprise", "ENTERPRISE", ["ENTERPRISE"]),
                ("Large Business", "LARGE_BUSINESS", ["LARGE"]),
                ("Mid-Market", "MID_MARKET", ["MIDMARKET", "MID_MARKET"]),
                ("Small Business", "SMALL_BUSINESS", ["SMB", "SMALL"]),
                ("Startup", "STARTUP", ["STARTUP"]),
                ("Government", "GOVERNMENT", ["PUBLIC_SECTOR"]),
                ("Non-Profit", "NON_PROFIT", ["NONPROFIT"]),
                ("Education", "EDUCATION", ["EDU"]),
                ("Healthcare", "HEALTHCARE", ["HEALTH"]),
                ("Retail", "RETAIL", ["RETAIL"]),
                ("Manufacturing", "MANUFACTURING", ["MANUFACTURING"])
            ],
            "SMALL_BUSINESS");

        await EnforceLookupCategoryAsync(
            db,
            "INDUSTRY",
            [
                ("Agriculture", "AGRICULTURE", ["AGRI"]),
                ("Banking", "BANKING", ["BANK"]),
                ("Construction", "CONSTRUCTION", ["CONSTRUCTION"]),
                ("Education", "EDUCATION", ["EDU"]),
                ("Energy", "ENERGY", ["POWER"]),
                ("Engineering", "ENGINEERING", ["ENGINEERING"]),
                ("Financial Services", "FINANCIAL_SERVICES", ["FINANCIAL"]),
                ("Government", "GOVERNMENT", ["PUBLIC_SECTOR"]),
                ("Healthcare", "HEALTHCARE", ["HEALTH"]),
                ("Hospitality", "HOSPITALITY", ["HOSPITALITY"]),
                ("Insurance", "INSURANCE", ["INSURANCE"]),
                ("IT & Technology", "IT_TECHNOLOGY", ["TECHNOLOGY", "IT"]),
                ("Logistics", "LOGISTICS", ["SUPPLY_CHAIN"]),
                ("Manufacturing", "MANUFACTURING", ["MANUFACTURING"]),
                ("Mining", "MINING", ["MINING"]),
                ("Retail", "RETAIL", ["RETAIL"]),
                ("Telecommunications", "TELECOMMUNICATIONS", ["TELECOM"]),
                ("Transport", "TRANSPORT", ["TRANSPORTATION"]),
                ("Utilities", "UTILITIES", ["UTILITY"])
            ],
            "IT_TECHNOLOGY");

        await EnforceLookupCategoryAsync(
            db,
            "LEAD_SOURCE",
            [
                ("Website", "WEBSITE", ["WEB"]),
                ("Referral", "REFERRAL", ["REFERAL"]),
                ("Cold Call", "COLD_CALL", ["COLDCALL"]),
                ("Email Campaign", "EMAIL_CAMPAIGN", ["EMAIL"]),
                ("Social Media", "SOCIAL_MEDIA", ["SOCIAL"]),
                ("Trade Show", "TRADE_SHOW", ["EVENT"]),
                ("Advertisement", "ADVERTISEMENT", ["AD"]),
                ("Partner", "PARTNER", ["PARTNER"]),
                ("Existing Customer", "EXISTING_CUSTOMER", ["CUSTOMER"]),
                ("Walk-In", "WALK_IN", ["WALKIN"]),
                ("Direct Inquiry", "DIRECT_INQUIRY", ["DIRECT"]),
                ("Other", "OTHER", ["OTHER"])
            ],
            "OTHER");

        await EnforceLookupCategoryAsync(
            db,
            "SALUTATION",
            [
                ("Mr", "MR", ["MR"]),
                ("Mrs", "MRS", ["MRS"]),
                ("Ms", "MS", ["MS"]),
                ("Dr", "DR", ["DR"]),
                ("Prof", "PROF", ["PROF"]),
                ("Adv", "ADV", ["ADV"]),
                ("Hon", "HON", ["HON"]),
                ("Rev", "REV", ["REV"])
            ],
            "MR");

        await EnforceLookupCategoryAsync(
            db,
            "GENDER",
            [
                ("Male", "MALE", ["MALE"]),
                ("Female", "FEMALE", ["FEMALE"]),
                ("Non-Binary", "NON_BINARY", ["NONBINARY"]),
                ("Prefer Not To Say", "PREFER_NOT_TO_SAY", ["PREFER_NOT"])
            ],
            "PREFER_NOT_TO_SAY");

        await EnforceLookupCategoryAsync(
            db,
            "CONTACT_METHOD",
            [
                ("Email", "EMAIL", ["EMAIL"]),
                ("Phone", "PHONE", ["WORK_PHONE", "HOME_PHONE", "MOBILE"]),
                ("SMS", "SMS", ["SMS"]),
                ("WhatsApp", "WHATSAPP", ["WHATS_APP"]),
                ("Teams", "TEAMS", ["MICROSOFT_TEAMS"]),
                ("In Person", "IN_PERSON", ["INPERSON"])
            ],
            "EMAIL");

        await EnforceLookupCategoryAsync(
            db,
            "CONTACT_ROLE",
            [
                ("Decision Maker", "DECISION_MAKER", ["DECISIONMAKER"]),
                ("Influencer", "INFLUENCER", ["INFLUENCER"]),
                ("Executive Sponsor", "EXECUTIVE_SPONSOR", ["SPONSOR"]),
                ("End User", "END_USER", ["ENDUSER"]),
                ("Billing Contact", "BILLING_CONTACT", ["BILLING"]),
                ("Technical Contact", "TECHNICAL_CONTACT", ["TECHNICAL"]),
                ("Procurement Contact", "PROCUREMENT_CONTACT", ["PROCUREMENT"]),
                ("Legal Contact", "LEGAL_CONTACT", ["LEGAL"]),
                ("Operations Contact", "OPERATIONS_CONTACT", ["OPERATIONS"])
            ],
            "DECISION_MAKER");

        await EnforceLookupCategoryAsync(
            db,
            "CASE_PRIORITY",
            [
                ("Critical", "CRITICAL", ["URGENT"]),
                ("High", "HIGH", ["HIGH"]),
                ("Medium", "MEDIUM", ["NORMAL"]),
                ("Low", "LOW", ["LOW"])
            ],
            "MEDIUM");

        await EnforceLookupCategoryAsync(
            db,
            "OPPORTUNITY_RATING",
            [
                ("Hot", "HOT", ["HOT"]),
                ("Warm", "WARM", ["WARM"]),
                ("Cold", "COLD", ["COLD"])
            ],
            "WARM");

        await EnforceLookupCategoryAsync(
            db,
            "OPPORTUNITY_STAGE",
            [
                ("Qualify", "QUALIFY", ["QUALIFY"]),
                ("Develop", "DEVELOP", ["DEVELOP"]),
                ("Propose", "PROPOSE", ["PROPOSE"]),
                ("Negotiate", "NEGOTIATE", ["NEGOTIATE"]),
                ("Close", "CLOSE", ["CLOSE"]),
                ("Won", "WON", ["WON"]),
                ("Lost", "LOST", ["LOST"])
            ],
            "QUALIFY");

        await ResolveLookupCodeCollisionsAsync(db);
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
        var category = await db.LookupCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code);
        if (category is null)
        {
            category = new LookupCategory
            {
                Name = name,
                Code = code,
                IsActive = true,
                IsDeleted = false
            };
            db.LookupCategories.Add(category);
            await db.SaveChangesAsync();
        }

        category.Name = name;
        category.IsActive = true;
        category.IsDeleted = false;

        if (values is null)
        {
            return;
        }

        var existingValues = await db.LookupValues
            .IgnoreQueryFilters()
            .Where(x => x.LookupCategoryId == category.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync();

        var sortOrder = 10;
        foreach (var (valueName, valueCode) in values)
        {
            var normalizedCode = valueCode.ToUpperInvariant();
            var existingValue = existingValues.FirstOrDefault(x =>
                x.Code.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase));

            if (existingValue is not null)
            {
                existingValue.Name = valueName;
                existingValue.Code = normalizedCode;
                existingValue.SortOrder = sortOrder;
                existingValue.IsActive = true;
                existingValue.IsDeleted = false;
                sortOrder += 10;
                continue;
            }

            var createdValue = new LookupValue
            {
                LookupCategoryId = category.Id,
                Name = valueName,
                Code = normalizedCode,
                SortOrder = sortOrder,
                IsActive = true,
                IsDeleted = false
            };
            db.LookupValues.Add(createdValue);
            existingValues.Add(createdValue);
            sortOrder += 10;
        }
    }

    private static async Task ResolveLookupCodeCollisionsAsync(AppDbContext db)
    {
        var values = await db.LookupValues
            .IgnoreQueryFilters()
            .OrderBy(x => x.LookupCategoryId)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync();

        var duplicateGroups = values
            .GroupBy(x => new { x.LookupCategoryId, Code = x.Code.ToUpperInvariant() })
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in duplicateGroups)
        {
            var winner = group
                .OrderByDescending(x => !x.IsDeleted)
                .ThenByDescending(x => x.IsActive)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.CreatedAt)
                .First();

            foreach (var duplicate in group)
            {
                if (duplicate.Id == winner.Id)
                {
                    duplicate.IsDeleted = false;
                    duplicate.IsActive = true;
                    continue;
                }

                await RemapLookupReferencesAsync(db, duplicate.Id, winner.Id);
                duplicate.IsActive = false;
                duplicate.IsDeleted = true;
                duplicate.Code = $"ARCHIVED_{duplicate.Id:N}";
            }
        }
    }

    private static async Task EnforceLookupCategoryAsync(
        AppDbContext db,
        string categoryCode,
        IReadOnlyList<(string Name, string Code, string[] Aliases)> allowedValues,
        string fallbackCode)
    {
        var category = await db.LookupCategories.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Code == categoryCode);
        if (category is null)
        {
            category = new LookupCategory
            {
                Name = categoryCode.Replace('_', ' '),
                Code = categoryCode,
                IsActive = true
            };
            db.LookupCategories.Add(category);
            await db.SaveChangesAsync();
        }

        category.IsDeleted = false;
        category.IsActive = true;

        var values = await db.LookupValues
            .IgnoreQueryFilters()
            .Where(x => x.LookupCategoryId == category.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync();

        var canonicalByCode = new Dictionary<string, LookupValue>(StringComparer.OrdinalIgnoreCase);

        var order = 10;
        foreach (var (name, code, _) in allowedValues)
        {
            var normalizedCode = code.ToUpperInvariant();
            var value = values.FirstOrDefault(x => x.Code.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase));
            if (value is null)
            {
                value = new LookupValue
                {
                    LookupCategoryId = category.Id,
                    Name = name,
                    Code = normalizedCode,
                    IsActive = true,
                    IsDeleted = false
                };
                db.LookupValues.Add(value);
                values.Add(value);
            }

            value.Name = name;
            value.SortOrder = order;
            value.IsActive = true;
            value.IsDeleted = false;
            canonicalByCode[normalizedCode] = value;
            order += 10;
        }

        var fallbackValue = canonicalByCode.TryGetValue(fallbackCode.ToUpperInvariant(), out var configuredFallback)
            ? configuredFallback
            : canonicalByCode.Values.First();

        var canonicalIds = new HashSet<Guid>(canonicalByCode.Values.Select(x => x.Id));

        var aliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, code, aliases) in allowedValues)
        {
            var normalizedCode = code.ToUpperInvariant();
            aliasMap[NormalizeLookupToken(normalizedCode)] = normalizedCode;
            aliasMap[NormalizeLookupToken(name)] = normalizedCode;
            foreach (var alias in aliases)
            {
                aliasMap[NormalizeLookupToken(alias)] = normalizedCode;
            }
        }

        foreach (var value in values)
        {
            if (canonicalIds.Contains(value.Id))
            {
                continue;
            }

            var normalizedCode = NormalizeLookupToken(value.Code);
            var normalizedName = NormalizeLookupToken(value.Name);
            var targetCode = aliasMap.TryGetValue(normalizedCode, out var codeMatch)
                ? codeMatch
                : aliasMap.TryGetValue(normalizedName, out var nameMatch)
                    ? nameMatch
                    : fallbackValue.Code;

            if (!canonicalByCode.TryGetValue(targetCode, out var targetValue))
            {
                targetValue = fallbackValue;
            }

            if (targetValue.Id != value.Id)
            {
                await RemapLookupReferencesAsync(db, value.Id, targetValue.Id);
                value.IsActive = false;
                value.IsDeleted = true;
                value.Code = $"ARCHIVED_{value.Id:N}";
            }
        }
    }

    private static string NormalizeLookupToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }

    private static async Task RemapLookupReferencesAsync(AppDbContext db, Guid sourceLookupId, Guid targetLookupId)
    {
        if (sourceLookupId == targetLookupId)
        {
            return;
        }

        foreach (var account in await db.Accounts.Where(x => x.AccountTypeId == sourceLookupId).ToListAsync()) account.AccountTypeId = targetLookupId;
        foreach (var account in await db.Accounts.Where(x => x.IndustryId == sourceLookupId).ToListAsync()) account.IndustryId = targetLookupId;
        foreach (var account in await db.Accounts.Where(x => x.OwnershipTypeId == sourceLookupId).ToListAsync()) account.OwnershipTypeId = targetLookupId;
        foreach (var account in await db.Accounts.Where(x => x.CustomerStatusId == sourceLookupId).ToListAsync()) account.CustomerStatusId = targetLookupId;
        foreach (var account in await db.Accounts.Where(x => x.CustomerSegmentId == sourceLookupId).ToListAsync()) account.CustomerSegmentId = targetLookupId;

        foreach (var contact in await db.Contacts.Where(x => x.ContactRoleId == sourceLookupId).ToListAsync()) contact.ContactRoleId = targetLookupId;
        foreach (var contact in await db.Contacts.Where(x => x.SalutationLookupId == sourceLookupId).ToListAsync()) contact.SalutationLookupId = targetLookupId;
        foreach (var contact in await db.Contacts.Where(x => x.GenderLookupId == sourceLookupId).ToListAsync()) contact.GenderLookupId = targetLookupId;
        foreach (var contact in await db.Contacts.Where(x => x.PreferredContactMethodId == sourceLookupId).ToListAsync()) contact.PreferredContactMethodId = targetLookupId;

        foreach (var lead in await db.Leads.Where(x => x.LeadSourceId == sourceLookupId).ToListAsync()) lead.LeadSourceId = targetLookupId;
        foreach (var lead in await db.Leads.Where(x => x.IndustryId == sourceLookupId).ToListAsync()) lead.IndustryId = targetLookupId;
        foreach (var lead in await db.Leads.Where(x => x.RatingId == sourceLookupId).ToListAsync()) lead.RatingId = targetLookupId;

        foreach (var opportunity in await db.Opportunities.Where(x => x.OpportunityStageId == sourceLookupId).ToListAsync()) opportunity.OpportunityStageId = targetLookupId;
        foreach (var opportunity in await db.Opportunities.Where(x => x.RatingId == sourceLookupId).ToListAsync()) opportunity.RatingId = targetLookupId;
        foreach (var history in await db.OpportunityStageHistory.Where(x => x.NewStageId == sourceLookupId).ToListAsync()) history.NewStageId = targetLookupId;
        foreach (var history in await db.OpportunityStageHistory.Where(x => x.PreviousStageId == sourceLookupId).ToListAsync()) history.PreviousStageId = targetLookupId;

        foreach (var serviceCase in await db.ServiceCases.Where(x => x.PriorityId == sourceLookupId).ToListAsync()) serviceCase.PriorityId = targetLookupId;
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
