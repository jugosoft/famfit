using Microsoft.EntityFrameworkCore;

using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using FamFit.Domain.Enums;
using MediatR;

namespace FamFit.Application.Commands.ActivityTypes;

/// <summary>
/// Команда создания нового типа активности (только для FamilyAdmin).
/// </summary>
public record CreateActivityTypeCommand(
    string Name,
    UnitType UnitType) : IRequest<ActivityType>;

/// <summary>
/// Handler команды CreateActivityTypeCommand.
/// Содержит бизнес-логику: проверка роли пользователя, создание сущности.
/// </summary>
public class CreateActivityTypeHandler : IRequestHandler<CreateActivityTypeCommand, ActivityType>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateActivityTypeHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ActivityType> Handle(
        CreateActivityTypeCommand request,
        CancellationToken cancellationToken)
    {
        // --- Проверка: авторизация через HttpContext вне этого слоя ---
        // В реальном проекте роль пользователя передаётся через контекст (например, 
        // ICurrentUserService) и проверяется здесь.
        //
        // Пример:
        //   if (!_currentUserService.IsAdmin())
        //       throw new UnauthorizedAccessException("Only FamilyAdmin can create activity types.");

        // Проверка: тип с таким именем уже существует?
        var exists = await _dbContext.ActivityTypes
            .AnyAsync(at => at.Name == request.Name, cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                $"Activity type with name '{request.Name}' already exists.");

        // Создание сущности через фабричный метод Domain-модели
        var activityType = ActivityType.Create(request.Name, request.UnitType);

        _dbContext.ActivityTypes.Add(activityType);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return activityType;
    }
}
