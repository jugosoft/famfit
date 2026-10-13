using Microsoft.EntityFrameworkCore;

using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;

namespace FamFit.Application.Commands.PushSubscriptions;

/// <summary>
/// Команда регистрации / обновления push-подписки.
/// </summary>
public record RegisterPushSubscriptionCommand(
    Guid UserId,
    string Endpoint,
    string P256dh,
    string Auth) : IRequest<PushSubscription>;

/// <summary>
/// Handler команды RegisterPushSubscriptionCommand.
/// Логика: если подписка с таким endpoint уже есть — обновляем, иначе создаём.
/// </summary>
public class RegisterPushSubscriptionHandler
    : IRequestHandler<RegisterPushSubscriptionCommand, PushSubscription>
{
    private readonly IApplicationDbContext _dbContext;

    public RegisterPushSubscriptionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PushSubscription> Handle(
        RegisterPushSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        // Поиск существующей подписки по endpoint
        var existing = await _dbContext.PushSubscriptions
            .FirstOrDefaultAsync(
                ps => ps.UserId == request.UserId && ps.Endpoint == request.Endpoint,
                cancellationToken);

        if (existing is not null)
        {
            existing.Update(request.Endpoint, request.P256dh, request.Auth);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var subscription = PushSubscription.Create(
            request.UserId,
            request.Endpoint,
            request.P256dh,
            request.Auth);

        _dbContext.PushSubscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return subscription;
    }
}
