using FamFit.Application.Interfaces;
using FamFit.Infrastructure.Configuration;
using FamFit.Infrastructure.Data;
using FamFit.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamFit.Infrastructure;

/// <summary>
/// Методы расширения для регистрации Infrastructure-сервисов.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует все сервисы Infrastructure-слоя.
    /// </summary>
    /// <param name="services">IServiceCollection.</param>
    /// <param name="configuration">IConfiguration для чтения секций.</param>
    /// <param name="connectionString">Строка подключения к PostgreSQL (альтернатива конфигурации).</param>
    /// <returns>IServiceCollection для цепочки.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? connectionString = null)
    {
        // --- EF Core + PostgreSQL ---
        var dbConnection = connectionString
            ?? configuration.GetConnectionString("FamFit")
            ?? throw new InvalidOperationException(
                "Connection string 'FamFit' not found. Pass it explicitly or add to appsettings.json.");

        services.AddDbContext<FamFitDbContext>(options =>
        {
            options.UseNpgsql(
                dbConnection,
                np => np.MigrationsAssembly(typeof(FamFitDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention(); // Конвенция именования таблиц в PostgreSQL
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<FamFitDbContext>());

        // --- Web Push (VAPID) ---
        services.Configure<WebPushOptions>(opts =>
        {
            configuration.GetSection("WebPush").Bind(opts);
        });

        services.AddSingleton<IPushNotificationService, PushNotificationService>();

        return services;
    }
}
