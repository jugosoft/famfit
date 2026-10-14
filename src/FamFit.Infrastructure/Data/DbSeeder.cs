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

        // Применяем все доступные миграции (если есть pending migrations)
        await dbContext.Database.MigrateAsync();

        // Seed: Admin пользователь
        if (!await dbContext.Users.AnyAsync())
        {
            // В реальном проекте: хэшировать пароль через Application-сервис (PBKDF2/Argon2)
            var admin = User.Create(
                "Admin",
                "admin@famfit.local",
                "$2a$12$uZpZrAaQmGPPc45zcFYrgeg5SUyRQa/oI4fbehk93KSbZarTHAGrC", // changeme123
                Role.Admin);

            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded admin user: {Email}", admin.Email);
        }

        // Seed: Activity Types
        if (!await dbContext.ActivityTypes.AnyAsync())
        {
            var defaultTypes = new (string Name, UnitType UnitType, int? Sets, int? Reps, int? Weight)[]
            {
                ("Отжимания", UnitType.Count, 3, 15, null),
                ("Приседания", UnitType.Count, 4, 20, null),
                ("Планка", UnitType.Time, null, null, null),
                ("Бег", UnitType.Time, null, null, null),
                ("Велосипед", UnitType.Time, null, null, null),
                ("Жим гантелей лёжа", UnitType.Count, 4, 12, 10),
                ("Скручивания", UnitType.Count, 3, 20, null),
                ("Бёрпи", UnitType.Count, 3, 10, null)
            };

            foreach (var (Name, UnitType, Sets, Reps, Weight) in defaultTypes)
            {
                var activityType = ActivityType.Create(Name, UnitType);
                dbContext.ActivityTypes.Add(activityType);
                await dbContext.SaveChangesAsync();

                // Создаём атрибуты если они есть
                if (Sets.HasValue || Reps.HasValue || Weight.HasValue)
                {
                    var attributes = ActivityTypeAttributes.Create(
                        activityType.Id, Sets, Reps, Weight);
                    dbContext.ActivityTypeAttributes.Add(attributes);
                    await dbContext.SaveChangesAsync();
                }
            }

            logger.LogInformation("Seeded {Count} default activity types", defaultTypes.Length);
        }

        // Seed: Default Workout Template
        if (!await dbContext.WorkoutTemplates.AnyAsync())
        {
            // Получаем ID админа и типов для шаблона
            var adminId = await dbContext.Users
                .Where(u => u.Role == Role.Admin)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            var pushupsId = await dbContext.ActivityTypes
                .Where(at => at.Name == "Отжимания")
                .Select(at => at.Id)
                .FirstOrDefaultAsync();

            var squatsId = await dbContext.ActivityTypes
                .Where(at => at.Name == "Приседания")
                .Select(at => at.Id)
                .FirstOrDefaultAsync();

            var benchPressId = await dbContext.ActivityTypes
                .Where(at => at.Name == "Жим гантелей лёжа")
                .Select(at => at.Id)
                .FirstOrDefaultAsync();

            var crunchesId = await dbContext.ActivityTypes
                .Where(at => at.Name == "Скручивания")
                .Select(at => at.Id)
                .FirstOrDefaultAsync();

            // Шаблон тренировки для админа
            var template = WorkoutTemplate.Create(
                adminId,
                "Утренняя тренировка",
                "Классическая круговая тренировка для всей семьи");

            dbContext.WorkoutTemplates.Add(template);
            await dbContext.SaveChangesAsync();

            // Добавляем элементы в шаблон
            if (pushupsId != Guid.Empty)
            {
                dbContext.WorkoutTemplateItems.Add(WorkoutTemplateItem.Create(
                    template.Id, pushupsId, sets: 3, reps: 15, weight: null, displayOrder: 1));
            }

            if (squatsId != Guid.Empty)
            {
                dbContext.WorkoutTemplateItems.Add(WorkoutTemplateItem.Create(
                    template.Id, squatsId, sets: 4, reps: 20, weight: null, displayOrder: 2));
            }

            if (benchPressId != Guid.Empty)
            {
                dbContext.WorkoutTemplateItems.Add(WorkoutTemplateItem.Create(
                    template.Id, benchPressId, sets: 4, reps: 12, weight: 10, displayOrder: 3));
            }

            if (crunchesId != Guid.Empty)
            {
                dbContext.WorkoutTemplateItems.Add(WorkoutTemplateItem.Create(
                    template.Id, crunchesId, sets: 3, reps: 20, weight: null, displayOrder: 4));
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded default workout template: {Name}", template.Name);
        }
    }
}
