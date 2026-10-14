using FamFit.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.WorkoutTemplates;

/// <summary>
/// Команда удаления элемента из шаблона тренировки.
/// </summary>
public record RemoveWorkoutTemplateItemCommand(
    Guid TemplateId,
    Guid ActivityTypeId) : IRequest;

/// <summary>
/// Handler команды RemoveWorkoutTemplateItemCommand.
/// </summary>
public class RemoveWorkoutTemplateItemHandler : IRequestHandler<RemoveWorkoutTemplateItemCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveWorkoutTemplateItemHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        RemoveWorkoutTemplateItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _dbContext.WorkoutTemplateItems
            .FirstOrDefaultAsync(
                i => i.TemplateId == request.TemplateId && i.ActivityTypeId == request.ActivityTypeId,
                cancellationToken);

        if (item is not null)
        {
            _dbContext.WorkoutTemplateItems.Remove(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
