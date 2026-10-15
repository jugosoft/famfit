using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.ActivityUnits;

/// <summary>
/// Команда создания единицы измерения активности.
/// </summary>
public record CreateActivityUnitCommand(
    string Name,
    string Symbol,
    string DataType,
    int DisplayOrder) : IRequest<ActivityUnitResult>;

public record ActivityUnitResult(
    Guid Id,
    string Name,
    string Symbol,
    string DataType,
    bool IsActive,
    int DisplayOrder);

/// <summary>
/// Handler команды CreateActivityUnitCommand.
/// </summary>
public class CreateActivityUnitHandler : IRequestHandler<CreateActivityUnitCommand, ActivityUnitResult>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateActivityUnitHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ActivityUnitResult> Handle(
        CreateActivityUnitCommand request,
        CancellationToken cancellationToken)
    {
        // Проверяем уникальность имени
        var exists = await _dbContext.ActivityUnits
            .AnyAsync(u => u.Name == request.Name, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Activity unit with name '{request.Name}' already exists.");

        var unit = ActivityUnit.Create(
            request.Name,
            request.Symbol,
            request.DataType,
            request.DisplayOrder);

        _dbContext.ActivityUnits.Add(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ActivityUnitResult(
            unit.Id,
            unit.Name,
            unit.Symbol,
            unit.DataType,
            unit.IsActive,
            unit.DisplayOrder);
    }
}
