namespace FamFit.Infrastructure.Configuration;

/// <summary>
/// Конфигурация Web Push (VAPID) для отправки уведомлений.
/// Ключи генерируются один раз при старте приложения через WebPush library.
/// </summary>
public sealed class WebPushOptions
{
    /// <summary>
    /// VAPID публичный ключ — передаётся фронтенду при регистрации подписки.
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// VAPID приватный ключ — используется для подписи push-сообщений.
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// Email администратора для VAPID (требование спецификации Web Push).
    /// </summary>
    public string? ContactEmail { get; set; } = "admin@famfit.local";
}
