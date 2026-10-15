namespace FamFit.Domain.Entities;

/// <summary>
/// Связь WorkoutTemplateItem с конкретными значениями атрибутов.
/// Заменяет жёсткие поля Sets/Reps/Weight.
/// </summary>
public sealed class WorkoutTemplateItemAttributeValue
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid WorkoutTemplateItemId { get; private set; }
    public Guid AttributeValueId { get; private set; }

    // Navigation properties
    public WorkoutTemplateItem? WorkoutTemplateItem { get; private set; }
    public ActivityAttributeValue? AttributeValue { get; private set; }

    // EF Core parameterless constructor
    private WorkoutTemplateItemAttributeValue() { }

    public static WorkoutTemplateItemAttributeValue Create(Guid workoutTemplateItemId, Guid attributeValueId)
    {
        return new WorkoutTemplateItemAttributeValue
        {
            WorkoutTemplateItemId = workoutTemplateItemId,
            AttributeValueId = attributeValueId
        };
    }
}
