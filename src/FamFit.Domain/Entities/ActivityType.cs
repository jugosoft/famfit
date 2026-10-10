using FamFit.Domain.Enums;

namespace FamFit.Domain.Entities;

/// <summary>
/// Справочный тип активности (например: «Отжимания», «Бег», «Планка»).
/// </summary>
public sealed class ActivityType
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = string.Empty;
    public UnitType UnitType { get; private set; } = UnitType.Count;
    public bool IsActive { get; private set; } = true;

    // EF Core parameterless constructor
    private ActivityType() { }

    public static ActivityType Create(string name, UnitType unitType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));

        return new ActivityType
        {
            Name = name.Trim(),
            UnitType = unitType
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
    }
}
