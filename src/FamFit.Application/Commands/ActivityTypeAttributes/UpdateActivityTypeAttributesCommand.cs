using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.ActivityTypeAttributes;

/// <summary>
/// Команда обновления атрибутов типа активности.
/// </summary>
public record UpdateActivityTypeAttributesCommand(
    Guid ActivityTypeId,
    int? Sets,
    int? Reps,
    int? Weight) : IRequest;

/// <summary>
/// Handler команды UpdateActivityTypeAttributesCommand.
/// </summary>
public class UpdateActivityTypeAttributesHandler : IRequestHandler<UpdateActivityTypeAttributesCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateActivityTypeAttributesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        UpdateActivityTypeAttributesCommand request,
        CancellationToken cancellationToken)
    {
        var attributes = await _dbContext.ActivityTypeAttributes
            .FirstOrDefaultAsync(a => a.ActivityTypeId == request.ActivityTypeId, cancellationToken);

        if (attributes is null)
        {
            // Создаём атрибуты, если их ещё нет
            attributes = FamFit.Domain.Entities.ActivityTypeAttributes.Create(
                request.ActivityTypeId,
                request.Sets,
                request.Reps,
                request.Weight);

            _dbContext.ActivityTypeAttributes.Add(attributes);
        }
        else
        {
            attributes.Update(request.Sets, request.Reps, request.Weight);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
