namespace FamFit.Domain.Entities;

/// <summary>
/// Элемент шаблона тренировки — ссылка на тип активности с дефолтными значениями атрибутов.
/// Атрибуты хранятся через гибкую систему WorkoutTemplateItemAttributeValue.
/// </summary>
public sealed class WorkoutTemplateItem
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid TemplateId { get; private set; }
    public Guid ActivityTypeId { get; private set; }
    public int DisplayOrder { get; private set; } // порядок отображения

    // Navigation properties
    public WorkoutTemplate? Template { get; private set; }
    public ActivityType? ActivityType { get; private set; }
    public ICollection<WorkoutTemplateItemAttributeValue> WorkoutTemplateItemAttributeValues { get; private set; } = new List<WorkoutTemplateItemAttributeValue>();

    // EF Core parameterless constructor
    private WorkoutTemplateItem() { }

    public static WorkoutTemplateItem Create(
        Guid templateId,
        Guid activityTypeId,
        int displayOrder = 0)
    {
        if (displayOrder < 0)
            throw new ArgumentException("DisplayOrder cannot be negative.", nameof(displayOrder));

        return new WorkoutTemplateItem
        {
            TemplateId = templateId,
            ActivityTypeId = activityTypeId,
            DisplayOrder = displayOrder
        };
    }
}
