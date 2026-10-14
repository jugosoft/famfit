using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.WorkoutTemplates;

/// <summary>
/// Команда создания нового шаблона тренировки.
/// </summary>
public record CreateWorkoutTemplateCommand(
    Guid UserId,
    string Name,
    string? Description) : IRequest<WorkoutTemplate>;

/// <summary>
/// Handler команды CreateWorkoutTemplateCommand.
/// </summary>
public class CreateWorkoutTemplateHandler : IRequestHandler<CreateWorkoutTemplateCommand, WorkoutTemplate>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateWorkoutTemplateHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<WorkoutTemplate> Handle(
        CreateWorkoutTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = WorkoutTemplate.Create(request.UserId, request.Name, request.Description);
        _dbContext.WorkoutTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return template;
    }
}
