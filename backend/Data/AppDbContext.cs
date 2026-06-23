using backend.Entities;
using backend.Middleware;
using CRM.Domain.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace backend.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    private readonly ICurrentUserContext? _currentUserContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext? currentUserContext = null) : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<UserTeam> UserTeams => Set<UserTeam>();
    public DbSet<UserDepartment> UserDepartments => Set<UserDepartment>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LookupCategory> LookupCategories => Set<LookupCategory>();
    public DbSet<LookupValue> LookupValues => Set<LookupValue>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<AccountAddress> AccountAddresses => Set<AccountAddress>();
    public DbSet<AccountRelationship> AccountRelationships => Set<AccountRelationship>();
    public DbSet<AccountActivity> AccountActivities => Set<AccountActivity>();
    public DbSet<ContactCommunication> ContactCommunications => Set<ContactCommunication>();
    public DbSet<ContactInteraction> ContactInteractions => Set<ContactInteraction>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();
    public DbSet<LeadScoreRule> LeadScoreRules => Set<LeadScoreRule>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<OpportunityProduct> OpportunityProducts => Set<OpportunityProduct>();
    public DbSet<OpportunityCompetitor> OpportunityCompetitors => Set<OpportunityCompetitor>();
    public DbSet<OpportunityActivity> OpportunityActivities => Set<OpportunityActivity>();
    public DbSet<OpportunityStageHistory> OpportunityStageHistory => Set<OpportunityStageHistory>();
    public DbSet<SalesTarget> SalesTargets => Set<SalesTarget>();
    public DbSet<RevenueForecast> RevenueForecasts => Set<RevenueForecast>();
    public DbSet<SalesPerformanceSnapshot> SalesPerformanceSnapshots => Set<SalesPerformanceSnapshot>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<ProductBundle> ProductBundles => Set<ProductBundle>();
    public DbSet<ProductBundleItem> ProductBundleItems => Set<ProductBundleItem>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteLine> QuoteLines => Set<QuoteLine>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<ServiceCase> ServiceCases => Set<ServiceCase>();
    public DbSet<CaseComment> CaseComments => Set<CaseComment>();
    public DbSet<CaseActivity> CaseActivities => Set<CaseActivity>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityComment> ActivityComments => Set<ActivityComment>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<SecurityPolicy> SecurityPolicies => Set<SecurityPolicy>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<IntegrationConnection> IntegrationConnections => Set<IntegrationConnection>();
    public DbSet<IntegrationSyncRun> IntegrationSyncRuns => Set<IntegrationSyncRun>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<RecordStatusDefinition> RecordStatusDefinitions => Set<RecordStatusDefinition>();
    public DbSet<AiPromptTemplate> AiPromptTemplates => Set<AiPromptTemplate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        builder.Entity<UserTeam>().HasKey(x => new { x.UserId, x.TeamId });
        builder.Entity<UserDepartment>().HasKey(x => new { x.UserId, x.DepartmentId });

        builder.Entity<Department>()
            .HasOne(x => x.ParentDepartment)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Team>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Account>()
            .HasOne(x => x.PrimaryContact)
            .WithMany()
            .HasForeignKey(x => x.PrimaryContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contact>()
            .HasOne(x => x.Account)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contact>()
            .HasOne(x => x.ContactRole)
            .WithMany()
            .HasForeignKey(x => x.ContactRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contact>()
            .HasOne(x => x.Salutation)
            .WithMany()
            .HasForeignKey(x => x.SalutationLookupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contact>()
            .HasOne(x => x.Gender)
            .WithMany()
            .HasForeignKey(x => x.GenderLookupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contact>()
            .HasOne(x => x.PreferredContactMethod)
            .WithMany()
            .HasForeignKey(x => x.PreferredContactMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contact>()
            .HasOne(x => x.PreferredLanguage)
            .WithMany()
            .HasForeignKey(x => x.PreferredLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Contact>()
            .HasOne(x => x.PreferredTimeZone)
            .WithMany()
            .HasForeignKey(x => x.PreferredTimeZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContactCommunication>()
            .HasOne(x => x.Contact)
            .WithMany(x => x.Communications)
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContactCommunication>()
            .HasOne(x => x.CommunicationType)
            .WithMany()
            .HasForeignKey(x => x.CommunicationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContactInteraction>()
            .HasOne(x => x.Contact)
            .WithMany(x => x.Interactions)
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContactInteraction>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContactInteraction>()
            .HasOne(x => x.InteractionType)
            .WithMany()
            .HasForeignKey(x => x.InteractionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContactInteraction>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AccountActivity>()
            .HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.LeadSource)
            .WithMany()
            .HasForeignKey(x => x.LeadSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.LeadStatus)
            .WithMany()
            .HasForeignKey(x => x.LeadStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.QualificationStatus)
            .WithMany()
            .HasForeignKey(x => x.QualificationStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.Rating)
            .WithMany()
            .HasForeignKey(x => x.RatingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.Industry)
            .WithMany()
            .HasForeignKey(x => x.IndustryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.DisqualifiedReason)
            .WithMany()
            .HasForeignKey(x => x.DisqualifiedReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.AssignedToTeam)
            .WithMany()
            .HasForeignKey(x => x.AssignedToTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.ConvertedAccount)
            .WithMany()
            .HasForeignKey(x => x.ConvertedAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.ConvertedContact)
            .WithMany()
            .HasForeignKey(x => x.ConvertedContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lead>()
            .HasOne(x => x.ConvertedBy)
            .WithMany()
            .HasForeignKey(x => x.ConvertedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadActivity>()
            .HasOne(x => x.Lead)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadActivity>()
            .HasOne(x => x.ActivityType)
            .WithMany()
            .HasForeignKey(x => x.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadActivity>()
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadActivity>()
            .HasOne(x => x.Priority)
            .WithMany()
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadActivity>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadScoreRule>()
            .HasOne(x => x.RuleType)
            .WithMany()
            .HasForeignKey(x => x.RuleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Opportunity>()
            .HasOne(x => x.Lead)
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Opportunity>()
            .HasOne(x => x.OpportunityStage)
            .WithMany()
            .HasForeignKey(x => x.OpportunityStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.OpportunityStatus)
            .WithMany()
            .HasForeignKey(x => x.OpportunityStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.SalesProcessStage)
            .WithMany()
            .HasForeignKey(x => x.SalesProcessStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.Rating)
            .WithMany()
            .HasForeignKey(x => x.RatingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.Priority)
            .WithMany()
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.Source)
            .WithMany()
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.WinReason)
            .WithMany()
            .HasForeignKey(x => x.WinReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.LossReason)
            .WithMany()
            .HasForeignKey(x => x.LossReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.LostToCompetitor)
            .WithMany()
            .HasForeignKey(x => x.LostToCompetitorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Opportunity>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>(entity =>
        {
            entity.Property(x => x.Probability).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<OpportunityProduct>()
            .HasOne(x => x.Opportunity)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityProduct>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityProduct>()
            .Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.Entity<OpportunityCompetitor>()
            .HasOne(x => x.Opportunity)
            .WithMany(x => x.Competitors)
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityCompetitor>()
            .HasOne(x => x.ThreatLevel)
            .WithMany()
            .HasForeignKey(x => x.ThreatLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityCompetitor>()
            .Property(x => x.IsPrimaryCompetitor)
            .HasDefaultValue(false);

        builder.Entity<OpportunityActivity>()
            .HasOne(x => x.Opportunity)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityActivity>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OpportunityActivity>()
            .HasOne(x => x.ActivityType)
            .WithMany()
            .HasForeignKey(x => x.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityActivity>()
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityActivity>()
            .HasOne(x => x.Priority)
            .WithMany()
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityActivity>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OpportunityStageHistory>()
            .HasOne(x => x.Opportunity)
            .WithMany(x => x.StageHistory)
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityStageHistory>()
            .HasOne(x => x.PreviousStage)
            .WithMany()
            .HasForeignKey(x => x.PreviousStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityStageHistory>()
            .HasOne(x => x.NewStage)
            .WithMany()
            .HasForeignKey(x => x.NewStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityStageHistory>()
            .HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SalesTarget>()
            .HasOne(x => x.TargetType)
            .WithMany()
            .HasForeignKey(x => x.TargetTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesTarget>()
            .HasOne(x => x.TargetPeriod)
            .WithMany()
            .HasForeignKey(x => x.TargetPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesTarget>()
            .HasOne(x => x.AssignedUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesTarget>()
            .HasOne(x => x.AssignedTeam)
            .WithMany()
            .HasForeignKey(x => x.AssignedTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesTarget>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesTarget>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesTarget>()
            .Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Entity<RevenueForecast>()
            .HasOne(x => x.ForecastType)
            .WithMany()
            .HasForeignKey(x => x.ForecastTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SalesPerformanceSnapshot>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SalesPerformanceSnapshot>()
            .HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ProductCategory>()
            .HasOne(x => x.ParentCategory)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(x => x.ProductCategory)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(x => x.ProductType)
            .WithMany()
            .HasForeignKey(x => x.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(x => x.UnitOfMeasure)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(x => x.ProductStatus)
            .WithMany()
            .HasForeignKey(x => x.ProductStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .Property(x => x.AllowDiscount)
            .HasDefaultValue(true);

        builder.Entity<Product>()
            .Property(x => x.IsStockItem)
            .HasDefaultValue(false);

        builder.Entity<PriceList>()
            .HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PriceListItem>()
            .HasOne(x => x.PriceList)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PriceListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PriceListItem>()
            .HasOne(x => x.Product)
            .WithMany(x => x.PriceListItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductBundleItem>()
            .HasOne(x => x.ProductBundle)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ProductBundleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductBundleItem>()
            .HasOne(x => x.Product)
            .WithMany(x => x.ProductBundleItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductBundleItem>()
            .Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.Entity<Discount>()
            .HasOne(x => x.DiscountType)
            .WithMany()
            .HasForeignKey(x => x.DiscountTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Discount>()
            .Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Entity<UnitOfMeasure>()
            .Property(x => x.IsDefault)
            .HasDefaultValue(false);

        builder.Entity<Quote>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.Opportunity)
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.PriceList)
            .WithMany()
            .HasForeignKey(x => x.PriceListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.QuoteStatus)
            .WithMany()
            .HasForeignKey(x => x.QuoteStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.ApprovalStatus)
            .WithMany()
            .HasForeignKey(x => x.ApprovalStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Quote>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quote>(entity =>
        {
            entity.Property(x => x.SubtotalAmount).HasDefaultValue(0m);
            entity.Property(x => x.DiscountAmount).HasDefaultValue(0m);
            entity.Property(x => x.TaxAmount).HasDefaultValue(0m);
            entity.Property(x => x.TotalAmount).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<QuoteLine>()
            .HasOne(x => x.Quote)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuoteLine>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuoteLine>()
            .HasOne(x => x.ProductBundle)
            .WithMany()
            .HasForeignKey(x => x.ProductBundleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuoteLine>()
            .HasOne(x => x.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuoteLine>(entity =>
        {
            entity.Property(x => x.DiscountPercent).HasDefaultValue(0m);
            entity.Property(x => x.DiscountAmount).HasDefaultValue(0m);
            entity.Property(x => x.TaxRate).HasDefaultValue(0m);
            entity.Property(x => x.TaxAmount).HasDefaultValue(0m);
            entity.Property(x => x.LineTotal).HasDefaultValue(0m);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
        });

        builder.Entity<Order>()
            .HasOne(x => x.Quote)
            .WithMany()
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasOne(x => x.Opportunity)
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(x => x.OrderStatus)
            .WithMany()
            .HasForeignKey(x => x.OrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(x => x.ApprovalStatus)
            .WithMany()
            .HasForeignKey(x => x.ApprovalStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(x => x.DeliveryStatus)
            .WithMany()
            .HasForeignKey(x => x.DeliveryStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(x => x.BillingStatus)
            .WithMany()
            .HasForeignKey(x => x.BillingStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>(entity =>
        {
            entity.Property(x => x.SubtotalAmount).HasDefaultValue(0m);
            entity.Property(x => x.DiscountAmount).HasDefaultValue(0m);
            entity.Property(x => x.TaxAmount).HasDefaultValue(0m);
            entity.Property(x => x.TotalAmount).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<OrderLine>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderLine>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderLine>()
            .HasOne(x => x.ProductBundle)
            .WithMany()
            .HasForeignKey(x => x.ProductBundleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderLine>()
            .HasOne(x => x.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderLine>(entity =>
        {
            entity.Property(x => x.DiscountPercent).HasDefaultValue(0m);
            entity.Property(x => x.DiscountAmount).HasDefaultValue(0m);
            entity.Property(x => x.TaxRate).HasDefaultValue(0m);
            entity.Property(x => x.TaxAmount).HasDefaultValue(0m);
            entity.Property(x => x.LineTotal).HasDefaultValue(0m);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
        });

        builder.Entity<Invoice>()
            .HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Invoice>()
            .HasOne(x => x.Quote)
            .WithMany()
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Invoice>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Invoice>()
            .HasOne(x => x.Opportunity)
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Invoice>()
            .HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(x => x.InvoiceStatus)
            .WithMany()
            .HasForeignKey(x => x.InvoiceStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(x => x.PaymentStatus)
            .WithMany()
            .HasForeignKey(x => x.PaymentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>(entity =>
        {
            entity.Property(x => x.SubtotalAmount).HasDefaultValue(0m);
            entity.Property(x => x.DiscountAmount).HasDefaultValue(0m);
            entity.Property(x => x.TaxAmount).HasDefaultValue(0m);
            entity.Property(x => x.TotalAmount).HasDefaultValue(0m);
            entity.Property(x => x.PaidAmount).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<InvoiceLine>()
            .HasOne(x => x.Invoice)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InvoiceLine>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InvoiceLine>()
            .HasOne(x => x.ProductBundle)
            .WithMany()
            .HasForeignKey(x => x.ProductBundleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InvoiceLine>()
            .HasOne(x => x.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InvoiceLine>(entity =>
        {
            entity.Property(x => x.DiscountPercent).HasDefaultValue(0m);
            entity.Property(x => x.DiscountAmount).HasDefaultValue(0m);
            entity.Property(x => x.TaxRate).HasDefaultValue(0m);
            entity.Property(x => x.TaxAmount).HasDefaultValue(0m);
            entity.Property(x => x.LineTotal).HasDefaultValue(0m);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
        });

        builder.Entity<ServiceCase>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.Opportunity)
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.CaseStatus)
            .WithMany()
            .HasForeignKey(x => x.CaseStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.Priority)
            .WithMany()
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.Severity)
            .WithMany()
            .HasForeignKey(x => x.SeverityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.Source)
            .WithMany()
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.EscalatedToUser)
            .WithMany()
            .HasForeignKey(x => x.EscalatedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceCase>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceCase>(entity =>
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.OpenedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        builder.Entity<CaseComment>()
            .HasOne(x => x.Case)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.CaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CaseComment>(entity =>
        {
            entity.Property(x => x.IsInternal).HasDefaultValue(false);
        });

        builder.Entity<CaseActivity>()
            .HasOne(x => x.Case)
            .WithMany(x => x.Activities)
            .HasForeignKey(x => x.CaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CaseActivity>()
            .HasOne(x => x.ActivityType)
            .WithMany()
            .HasForeignKey(x => x.ActivityTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CaseActivity>()
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CaseActivity>()
            .HasOne(x => x.Priority)
            .WithMany()
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CaseActivity>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.ActivityType)
            .WithMany()
            .HasForeignKey(x => x.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(x => x.Priority)
            .WithMany()
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.Lead)
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.Opportunity)
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.Case)
            .WithMany()
            .HasForeignKey(x => x.CaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.Outcome)
            .WithMany()
            .HasForeignKey(x => x.OutcomeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>(entity =>
        {
            entity.Property(x => x.IsPrivate).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.ActivityDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        builder.Entity<ActivityComment>()
            .HasOne(x => x.Activity)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ActivityComment>(entity =>
        {
            entity.Property(x => x.IsInternal).HasDefaultValue(false);
        });

        builder.Entity<Document>()
            .HasOne(x => x.DocumentCategory)
            .WithMany()
            .HasForeignKey(x => x.DocumentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Document>()
            .HasOne(x => x.DocumentStatus)
            .WithMany()
            .HasForeignKey(x => x.DocumentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Document>()
            .HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Document>()
            .HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Document>()
            .HasOne(x => x.Lead)
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Document>()
            .HasOne(x => x.Opportunity)
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Document>()
            .HasOne(x => x.Case)
            .WithMany()
            .HasForeignKey(x => x.CaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Document>()
            .HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Document>()
            .HasOne(x => x.OwnerTeam)
            .WithMany()
            .HasForeignKey(x => x.OwnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Document>(entity =>
        {
            entity.Property(x => x.IsConfidential).HasDefaultValue(false);
            entity.Property(x => x.CurrentVersion).HasDefaultValue(1);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<DocumentVersion>()
            .HasOne(x => x.Document)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Workflow>()
            .HasOne(x => x.WorkflowType)
            .WithMany()
            .HasForeignKey(x => x.WorkflowTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Workflow>()
            .HasOne(x => x.WorkflowStatus)
            .WithMany()
            .HasForeignKey(x => x.WorkflowStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Workflow>(entity =>
        {
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsSystem).HasDefaultValue(false);
            entity.Property(x => x.Version).HasDefaultValue(1);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<SecurityPolicy>()
            .HasOne(x => x.ScopeType)
            .WithMany()
            .HasForeignKey(x => x.ScopeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SecurityPolicy>(entity =>
        {
            entity.Property(x => x.MaskSensitiveFields).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<NotificationTemplate>()
            .HasOne(x => x.Channel)
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NotificationTemplate>(entity =>
        {
            entity.Property(x => x.IsSystem).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<Notification>()
            .HasOne(x => x.RecipientUser)
            .WithMany()
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
            .HasOne(x => x.NotificationTemplate)
            .WithMany()
            .HasForeignKey(x => x.NotificationTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasOne(x => x.Channel)
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
            .HasOne(x => x.Priority)
            .WithMany()
            .HasForeignKey(x => x.PriorityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.IsDismissed).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<IntegrationConnection>()
            .HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IntegrationConnection>()
            .HasOne(x => x.Direction)
            .WithMany()
            .HasForeignKey(x => x.DirectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IntegrationConnection>()
            .HasOne(x => x.AuthType)
            .WithMany()
            .HasForeignKey(x => x.AuthTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IntegrationConnection>()
            .HasOne(x => x.LastSyncStatus)
            .WithMany()
            .HasForeignKey(x => x.LastSyncStatusId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<IntegrationConnection>(entity =>
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<IntegrationSyncRun>()
            .HasOne(x => x.IntegrationConnection)
            .WithMany(x => x.SyncRuns)
            .HasForeignKey(x => x.IntegrationConnectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IntegrationSyncRun>()
            .HasOne(x => x.TriggerType)
            .WithMany()
            .HasForeignKey(x => x.TriggerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IntegrationSyncRun>()
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CustomFieldDefinition>()
            .HasOne(x => x.DataType)
            .WithMany()
            .HasForeignKey(x => x.DataTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CustomFieldDefinition>(entity =>
        {
            entity.Property(x => x.IsRequired).HasDefaultValue(false);
            entity.Property(x => x.IsIndexed).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<RecordStatusDefinition>(entity =>
        {
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsClosedState).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<AiPromptTemplate>(entity =>
        {
            entity.Property(x => x.IsSystem).HasDefaultValue(false);
            entity.Property(x => x.Version).HasDefaultValue(1);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<NumberSequence>()
            .HasOne(x => x.ResetFrequency)
            .WithMany()
            .HasForeignKey(x => x.ResetFrequencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NumberSequence>(entity =>
        {
            entity.Property(x => x.Separator).HasDefaultValue("-");
            entity.Property(x => x.CurrentNumber).HasDefaultValue(0L);
            entity.Property(x => x.NextNumber).HasDefaultValue(1L);
            entity.Property(x => x.MinimumDigits).HasDefaultValue(6);
            entity.Property(x => x.IncludeYear).HasDefaultValue(false);
            entity.Property(x => x.IncludeMonth).HasDefaultValue(false);
            entity.Property(x => x.IncludeDay).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        builder.Entity<AppRole>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AppUser>().HasQueryFilter(x => !x.IsDeleted);

        ApplyBaseEntityQueryFilters(builder);

        builder.Entity<Permission>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<SystemSetting>().HasIndex(x => new { x.Category, x.Key }).IsUnique();
        builder.Entity<LookupCategory>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<LookupValue>().HasIndex(x => new { x.LookupCategoryId, x.Code }).IsUnique();
        builder.Entity<Contact>().HasIndex(x => x.ContactNumber).IsUnique();
        builder.Entity<Lead>().HasIndex(x => x.LeadNumber).IsUnique();
        builder.Entity<LeadScoreRule>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<NumberSequence>().HasIndex(x => x.SequenceCode).IsUnique();
        builder.Entity<Opportunity>().HasIndex(x => x.OpportunityNumber).IsUnique();
        builder.Entity<Opportunity>().HasIndex(x => x.AccountId);
        builder.Entity<Opportunity>().HasIndex(x => x.OpportunityStageId);
        builder.Entity<Opportunity>().HasIndex(x => x.OpportunityStatusId);
        builder.Entity<Opportunity>().HasIndex(x => x.EstimatedCloseDate);
        builder.Entity<Product>().HasIndex(x => x.ProductCode).IsUnique();
        builder.Entity<Product>().HasIndex(x => x.Name);
        builder.Entity<ProductCategory>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<ProductCategory>().HasIndex(x => x.Name);
        builder.Entity<PriceList>().HasIndex(x => x.PriceListNumber).IsUnique();
        builder.Entity<PriceList>().HasIndex(x => new { x.CurrencyId, x.IsDefault });
        builder.Entity<PriceListItem>().HasIndex(x => new { x.PriceListId, x.ProductId, x.MinimumQuantity, x.MaximumQuantity });
        builder.Entity<ProductBundle>().HasIndex(x => x.BundleCode).IsUnique();
        builder.Entity<ProductBundle>().HasIndex(x => x.Name);
        builder.Entity<ProductBundleItem>().HasIndex(x => new { x.ProductBundleId, x.ProductId });
        builder.Entity<UnitOfMeasure>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<UnitOfMeasure>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Discount>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Discount>().HasIndex(x => x.Name);
        builder.Entity<Quote>().HasIndex(x => x.QuoteNumber).IsUnique();
        builder.Entity<Quote>().HasIndex(x => x.AccountId);
        builder.Entity<Quote>().HasIndex(x => x.OpportunityId);
        builder.Entity<Quote>().HasIndex(x => x.QuoteStatusId);
        builder.Entity<Quote>().HasIndex(x => x.ApprovalStatusId);
        builder.Entity<QuoteLine>().HasIndex(x => new { x.QuoteId, x.SortOrder });
        builder.Entity<Order>().HasIndex(x => x.OrderNumber).IsUnique();
        builder.Entity<Order>().HasIndex(x => x.QuoteId);
        builder.Entity<Order>().HasIndex(x => x.AccountId);
        builder.Entity<Order>().HasIndex(x => x.OpportunityId);
        builder.Entity<Order>().HasIndex(x => x.OrderStatusId);
        builder.Entity<Order>().HasIndex(x => x.ApprovalStatusId);
        builder.Entity<Order>().HasIndex(x => x.DeliveryStatusId);
        builder.Entity<Order>().HasIndex(x => x.BillingStatusId);
        builder.Entity<OrderLine>().HasIndex(x => new { x.OrderId, x.SortOrder });
        builder.Entity<Invoice>().HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.Entity<Invoice>().HasIndex(x => x.OrderId);
        builder.Entity<Invoice>().HasIndex(x => x.QuoteId);
        builder.Entity<Invoice>().HasIndex(x => x.AccountId);
        builder.Entity<Invoice>().HasIndex(x => x.OpportunityId);
        builder.Entity<Invoice>().HasIndex(x => x.InvoiceStatusId);
        builder.Entity<Invoice>().HasIndex(x => x.PaymentStatusId);
        builder.Entity<InvoiceLine>().HasIndex(x => new { x.InvoiceId, x.SortOrder });
        builder.Entity<ServiceCase>().HasIndex(x => x.CaseNumber).IsUnique();
        builder.Entity<ServiceCase>().HasIndex(x => x.AccountId);
        builder.Entity<ServiceCase>().HasIndex(x => x.CaseStatusId);
        builder.Entity<ServiceCase>().HasIndex(x => x.PriorityId);
        builder.Entity<ServiceCase>().HasIndex(x => x.AssignedToUserId);
        builder.Entity<ServiceCase>().HasIndex(x => x.DueAt);
        builder.Entity<CaseComment>().HasIndex(x => new { x.CaseId, x.CreatedAt });
        builder.Entity<CaseActivity>().HasIndex(x => new { x.CaseId, x.ActivityDate });
        builder.Entity<Activity>().HasIndex(x => x.ActivityNumber).IsUnique();
        builder.Entity<Activity>().HasIndex(x => x.ActivityTypeId);
        builder.Entity<Activity>().HasIndex(x => x.StatusId);
        builder.Entity<Activity>().HasIndex(x => x.ActivityDate);
        builder.Entity<Activity>().HasIndex(x => x.AssignedToUserId);
        builder.Entity<Activity>().HasIndex(x => x.DueDate);
        builder.Entity<Activity>().HasIndex(x => x.AccountId);
        builder.Entity<Activity>().HasIndex(x => x.ContactId);
        builder.Entity<Activity>().HasIndex(x => x.LeadId);
        builder.Entity<Activity>().HasIndex(x => x.OpportunityId);
        builder.Entity<Activity>().HasIndex(x => x.CaseId);
        builder.Entity<ActivityComment>().HasIndex(x => new { x.ActivityId, x.CreatedAt });
        builder.Entity<Document>().HasIndex(x => x.DocumentNumber).IsUnique();
        builder.Entity<Document>().HasIndex(x => x.DocumentCategoryId);
        builder.Entity<Document>().HasIndex(x => x.DocumentStatusId);
        builder.Entity<Document>().HasIndex(x => x.EffectiveDate);
        builder.Entity<Document>().HasIndex(x => x.ExpiryDate);
        builder.Entity<Document>().HasIndex(x => x.AccountId);
        builder.Entity<Document>().HasIndex(x => x.ContactId);
        builder.Entity<Document>().HasIndex(x => x.LeadId);
        builder.Entity<Document>().HasIndex(x => x.OpportunityId);
        builder.Entity<Document>().HasIndex(x => x.CaseId);
        builder.Entity<DocumentVersion>().HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
        builder.Entity<Workflow>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Workflow>().HasIndex(x => x.Name);
        builder.Entity<Workflow>().HasIndex(x => x.WorkflowTypeId);
        builder.Entity<Workflow>().HasIndex(x => x.WorkflowStatusId);
        builder.Entity<SecurityPolicy>().HasIndex(x => x.EntityName).IsUnique();
        builder.Entity<SecurityPolicy>().HasIndex(x => x.ScopeTypeId);
        builder.Entity<NotificationTemplate>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<NotificationTemplate>().HasIndex(x => x.ChannelId);
        builder.Entity<Notification>().HasIndex(x => x.RecipientUserId);
        builder.Entity<Notification>().HasIndex(x => x.StatusId);
        builder.Entity<Notification>().HasIndex(x => x.ChannelId);
        builder.Entity<Notification>().HasIndex(x => x.PriorityId);
        builder.Entity<Notification>().HasIndex(x => x.CreatedAt);
        builder.Entity<IntegrationConnection>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<IntegrationConnection>().HasIndex(x => x.ProviderId);
        builder.Entity<IntegrationConnection>().HasIndex(x => x.DirectionId);
        builder.Entity<IntegrationConnection>().HasIndex(x => x.AuthTypeId);
        builder.Entity<IntegrationConnection>().HasIndex(x => x.LastSyncStatusId);
        builder.Entity<IntegrationSyncRun>().HasIndex(x => x.IntegrationConnectionId);
        builder.Entity<IntegrationSyncRun>().HasIndex(x => x.StatusId);
        builder.Entity<IntegrationSyncRun>().HasIndex(x => x.TriggerTypeId);
        builder.Entity<IntegrationSyncRun>().HasIndex(x => x.StartedAt);
        builder.Entity<CustomFieldDefinition>().HasIndex(x => new { x.EntityName, x.FieldKey }).IsUnique();
        builder.Entity<CustomFieldDefinition>().HasIndex(x => x.DataTypeId);
        builder.Entity<RecordStatusDefinition>().HasIndex(x => new { x.EntityName, x.StatusCode }).IsUnique();
        builder.Entity<RecordStatusDefinition>().HasIndex(x => new { x.EntityName, x.SortOrder });
        builder.Entity<AiPromptTemplate>().HasIndex(x => new { x.UseCaseCode, x.Version }).IsUnique();
        builder.Entity<AiPromptTemplate>().HasIndex(x => x.Name);
        builder.Entity<OpportunityProduct>().HasIndex(x => x.OpportunityId);
        builder.Entity<OpportunityCompetitor>().HasIndex(x => x.OpportunityId);
        builder.Entity<OpportunityActivity>().HasIndex(x => x.OpportunityId);
        builder.Entity<OpportunityStageHistory>().HasIndex(x => new { x.OpportunityId, x.ChangedAt });
        builder.Entity<SalesTarget>().HasIndex(x => x.TargetTypeId);
        builder.Entity<SalesTarget>().HasIndex(x => x.TargetPeriodId);
        builder.Entity<SalesTarget>().HasIndex(x => x.AssignedUserId);
        builder.Entity<SalesTarget>().HasIndex(x => x.AssignedTeamId);
        builder.Entity<RevenueForecast>().HasIndex(x => x.ForecastDate);
        builder.Entity<RevenueForecast>().HasIndex(x => new { x.ForecastPeriodStart, x.ForecastPeriodEnd });
        builder.Entity<SalesPerformanceSnapshot>().HasIndex(x => x.SnapshotDate);
    }

    public override int SaveChanges()
    {
        ApplyAuditMetadata();
        var generatedAuditLogs = GenerateEntityAuditLogs();
        if (generatedAuditLogs.Count > 0)
        {
            AuditLogs.AddRange(generatedAuditLogs);
        }

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditMetadata();
        var generatedAuditLogs = GenerateEntityAuditLogs();
        if (generatedAuditLogs.Count > 0)
        {
            AuditLogs.AddRange(generatedAuditLogs);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditMetadata()
    {
        var now = DateTime.UtcNow;
        var userId = _currentUserContext?.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Id == Guid.Empty)
                {
                    entry.Entity.Id = Guid.NewGuid();
                }

                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }

                entry.Entity.CreatedById ??= userId;
                entry.Entity.UpdatedAt = null;
                entry.Entity.UpdatedById = null;
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedById = userId;
            }
        }
    }

    private List<AuditLog> GenerateEntityAuditLogs()
    {
        var userId = _currentUserContext?.UserId;
        var now = DateTime.UtcNow;
        var logs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog)
            {
                continue;
            }

            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            var action = ResolveAuditAction(entry);
            if (action is null)
            {
                continue;
            }

            var (oldValues, newValues) = BuildAuditValues(entry);
            logs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = entry.Entity.Id.ToString(),
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId,
                CreatedAt = now,
                CreatedById = userId,
                IsDeleted = false
            });
        }

        return logs;
    }

    private static string? ResolveAuditAction(EntityEntry<BaseEntity> entry)
    {
        if (entry.State == EntityState.Added)
        {
            return "Create";
        }

        if (entry.Property(nameof(BaseEntity.IsDeleted)).IsModified && entry.Entity.IsDeleted)
        {
            return "SoftDelete";
        }

        if (entry.Entity is OwnedEntity ownedEntity)
        {
            var ownerUserChanged = entry.Property(nameof(OwnedEntity.OwnerUserId)).IsModified;
            var ownerTeamChanged = entry.Property(nameof(OwnedEntity.OwnerTeamId)).IsModified;
            if (ownerUserChanged || ownerTeamChanged)
            {
                return "OwnershipChanged";
            }
        }

        if (entry.Entity is ActivatableEntity && entry.Property(nameof(ActivatableEntity.IsActive)).IsModified)
        {
            return "StatusChanged";
        }

        var hasBusinessChanges = entry.Properties.Any(p =>
            p.IsModified &&
            p.Metadata.Name is not nameof(BaseEntity.CreatedAt)
                and not nameof(BaseEntity.CreatedById)
                and not nameof(BaseEntity.UpdatedAt)
                and not nameof(BaseEntity.UpdatedById));

        return hasBusinessChanges ? "Update" : null;
    }

    private static (string? oldValues, string? newValues) BuildAuditValues(EntityEntry<BaseEntity> entry)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.Name is nameof(BaseEntity.CreatedAt)
                or nameof(BaseEntity.CreatedById)
                or nameof(BaseEntity.UpdatedAt)
                or nameof(BaseEntity.UpdatedById)
                or nameof(BaseEntity.TenantId))
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                newValues[property.Metadata.Name] = property.CurrentValue;
                continue;
            }

            if (!property.IsModified)
            {
                continue;
            }

            oldValues[property.Metadata.Name] = property.OriginalValue;
            newValues[property.Metadata.Name] = property.CurrentValue;
        }

        var oldJson = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
        var newJson = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;
        return (oldJson, newJson);
    }

    private static void ApplyBaseEntityQueryFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(AppDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, new object[] { builder });
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : BaseEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(x => !x.IsDeleted);
    }
}
