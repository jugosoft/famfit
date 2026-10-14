using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Queries.WorkoutTemplates;

/// <summary>
/// Запрос получения списка шаблонов тренировки пользователя.
/// </summary>
public record GetUserWorkoutTemplatesQuery(Guid UserId) : IRequest<IEnumerable<WorkoutTemplate>>;

/// <summary>
/// Handler запроса GetUserWorkoutTemplatesQuery.
/// </summary>
public class GetUserWorkoutTemplatesHandler : IRequestHandler<GetUserWorkoutTemplatesQuery, IEnumerable<WorkoutTemplate>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserWorkoutTemplatesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IEnumerable<WorkoutTemplate>> Handle(
        GetUserWorkoutTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.WorkoutTemplates
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.Name)
            .ToListAsync(cancellationToken);
    }
}
