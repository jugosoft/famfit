using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.ActivityUnits;

/// <summary>
/// Команда обновления единицы измерения активности.
/// </summary>
public record UpdateActivityUnitCommand(
    Guid Id,
    string? Name,
    string? Symbol,
    string? DataType,
    bool? IsActive,
    int? DisplayOrder) : IRequest;

/// <summary>
/// Handler команды UpdateActivityUnitCommand.
/// </summary>
public class UpdateActivityUnitHandler : IRequestHandler<UpdateActivityUnitCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateActivityUnitHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        UpdateActivityUnitCommand request,
        CancellationToken cancellationToken)
    {
        var unit = await _dbContext.ActivityUnits
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (unit is null)
            throw new InvalidOperationException($"Activity unit with Id {request.Id} not found.");

        unit.Update(request.Name, request.Symbol, request.DataType, request.IsActive, request.DisplayOrder);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
