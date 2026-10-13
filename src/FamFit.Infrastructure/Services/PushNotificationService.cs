using FamFit.Application.Interfaces;
using FamFit.Infrastructure.Configuration;
using FamFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebPush;

namespace FamFit.Infrastructure.Services;

/// <summary>
/// Реализация IPushNotificationService с использованием WebPush библиотеки (VAPID).
/// </summary>
public sealed class PushNotificationService : IPushNotificationService, IDisposable
{
    private readonly WebPushOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;

    public PushNotificationService(
        IOptions<WebPushOptions> options,
        IServiceProvider serviceProvider)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        if (string.IsNullOrWhiteSpace(_options.PublicKey))
            throw new InvalidOperationException("VAPID public key is not configured.");
        if (string.IsNullOrWhiteSpace(_options.PrivateKey))
            throw new InvalidOperationException("VAPID private key is not configured.");
    }

    public async Task SendNotificationAsync(
        Guid userId,
        string title,
        string body,
        IEnumerable<PushAction> actions,
        CancellationToken cancellationToken = default)
    {
        // 1. Получаем все подписки пользователя
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FamFitDbContext>();

        var subscriptions = await dbContext.PushSubscriptions
            .Where(ps => ps.UserId == userId)
            .Select(ps => new { ps.Endpoint, ps.P256dh, ps.Auth })
            .ToListAsync(cancellationToken);

        if (!subscriptions.Any())
            return; // Нет подписок — ничего не отправляем

        // 2. Создаём VAPID-детали
        var vapidDetails = new WebPush.VapidDetails(
            $"mailto:{_options.ContactEmail}",
            _options.PublicKey!,
            _options.PrivateKey!);

        // 3. Формируем payload с Notification Actions
        var payload = new PushPayload
        {
            Notification = new NotificationPayload
            {
                Title = title,
                Body = body,
                Actions = actions
                    .Select(a => new ActionPayload
                    {
                        Action = a.Action,
                        Title = a.Title,
                        Icon = a.Icon
                    })
                    .ToList()
            },
            Data = new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            }
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

        // 4. Отправляем уведомления на все подписки
        var pushClient = new WebPushClient();

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSubscription = new WebPush.PushSubscription(
                    sub.Endpoint,
                    sub.P256dh,
                    sub.Auth);

                pushClient.SendNotificationAsync(
                    pushSubscription,
                    jsonPayload,
                    vapidDetails).Wait(cancellationToken);
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                // Подписка истекла — удаляем (логика на уровне Application слоя, но здесь логгируем)
                // В реальном проекте: вызвать Application Command для удаления подписки
                Console.WriteLine($"Push subscription expired for user {userId}: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем цикл для других подписок
                Console.WriteLine($"Failed to send push to user {userId}: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Структура push-уведомления с поддержкой Actions.
/// </summary>
internal sealed class PushPayload
{
    [System.Text.Json.Serialization.JsonPropertyName("notification")]
    public NotificationPayload? Notification { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public Dictionary<string, string>? Data { get; set; }
}

internal sealed class NotificationPayload
{
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string? Title { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("body")]
    public string? Body { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("actions")]
    public List<ActionPayload>? Actions { get; set; }
}

internal sealed class ActionPayload
{
    [System.Text.Json.Serialization.JsonPropertyName("action")]
    public string? Action { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string? Title { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("icon")]
    public string? Icon { get; set; }
}
