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
    DbSet<ActivityAttributeDefinition> ActivityAttributeDefinitions { get; }
    DbSet<ActivityAttributeValue> AttributeValues { get; }
    DbSet<ActivityTypeAttributeValue> ActivityTypeAttributeValues { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<Workout> Workouts { get; }
    DbSet<WorkoutEntry> WorkoutEntries { get; }
    DbSet<WorkoutEntryAttributeValue> WorkoutEntryAttributeValues { get; }
    DbSet<WorkoutTemplate> WorkoutTemplates { get; }
    DbSet<WorkoutTemplateItem> WorkoutTemplateItems { get; }
    DbSet<WorkoutTemplateItemAttributeValue> WorkoutTemplateItemAttributeValues { get; }
    DbSet<PushSubscription> PushSubscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
