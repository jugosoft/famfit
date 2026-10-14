using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.Workouts;

/// <summary>
/// Команда добавления записи в тренировку.
/// </summary>
public record AddWorkoutEntryCommand(
    Guid WorkoutId,
    Guid ActivityTypeId,
    int? Sets,
    int? Reps,
    int? Weight) : IRequest<WorkoutEntry>;

/// <summary>
/// Handler команды AddWorkoutEntryCommand.
/// </summary>
public class AddWorkoutEntryHandler : IRequestHandler<AddWorkoutEntryCommand, WorkoutEntry>
{
    private readonly IApplicationDbContext _dbContext;

    public AddWorkoutEntryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<WorkoutEntry> Handle(
        AddWorkoutEntryCommand request,
        CancellationToken cancellationToken)
    {
        var workout = await _dbContext.Workouts
            .Include(w => w.Entries)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout is null)
            throw new KeyNotFoundException($"Workout with id {request.WorkoutId} not found.");

        if (workout.IsCompleted)
            throw new InvalidOperationException("Cannot add entries to a completed workout.");

        var entry = WorkoutEntry.Create(
            workout.Id,
            request.ActivityTypeId,
            request.Sets,
            request.Reps,
            request.Weight);

        workout.Entries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry;
    }
}
