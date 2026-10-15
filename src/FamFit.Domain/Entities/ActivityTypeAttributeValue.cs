namespace FamFit.Domain.Entities;

/// <summary>
/// Связь ActivityType с конкретными значениями атрибутов.
/// Позволяет привязывать любые комбинации атрибутов к типу активности.
/// </summary>
public sealed class ActivityTypeAttributeValue
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid ActivityTypeId { get; private set; }
    public Guid AttributeValueId { get; private set; }

    // Navigation properties
    public ActivityType? ActivityType { get; private set; }
    public ActivityAttributeValue? AttributeValue { get; private set; }

    // EF Core parameterless constructor
    private ActivityTypeAttributeValue() { }

    public static ActivityTypeAttributeValue Create(Guid activityTypeId, Guid attributeValueId)
    {
        return new ActivityTypeAttributeValue
        {
            ActivityTypeId = activityTypeId,
            AttributeValueId = attributeValueId
        };
    }
}
