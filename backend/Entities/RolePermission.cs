namespace backend.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public AppRole Role { get; set; } = default!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
