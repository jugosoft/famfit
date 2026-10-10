namespace FamFit.Application.Interfaces;

/// <summary>
/// Сервис отправки Web Push-уведомлений (VAPID).
/// Реализация находится в Infrastructure-слое.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Отправляет push-уведомление пользователю по его подписке.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="title">Заголовок уведомления.</param>
    /// <param name="body">Тело уведомления.</param>
    /// <param name="actions">Список кнопок действий (Notification Actions).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Task.</returns>
    Task SendNotificationAsync(
        Guid userId,
        string title,
        string body,
        IEnumerable<PushAction> actions,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Описание кнопки действия в push-уведомлении.
/// </summary>
public record PushAction(string Action, string Title, string Icon);
