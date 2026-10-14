namespace FamFit.Domain.Entities;

/// <summary>
/// Шаблон тренировки — набор активностей с дефолтными значениями для быстрого создания тренировки.
/// </summary>
public sealed class WorkoutTemplate
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid UserId { get; private set; }         // владелец шаблона (null = глобальный шаблон)
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }  // описание шаблона

    // Navigation properties
    public User? User { get; private set; }
    public ICollection<WorkoutTemplateItem> Items { get; private set; } = new List<WorkoutTemplateItem>();

    // EF Core parameterless constructor
    private WorkoutTemplate() { }

    public static WorkoutTemplate Create(Guid userId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));

        return new WorkoutTemplate
        {
            UserId = userId,
            Name = name.Trim(),
            Description = description?.Trim()
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));

        Name = name.Trim();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }
}
