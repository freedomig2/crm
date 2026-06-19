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
        builder.Entity<OpportunityProduct>().HasIndex(x => x.OpportunityId);
        builder.Entity<OpportunityCompetitor>().HasIndex(x => x.OpportunityId);
        builder.Entity<OpportunityActivity>().HasIndex(x => x.OpportunityId);
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
