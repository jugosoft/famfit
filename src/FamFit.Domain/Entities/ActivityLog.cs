namespace FamFit.Domain.Entities;

/// <summary>
/// Запись об активности пользователя.
/// </summary>
public sealed class ActivityLog
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid UserId { get; private set; }
    public Guid ActivityTypeId { get; private set; }
    public decimal Value { get; private set; }
    public DateTime Timestamp { get; private set; }

    // Navigation properties (не сериализуются в API автоматически, но используются EF)
    public User? User { get; private set; }
    public ActivityType? ActivityType { get; private set; }

    // EF Core parameterless constructor
    private ActivityLog() { }

    public static ActivityLog Create(Guid userId, Guid activityTypeId, decimal value, DateTime? timestamp = null)
    {
        if (value <= 0)
            throw new ArgumentException("Value must be greater than zero.", nameof(value));

        return new ActivityLog
        {
            UserId = userId,
            ActivityTypeId = activityTypeId,
            Value = value,
            Timestamp = timestamp ?? DateTime.UtcNow
        };
    }
}
