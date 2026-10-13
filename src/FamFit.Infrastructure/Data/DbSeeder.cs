using FamFit.Domain.Entities;
using FamFit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamFit.Infrastructure.Data;

/// <summary>
/// Методы для начального заполнения базы данных (Seed).
/// Вызываются один раз при старте приложения.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Заполняет базу данных начальными данными.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FamFitDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FamFitDbContext>>();

        await dbContext.Database.EnsureCreatedAsync();

        // Seed: Admin пользователь
        if (!await dbContext.Users.AnyAsync())
        {
            // В реальном проекте: хэшировать пароль через Application-сервис (PBKDF2/Argon2)
            var admin = User.Create(
                "Admin",
                "admin@famfit.local",
                "$2a$11$WpIR7diSj61PGuDaXJnE4eJmYlBz.a2fPxqeMflMJlZzd7p0k5cGm", // changeme123
                Role.Admin);

            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded admin user: {Email}", admin.Email);
        }

        // Seed: Activity Types
        if (!await dbContext.ActivityTypes.AnyAsync())
        {
            var defaultTypes = new[]
            {
                ("Отжимания", UnitType.Count),
                ("Приседания", UnitType.Count),
                ("Планка", UnitType.Time),
                ("Бег", UnitType.Time),
                ("Велосипед", UnitType.Time),
                ("Скручивания", UnitType.Count),
                ("Бёрпи", UnitType.Count)
            };

            foreach (var (name, unitType) in defaultTypes)
            {
                var activityType = ActivityType.Create(name, unitType);
                dbContext.ActivityTypes.Add(activityType);
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} default activity types", defaultTypes.Length);
        }
    }
}
