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
            .Include(w => w.Entries)
                .ThenInclude(e => e.WorkoutEntryAttributeValues)
                    .ThenInclude(awi => awi.AttributeValue)
                        .ThenInclude(av => av!.Definition)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout is null)
            throw new KeyNotFoundException($"Workout with id {request.WorkoutId} not found.");

        if (workout.IsCompleted)
            throw new InvalidOperationException("Workout is already completed.");

        // Для каждой записи создаём ActivityLog
        // Value рассчитывается на основе атрибутов записи
        foreach (var entry in workout.Entries)
        {
            // Извлекаем значения атрибутов по именам определений
            int? sets = null;
            int? reps = null;
            decimal? weight = null;
            decimal? distance = null;
            decimal? time = null;

            foreach (var attr in entry.WorkoutEntryAttributeValues)
            {
                var attrValue = attr.AttributeValue;
                if (attrValue == null) continue;

                var def = attrValue.Definition;
                if (def == null) continue;

                var val = attrValue.GetValue();
                switch (def.Name)
                {
                    case "Sets":
                        if (val is int i) sets = i;
                        break;
                    case "Reps":
                        if (val is int r) reps = r;
                        break;
                    case "Weight":
                        if (val is decimal w) weight = w;
                        break;
                    case "Distance":
                        if (val is decimal d) distance = d;
                        break;
                    case "Time":
                        if (val is decimal t) time = t;
                        break;
                }
            }

            decimal value;

            if (weight.HasValue && sets.HasValue && reps.HasValue)
            {
                // Общий подъятый вес = подходы * повторения * вес
                value = sets.Value * reps.Value * weight.Value;
            }
            else if (sets.HasValue && reps.HasValue)
            {
                // Общее количество = подходы * повторения
                value = sets.Value * reps.Value;
            }
            else if (distance.HasValue)
            {
                // Дистанция
                value = distance.Value;
            }
            else if (time.HasValue)
            {
                // Время
                value = time.Value;
            }
            else if (reps.HasValue)
            {
                // Просто повторения
                value = reps.Value;
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
