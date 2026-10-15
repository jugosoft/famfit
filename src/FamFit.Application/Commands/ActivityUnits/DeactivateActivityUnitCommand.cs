using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.ActivityUnits;

/// <summary>
/// Команда деактивации единицы измерения активности.
/// </summary>
public record DeactivateActivityUnitCommand(Guid Id) : IRequest;

/// <summary>
/// Handler команды DeactivateActivityUnitCommand.
/// </summary>
public class DeactivateActivityUnitHandler : IRequestHandler<DeactivateActivityUnitCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateActivityUnitHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        DeactivateActivityUnitCommand request,
        CancellationToken cancellationToken)
    {
        var unit = await _dbContext.ActivityUnits
            .Include(u => u.ActivityTypes)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (unit is null)
            throw new InvalidOperationException($"Activity unit with Id {request.Id} not found.");

        // Проверяем, используется ли единица измерения
        if (unit.ActivityTypes.Any(at => at.IsActive))
        {
            throw new InvalidOperationException(
                $"Невозможно деактивировать единицу измерения '{unit.Name}', так как она используется в активных типах активностей.");
        }

        unit.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
