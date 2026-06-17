namespace backend.Entities;

public enum SettingDataType
{
    String = 1,
    Number = 2,
    Boolean = 3,
    Json = 4
}

public class SystemSetting : AuditableEntity
{
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SettingDataType DataType { get; set; }
    public string? Description { get; set; }
}
