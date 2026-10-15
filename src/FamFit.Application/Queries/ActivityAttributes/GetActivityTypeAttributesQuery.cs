using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Queries.ActivityAttributes;

/// <summary>
/// Запрос получения значений атрибутов для ActivityType.
/// </summary>
public record GetActivityTypeAttributesQuery(Guid ActivityTypeId) : IRequest<IReadOnlyList<ActivityAttributeValue>>;

/// <summary>
/// Handler запроса GetActivityTypeAttributesQuery.
/// </summary>
public class GetActivityTypeAttributesHandler : IRequestHandler<GetActivityTypeAttributesQuery, IReadOnlyList<ActivityAttributeValue>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetActivityTypeAttributesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<ActivityAttributeValue>> Handle(
        GetActivityTypeAttributesQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ActivityTypeAttributeValues
            .Where(atav => atav.ActivityTypeId == request.ActivityTypeId)
            .Select(atav => atav.AttributeValue!)
            .Include(av => av.Definition)
            .OrderBy(av => av.Definition!.DisplayOrder)
            .ThenBy(av => av.Definition!.Name)
            .ToListAsync(cancellationToken);
    }
}
