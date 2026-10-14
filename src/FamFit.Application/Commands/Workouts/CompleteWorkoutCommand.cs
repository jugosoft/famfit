using FamFit.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.Workouts;

/// <summary>
/// Команда завершения тренировки (с авто-созданием ActivityLog).
/// </summary>
public record CompleteWorkoutCommand(
    Guid WorkoutId) : IRequest;

/// <summary>
/// Handler команды CompleteWorkoutCommand.
/// Создаёт ActivityLog для каждой WorkoutEntry и завершает тренировку.
/// </summary>
public class CompleteWorkoutHandler : IRequestHandler<CompleteWorkoutCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public CompleteWorkoutHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        CompleteWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var workout = await _dbContext.Workouts
            .Include(w => w.Entries)
            .ThenInclude(e => e.ActivityType)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout is null)
            throw new KeyNotFoundException($"Workout with id {request.WorkoutId} not found.");

        if (workout.IsCompleted)
            throw new InvalidOperationException("Workout is already completed.");

        // Для каждой записи создаём ActivityLog
        // Value рассчитывается как: sets * reps * weight (если weight есть)
        // или sets * reps (если weight нет)
        // или просто reps (если нет sets)
        foreach (var entry in workout.Entries)
        {
            decimal value;

            if (entry.Weight.HasValue && entry.Sets.HasValue && entry.Reps.HasValue)
            {
                // Общий подъятый вес = подходы * повторения * вес
                value = entry.Sets.Value * entry.Reps.Value * entry.Weight.Value;
            }
            else if (entry.Sets.HasValue && entry.Reps.HasValue)
            {
                // Общее количество = подходы * повторения
                value = entry.Sets.Value * entry.Reps.Value;
            }
            else if (entry.Reps.HasValue)
            {
                // Просто повторения
                value = entry.Reps.Value;
            }
            else
            {
                // Если нет данных — пропускаем
                continue;
            }

            var activityLog = FamFit.Domain.Entities.ActivityLog.Create(
                workout.UserId,
                entry.ActivityTypeId,
                value);

            _dbContext.ActivityLogs.Add(activityLog);
        }

        workout.Complete();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
