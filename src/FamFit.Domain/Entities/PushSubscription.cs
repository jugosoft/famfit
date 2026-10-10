namespace FamFit.Domain.Entities;

/// <summary>
/// Подписка пользователя на Web Push-уведомления (VAPID).
/// </summary>
public sealed class PushSubscription
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid UserId { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string P256dh { get; private set; } = string.Empty;
    public string Auth { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; private set; }

    // EF Core parameterless constructor
    private PushSubscription() { }

    public static PushSubscription Create(Guid userId, string endpoint, string p256dh, string auth)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be empty.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(p256dh))
            throw new ArgumentException("P256dh cannot be empty.", nameof(p256dh));
        if (string.IsNullOrWhiteSpace(auth))
            throw new ArgumentException("Auth cannot be empty.", nameof(auth));

        return new PushSubscription
        {
            UserId = userId,
            Endpoint = endpoint.Trim(),
            P256dh = p256dh.Trim(),
            Auth = auth.Trim()
        };
    }

    public void Update(string endpoint, string p256dh, string auth)
    {
        Endpoint = endpoint.Trim();
        P256dh = p256dh.Trim();
        Auth = auth.Trim();
    }
}
