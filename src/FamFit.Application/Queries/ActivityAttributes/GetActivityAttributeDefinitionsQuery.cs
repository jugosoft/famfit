using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Queries.ActivityAttributes;

/// <summary>
/// Запрос получения всех активных определений атрибутов.
/// </summary>
public record GetActivityAttributeDefinitionsQuery : IRequest<IReadOnlyList<ActivityAttributeDefinition>>;

/// <summary>
/// Handler запроса GetActivityAttributeDefinitionsQuery.
/// </summary>
public class GetActivityAttributeDefinitionsHandler : IRequestHandler<GetActivityAttributeDefinitionsQuery, IReadOnlyList<ActivityAttributeDefinition>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetActivityAttributeDefinitionsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<ActivityAttributeDefinition>> Handle(
        GetActivityAttributeDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ActivityAttributeDefinitions
            .Where(d => d.IsActive)
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }
}
