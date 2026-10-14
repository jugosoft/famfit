using FamFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Interfaces;

/// <summary>
/// Интерфейс контекста приложения.
/// Позволяет Infrastructure-слою реализовать его через EF Core,
/// а Application-слой оставаться независимым от технологии хранения.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<ActivityType> ActivityTypes { get; }
    DbSet<ActivityTypeAttributes> ActivityTypeAttributes { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<Workout> Workouts { get; }
    DbSet<WorkoutEntry> WorkoutEntries { get; }
    DbSet<WorkoutTemplate> WorkoutTemplates { get; }
    DbSet<WorkoutTemplateItem> WorkoutTemplateItems { get; }
    DbSet<PushSubscription> PushSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
