using FamFit.Domain.Enums;

namespace FamFit.Domain.Entities;

/// <summary>
/// Конкретное значение атрибута (привязано к определению).
/// Хранит значение в зависимости от типа данных определения.
/// </summary>
public sealed class ActivityAttributeValue
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid DefinitionId { get; private set; }
    public int? IntValue { get; private set; }
    public decimal? DecimalValue { get; private set; }
    public bool BoolValue { get; private set; }
    public string? StringValue { get; private set; }

    // Navigation property
    public ActivityAttributeDefinition? Definition { get; private set; }
    public ICollection<ActivityTypeAttributeValue> ActivityTypeAttributeValues { get; private set; } = new List<ActivityTypeAttributeValue>();
    public ICollection<WorkoutEntryAttributeValue> WorkoutEntryAttributeValues { get; private set; } = new List<WorkoutEntryAttributeValue>();
    public ICollection<WorkoutTemplateItemAttributeValue> WorkoutTemplateItemAttributeValues { get; private set; } = new List<WorkoutTemplateItemAttributeValue>();

    // EF Core parameterless constructor
    private ActivityAttributeValue() { }

    public static ActivityAttributeValue Create(Guid definitionId, int? intValue = null, decimal? decimalValue = null, bool boolValue = false, string? stringValue = null)
    {
        return new ActivityAttributeValue
        {
            DefinitionId = definitionId,
            IntValue = intValue,
            DecimalValue = decimalValue,
            BoolValue = boolValue,
            StringValue = stringValue?.Trim()
        };
    }

    /// <summary>
    /// Получает значение атрибута как object (для сериализации).
    /// Возвращает null, если значение не установлено.
    /// </summary>
    public object? GetValue()
    {
        return Definition?.DataType switch
        {
            AttributeDataType.Integer => IntValue,
            AttributeDataType.Decimal => DecimalValue,
            AttributeDataType.Boolean => BoolValue,
            AttributeDataType.String => StringValue,
            _ => null
        };
    }

    public void Update(int? intValue = null, decimal? decimalValue = null, bool? boolValue = null, string? stringValue = null)
    {
        IntValue = intValue ?? IntValue;
        DecimalValue = decimalValue ?? DecimalValue;
        BoolValue = boolValue ?? BoolValue;
        StringValue = stringValue?.Trim() ?? StringValue;
    }
}
