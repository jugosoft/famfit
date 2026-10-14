namespace FamFit.Domain.Entities;

/// <summary>
/// Запись внутри тренировки — конкретная активность с фактическими значениями.
/// </summary>
public sealed class WorkoutEntry
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid WorkoutId { get; private set; }
    public Guid ActivityTypeId { get; private set; }
    public int? Sets { get; private set; }       // фактические подходы
    public int? Reps { get; private set; }        // фактические повторения
    public int? Weight { get; private set; }      // фактический вес

    // Navigation properties
    public Workout? Workout { get; private set; }
    public ActivityType? ActivityType { get; private set; }

    // EF Core parameterless constructor
    private WorkoutEntry() { }

    public static WorkoutEntry Create(Guid workoutId, Guid activityTypeId, int? sets = null, int? reps = null, int? weight = null)
    {
        if (sets.HasValue && sets.Value < 0)
            throw new ArgumentException("Sets cannot be negative.", nameof(sets));
        if (reps.HasValue && reps.Value < 0)
            throw new ArgumentException("Reps cannot be negative.", nameof(reps));
        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentException("Weight cannot be negative.", nameof(weight));

        return new WorkoutEntry
        {
            WorkoutId = workoutId,
            ActivityTypeId = activityTypeId,
            Sets = sets,
            Reps = reps,
            Weight = weight
        };
    }

    public void Update(int? sets = null, int? reps = null, int? weight = null)
    {
        if (sets.HasValue && sets.Value < 0)
            throw new ArgumentException("Sets cannot be negative.", nameof(sets));
        if (reps.HasValue && reps.Value < 0)
            throw new ArgumentException("Reps cannot be negative.", nameof(reps));
        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentException("Weight cannot be negative.", nameof(weight));

        Sets = sets ?? Sets;
        Reps = reps ?? Reps;
        Weight = weight ?? Weight;
    }
}
