namespace FamFit.Domain.Entities;

/// <summary>
/// Элемент шаблона тренировки — ссылка на тип активности с дефолтными значениями.
/// </summary>
public sealed class WorkoutTemplateItem
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid TemplateId { get; private set; }
    public Guid ActivityTypeId { get; private set; }
    public int? Sets { get; private set; }       // дефолтные подходы
    public int? Reps { get; private set; }        // дефолтные повторения
    public int? Weight { get; private set; }      // дефолтный вес
    public int DisplayOrder { get; private set; } // порядок отображения

    // Navigation properties
    public WorkoutTemplate? Template { get; private set; }
    public ActivityType? ActivityType { get; private set; }

    // EF Core parameterless constructor
    private WorkoutTemplateItem() { }

    public static WorkoutTemplateItem Create(
        Guid templateId,
        Guid activityTypeId,
        int? sets = null,
        int? reps = null,
        int? weight = null,
        int displayOrder = 0)
    {
        if (displayOrder < 0)
            throw new ArgumentException("DisplayOrder cannot be negative.", nameof(displayOrder));

        return new WorkoutTemplateItem
        {
            TemplateId = templateId,
            ActivityTypeId = activityTypeId,
            Sets = sets,
            Reps = reps,
            Weight = weight,
            DisplayOrder = displayOrder
        };
    }

    public void Update(int? sets = null, int? reps = null, int? weight = null, int? displayOrder = null)
    {
        if (sets.HasValue && sets.Value < 0)
            throw new ArgumentException("Sets cannot be negative.", nameof(sets));
        if (reps.HasValue && reps.Value < 0)
            throw new ArgumentException("Reps cannot be negative.", nameof(reps));
        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentException("Weight cannot be negative.", nameof(weight));
        if (displayOrder.HasValue && displayOrder.Value < 0)
            throw new ArgumentException("DisplayOrder cannot be negative.", nameof(displayOrder));

        Sets = sets ?? Sets;
        Reps = reps ?? Reps;
        Weight = weight ?? Weight;
        DisplayOrder = displayOrder ?? DisplayOrder;
    }
}
