using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.Workouts;

/// <summary>
/// Команда создания новой тренировки.
/// </summary>
public record CreateWorkoutCommand(
    Guid UserId,
    string? Name) : IRequest<Workout>;

/// <summary>
/// Handler команды CreateWorkoutCommand.
/// </summary>
public class CreateWorkoutHandler : IRequestHandler<CreateWorkoutCommand, Workout>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateWorkoutHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Workout> Handle(
        CreateWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var workout = Workout.Create(request.UserId, request.Name);
        _dbContext.Workouts.Add(workout);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return workout;
    }
}
