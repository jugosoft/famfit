using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Queries.WorkoutTemplates;

/// <summary>
/// Запрос получения шаблона тренировки по ID с элементами.
/// </summary>
public record GetWorkoutTemplateQuery(Guid TemplateId) : IRequest<WorkoutTemplate>;

/// <summary>
/// Handler запроса GetWorkoutTemplateQuery.
/// </summary>
public class GetWorkoutTemplateHandler : IRequestHandler<GetWorkoutTemplateQuery, WorkoutTemplate>
{
    private readonly IApplicationDbContext _dbContext;

    public GetWorkoutTemplateHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<WorkoutTemplate> Handle(
        GetWorkoutTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.WorkoutTemplates
            .Include(t => t.Items)
                .ThenInclude(i => i.ActivityType)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template is null)
            throw new KeyNotFoundException($"WorkoutTemplate with id {request.TemplateId} not found.");

        return template;
    }
}
