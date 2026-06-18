namespace backend.Entities;

public class UserDepartment
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
