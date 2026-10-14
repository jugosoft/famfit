using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Queries.Workouts;

/// <summary>
/// Запрос получения тренировки по ID с записями.
/// </summary>
public record GetWorkoutQuery(Guid WorkoutId) : IRequest<Workout>;

/// <summary>
/// Handler запроса GetWorkoutQuery.
/// </summary>
public class GetWorkoutHandler : IRequestHandler<GetWorkoutQuery, Workout>
{
    private readonly IApplicationDbContext _dbContext;

    public GetWorkoutHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Workout> Handle(
        GetWorkoutQuery request,
        CancellationToken cancellationToken)
    {
        var workout = await _dbContext.Workouts
            .Include(w => w.Entries)
                .ThenInclude(e => e.ActivityType)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout is null)
            throw new KeyNotFoundException($"Workout with id {request.WorkoutId} not found.");

        return workout;
    }
}
