using CRM.Domain.Common;

namespace backend.Entities;

public class Department : ActivatableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }

    public ICollection<Department> Children { get; set; } = new List<Department>();
    public ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
}
