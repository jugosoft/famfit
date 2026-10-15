namespace FamFit.Domain.Entities;

/// <summary>
/// Связь WorkoutEntry с конкретными значениями атрибутов.
/// Заменяет жёсткие поля Sets/Reps/Weight.
/// </summary>
public sealed class WorkoutEntryAttributeValue
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid WorkoutEntryId { get; private set; }
    public Guid AttributeValueId { get; private set; }

    // Navigation properties
    public WorkoutEntry? WorkoutEntry { get; private set; }
    public ActivityAttributeValue? AttributeValue { get; private set; }

    // EF Core parameterless constructor
    private WorkoutEntryAttributeValue() { }

    public static WorkoutEntryAttributeValue Create(Guid workoutEntryId, Guid attributeValueId)
    {
        return new WorkoutEntryAttributeValue
        {
            WorkoutEntryId = workoutEntryId,
            AttributeValueId = attributeValueId
        };
    }
}
