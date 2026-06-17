using backend.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
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

        builder.Entity<AppRole>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Permission>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<RefreshToken>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<PasswordResetToken>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Team>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Department>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<SystemSetting>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AuditLog>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<LookupCategory>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<LookupValue>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<UserProfile>().HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Permission>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<SystemSetting>().HasIndex(x => new { x.Category, x.Key }).IsUnique();
        builder.Entity<LookupCategory>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<LookupValue>().HasIndex(x => new { x.LookupCategoryId, x.Code }).IsUnique();
    }
}
