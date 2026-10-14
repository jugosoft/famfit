namespace FamFit.Domain.Entities;

/// <summary>
/// Атрибуты типа активности (подходы, повторения, вес).
/// 1:1 связь с ActivityType — создаётся автоматически при создании типа.
/// </summary>
public sealed class ActivityTypeAttributes
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid ActivityTypeId { get; private set; }
    public int? Sets { get; private set; }       // количество подходов (null = не применяется)
    public int? Reps { get; private set; }        // повторений в подходе
    public int? Weight { get; private set; }      // рабочий вес в кг

    // Navigation property
    public ActivityType? ActivityType { get; private set; }

    // EF Core parameterless constructor
    private ActivityTypeAttributes() { }

    public static ActivityTypeAttributes Create(Guid activityTypeId, int? sets = null, int? reps = null, int? weight = null)
    {
        if (sets.HasValue && sets.Value < 0)
            throw new ArgumentException("Sets cannot be negative.", nameof(sets));
        if (reps.HasValue && reps.Value < 0)
            throw new ArgumentException("Reps cannot be negative.", nameof(reps));
        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentException("Weight cannot be negative.", nameof(weight));

        return new ActivityTypeAttributes
        {
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
