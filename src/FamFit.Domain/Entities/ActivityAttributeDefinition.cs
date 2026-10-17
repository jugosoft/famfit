using FamFit.Domain.Enums;

namespace FamFit.Domain.Entities;

/// <summary>
/// Справочник определений атрибутов активности (например: "Подходы", "Повторения", "Вес").
/// Определяет доступные типы атрибутов для всей системы.
/// </summary>
public sealed class ActivityAttributeDefinition
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public AttributeDataType DataType { get; private set; } = AttributeDataType.Integer;
    public string? Unit { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    // Navigation property
    public ICollection<ActivityAttributeValue> AttributeValues { get; private set; } = new List<ActivityAttributeValue>();

    // EF Core parameterless constructor
    private ActivityAttributeDefinition() { }

    public static ActivityAttributeDefinition Create(
        string name,
        string displayName,
        AttributeDataType dataType,
        string? unit = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName cannot be empty.", nameof(displayName));
        if (displayName.Length > 100)
            throw new ArgumentException("DisplayName cannot exceed 100 characters.", nameof(displayName));

        return new ActivityAttributeDefinition
        {
            Name = name.Trim(),
            DisplayName = displayName.Trim(),
            DataType = dataType,
            Unit = unit?.Trim(),
            DisplayOrder = displayOrder
        };
    }

    public void Update(string? newName = null, string? newDisplayName = null, AttributeDataType? dataType = null, string? newUnit = null, bool? isActive = null, int? newDisplayOrder = null)
    {
        if (newName != null)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Name cannot be empty.", nameof(newName));
            if (newName.Length > 100)
                throw new ArgumentException("Name cannot exceed 100 characters.", nameof(newName));
            Name = newName.Trim();
        }

        if (newDisplayName != null)
        {
            if (string.IsNullOrWhiteSpace(newDisplayName))
                throw new ArgumentException("DisplayName cannot be empty.", nameof(newDisplayName));
            if (newDisplayName.Length > 100)
                throw new ArgumentException("DisplayName cannot exceed 100 characters.", nameof(newDisplayName));
            DisplayName = newDisplayName.Trim();
        }

        DataType = dataType ?? DataType;
        Unit = newUnit?.Trim() ?? Unit;
        IsActive = isActive ?? IsActive;
        DisplayOrder = newDisplayOrder ?? DisplayOrder;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.", nameof(newName));
        if (newName.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(newName));
        Name = newName.Trim();
    }

    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new ArgumentException("DisplayName cannot be empty.", nameof(newDisplayName));
        if (newDisplayName.Length > 100)
            throw new ArgumentException("DisplayName cannot exceed 100 characters.", nameof(newDisplayName));
        DisplayName = newDisplayName.Trim();
    }

    public void UpdateUnit(string? newUnit)
    {
        Unit = newUnit?.Trim();
    }

    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        DisplayOrder = newDisplayOrder;
    }
}
