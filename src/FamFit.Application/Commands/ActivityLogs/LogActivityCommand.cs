using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;

namespace FamFit.Application.Commands.ActivityLogs;

/// <summary>
/// Команда логирования факта активности.
/// </summary>
public record LogActivityCommand(
    Guid UserId,
    Guid ActivityTypeId,
    decimal Value,
    DateTime? Timestamp = null) : IRequest<ActivityLog>;

/// <summary>
/// Handler команды LogActivityCommand.
/// Сохраняет факт тренировки, проверяет существование ActivityType.
/// </summary>
public class LogActivityHandler : IRequestHandler<LogActivityCommand, ActivityLog>
{
    private readonly IApplicationDbContext _dbContext;

    public LogActivityHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ActivityLog> Handle(
        LogActivityCommand request,
        CancellationToken cancellationToken)
    {
        // Проверка: ActivityType существует и активен
        var activityType = await _dbContext.ActivityTypes
            .FirstOrDefaultAsync(at => at.Id == request.ActivityTypeId, cancellationToken);

        if (activityType is null)
            throw new InvalidOperationException(
                $"Activity type with ID '{request.ActivityTypeId}' not found.");

        if (!activityType.IsActive)
            throw new InvalidOperationException(
                "Cannot log activity for a deactivated activity type.");

        // Проверка: пользователь существует
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new InvalidOperationException(
                $"User with ID '{request.UserId}' not found.");

        // Создание записи через Domain-модель
        var activityLog = ActivityLog.Create(
            request.UserId,
            request.ActivityTypeId,
            request.Value,
            request.Timestamp);

        _dbContext.ActivityLogs.Add(activityLog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return activityLog;
    }
}
