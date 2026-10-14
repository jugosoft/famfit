using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.WorkoutTemplates;

/// <summary>
/// Команда добавления элемента в шаблон тренировки.
/// </summary>
public record AddWorkoutTemplateItemCommand(
    Guid TemplateId,
    Guid ActivityTypeId,
    int? Sets,
    int? Reps,
    int? Weight) : IRequest<WorkoutTemplateItem>;

/// <summary>
/// Handler команды AddWorkoutTemplateItemCommand.
/// </summary>
public class AddWorkoutTemplateItemHandler : IRequestHandler<AddWorkoutTemplateItemCommand, WorkoutTemplateItem>
{
    private readonly IApplicationDbContext _dbContext;

    public AddWorkoutTemplateItemHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<WorkoutTemplateItem> Handle(
        AddWorkoutTemplateItemCommand request,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.WorkoutTemplates
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template is null)
            throw new KeyNotFoundException($"WorkoutTemplate with id {request.TemplateId} not found.");

        // Вычисляем порядок: последний элемент + 1
        var maxOrder = template.Items.Any()
            ? template.Items.Max(i => i.DisplayOrder)
            : 0;

        var item = WorkoutTemplateItem.Create(
            template.Id,
            request.ActivityTypeId,
            request.Sets,
            request.Reps,
            request.Weight,
            maxOrder + 1);

        template.Items.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }
}
