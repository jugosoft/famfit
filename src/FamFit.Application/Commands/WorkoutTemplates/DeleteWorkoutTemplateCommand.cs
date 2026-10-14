using FamFit.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.WorkoutTemplates;

/// <summary>
/// Команда удаления шаблона тренировки.
/// </summary>
public record DeleteWorkoutTemplateCommand(
    Guid TemplateId) : IRequest;

/// <summary>
/// Handler команды DeleteWorkoutTemplateCommand.
/// </summary>
public class DeleteWorkoutTemplateHandler : IRequestHandler<DeleteWorkoutTemplateCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteWorkoutTemplateHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        DeleteWorkoutTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.WorkoutTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template is null)
            throw new KeyNotFoundException($"WorkoutTemplate with id {request.TemplateId} not found.");

        _dbContext.WorkoutTemplates.Remove(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
