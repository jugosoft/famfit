namespace FamFit.Domain.Entities;

/// <summary>
/// Справочный тип активности (например: «Отжимания», «Бег», «Планка»).
/// Атрибуты хранятся через гибкую систему ActivityTypeAttributeValue.
/// </summary>
public sealed class ActivityType
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public ICollection<ActivityTypeAttributeValue> ActivityTypeAttributeValues { get; private set; } = new List<ActivityTypeAttributeValue>();
    public ICollection<WorkoutTemplateItem> WorkoutTemplateItems { get; private set; } = new List<WorkoutTemplateItem>();
    public ICollection<WorkoutEntry> WorkoutEntries { get; private set; } = new List<WorkoutEntry>();
    public ICollection<ActivityLog> ActivityLogs { get; private set; } = new List<ActivityLog>();

    // EF Core parameterless constructor
    private ActivityType() { }

    public static ActivityType Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));

        return new ActivityType
        {
            Name = name.Trim()
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
