namespace FamFit.Domain.Entities;

/// <summary>
/// Тренировка — конкретная сессия пользователя, содержащая несколько записей активностей.
/// </summary>
public sealed class Workout
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid UserId { get; private set; }
    public string? Name { get; private set; }          // опциональное имя (из шаблона или пользовательское)
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public bool IsCompleted { get; private set; } = false;

    // Navigation properties
    public User? User { get; private set; }
    public ICollection<WorkoutEntry> Entries { get; private set; } = new List<WorkoutEntry>();

    // EF Core parameterless constructor
    private Workout() { }

    public static Workout Create(Guid userId, string? name = null)
    {
        if (name != null && name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));

        return new Workout
        {
            UserId = userId,
            Name = name?.Trim(),
            StartTime = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        EndTime = DateTime.UtcNow;
        IsCompleted = true;
    }

    public void UpdateName(string name)
    {
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));

        Name = name.Trim();
    }
}
