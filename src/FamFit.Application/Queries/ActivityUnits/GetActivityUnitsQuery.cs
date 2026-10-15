using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Queries.ActivityUnits;

/// <summary>
/// Запрос получения списка всех единиц измерения активности.
/// </summary>
public record GetActivityUnitsQuery : IRequest<IReadOnlyList<ActivityUnitDto>>;

public record ActivityUnitDto(
    Guid Id,
    string Name,
    string Symbol,
    string DataType,
    bool IsActive,
    int DisplayOrder);

/// <summary>
/// Handler запроса GetActivityUnitsQuery.
/// </summary>
public class GetActivityUnitsHandler : IRequestHandler<GetActivityUnitsQuery, IReadOnlyList<ActivityUnitDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetActivityUnitsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<ActivityUnitDto>> Handle(
        GetActivityUnitsQuery request,
        CancellationToken cancellationToken)
    {
        var units = await _dbContext.ActivityUnits
            .OrderBy(u => u.DisplayOrder)
            .ThenBy(u => u.Name)
            .Select(u => new ActivityUnitDto(
                u.Id,
                u.Name,
                u.Symbol,
                u.DataType,
                u.IsActive,
                u.DisplayOrder))
            .ToListAsync(cancellationToken);

        return units.AsReadOnly();
    }
}
