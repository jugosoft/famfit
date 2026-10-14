using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Queries.Workouts;

/// <summary>
/// Запрос получения списка тренировок пользователя.
/// </summary>
public record GetUserWorkoutsQuery(Guid UserId) : IRequest<IEnumerable<Workout>>;

/// <summary>
/// Handler запроса GetUserWorkoutsQuery.
/// </summary>
public class GetUserWorkoutsHandler : IRequestHandler<GetUserWorkoutsQuery, IEnumerable<Workout>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserWorkoutsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IEnumerable<Workout>> Handle(
        GetUserWorkoutsQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Workouts
            .Where(w => w.UserId == request.UserId)
            .OrderByDescending(w => w.StartTime)
            .ToListAsync(cancellationToken);
    }
}
