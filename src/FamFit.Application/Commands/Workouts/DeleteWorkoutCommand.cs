using FamFit.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.Workouts;

/// <summary>
/// Команда удаления тренировки.
/// </summary>
public record DeleteWorkoutCommand(
    Guid WorkoutId) : IRequest;

/// <summary>
/// Handler команды DeleteWorkoutCommand.
/// </summary>
public class DeleteWorkoutHandler : IRequestHandler<DeleteWorkoutCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteWorkoutHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        DeleteWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var workout = await _dbContext.Workouts
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout is null)
            throw new KeyNotFoundException($"Workout with id {request.WorkoutId} not found.");

        _dbContext.Workouts.Remove(workout);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
