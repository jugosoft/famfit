namespace FamFit.Domain.Entities;

/// <summary>
/// Запись внутри тренировки — конкретная активность с фактическими значениями атрибутов.
/// Атрибуты хранятся через гибкую систему WorkoutEntryAttributeValue.
/// </summary>
public sealed class WorkoutEntry
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid WorkoutId { get; private set; }
    public Guid ActivityTypeId { get; private set; }

    // Navigation properties
    public Workout? Workout { get; private set; }
    public ActivityType? ActivityType { get; private set; }
    public ICollection<WorkoutEntryAttributeValue> WorkoutEntryAttributeValues { get; private set; } = new List<WorkoutEntryAttributeValue>();

    // EF Core parameterless constructor
    private WorkoutEntry() { }

    public static WorkoutEntry Create(Guid workoutId, Guid activityTypeId)
    {
        return new WorkoutEntry
        {
            WorkoutId = workoutId,
            ActivityTypeId = activityTypeId
        };
    }
}
