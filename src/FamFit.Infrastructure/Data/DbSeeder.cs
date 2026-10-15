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

        // Seed: Attribute Definitions (справочник определений атрибутов)
        if (!await dbContext.ActivityAttributeDefinitions.AnyAsync())
        {
            var defaultDefinitions = new (string Name, string DisplayName, AttributeDataType DataType, string? Unit, int DisplayOrder)[]
            {
                ("Sets", "Подходы", AttributeDataType.Integer, null, 1),
                ("Reps", "Повторения", AttributeDataType.Integer, null, 2),
                ("Weight", "Вес", AttributeDataType.Decimal, "кг", 3),
                ("Distance", "Дистанция", AttributeDataType.Decimal, "км", 4),
                ("Time", "Время", AttributeDataType.Decimal, "мин", 5),
                ("Custom", "Произвольное", AttributeDataType.String, null, 6)
            };

            foreach (var (Name, DisplayName, DataType, Unit, DisplayOrder) in defaultDefinitions)
            {
                var definition = ActivityAttributeDefinition.Create(
                    Name, DisplayName, DataType, Unit, DisplayOrder);
                dbContext.ActivityAttributeDefinitions.Add(definition);
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} attribute definitions", defaultDefinitions.Length);
        }

        // Seed: Activity Types
        if (!await dbContext.ActivityTypes.AnyAsync())
        {
            var defaultTypes = new (string Name, UnitType UnitType, (string DefName, object Value)[] Attributes)[]
            {
                ("Отжимания", UnitType.Count, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 3),
                    (DefName: "Reps", Value: 15),
                    (DefName: "Weight", Value: 20)
                }),
                ("Приседания", UnitType.Count, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 4),
                    (DefName: "Reps", Value: 20),
                    (DefName: "Weight", Value: 30)
                }),
                ("Планка", UnitType.Time, new (string DefName, object Value)[] {
                    (DefName: "Time", Value: 30)
                }),
                ("Бег", UnitType.Time, new (string DefName, object Value)[] {
                    (DefName: "Time", Value: 30),
                    (DefName: "Distance", Value: 5)
                }),
                ("Велосипед", UnitType.Time, new (string DefName, object Value)[] {
                    (DefName: "Time", Value: 45),
                    (DefName: "Distance", Value: 20)
                }),
                ("Жим гантелей лёжа", UnitType.Count, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 4),
                    (DefName: "Reps", Value: 12),
                    (DefName: "Weight", Value: 10m)
                }),
                ("Скручивания", UnitType.Count, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 3),
                    (DefName: "Reps", Value: 20),
                    (DefName: "Weight", Value:16)
                }),
                ("Бёрпи", UnitType.Count, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 3),
                    (DefName: "Reps", Value: 10),
                    (DefName: "Weight", Value: 16)
                })
            };

            // Создаём словарь для маппинга определений
            var definitions = await dbContext.ActivityAttributeDefinitions
                .ToDictionaryAsync(d => d.Name);

            foreach (var (Name, UnitType, Attributes) in defaultTypes)
            {
                var activityType = ActivityType.Create(Name, UnitType);
                dbContext.ActivityTypes.Add(activityType);
                await dbContext.SaveChangesAsync();

                // Создаём значения атрибутов и привязываем к ActivityType
                foreach (var (DefName, Value) in Attributes)
                {
                    if (!definitions.TryGetValue(DefName, out var definition))
                        continue;

                    ActivityAttributeValue attributeValue = Value switch
                    {
                        int intValue => ActivityAttributeValue.Create(definition.Id, intValue: intValue),
                        decimal decimalValue => ActivityAttributeValue.Create(definition.Id, decimalValue: decimalValue),
                        null => ActivityAttributeValue.Create(definition.Id),
                        string strValue => ActivityAttributeValue.Create(definition.Id, stringValue: strValue),
                        _ => ActivityAttributeValue.Create(definition.Id, stringValue: Value.ToString())
                    };

                    dbContext.AttributeValues.Add(attributeValue);
                    await dbContext.SaveChangesAsync();

                    dbContext.ActivityTypeAttributeValues.Add(
                        ActivityTypeAttributeValue.Create(activityType.Id, attributeValue.Id));
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

            // Создаём элементы шаблона с атрибутами
            if (pushupsId != Guid.Empty)
            {
                var item = WorkoutTemplateItem.Create(template.Id, pushupsId, displayOrder: 1);
                dbContext.WorkoutTemplateItems.Add(item);
                await dbContext.SaveChangesAsync();

                await AddTemplateItemAttributes(dbContext, item.Id, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 3),
                    (DefName: "Reps", Value: 15)
                });
            }

            if (squatsId != Guid.Empty)
            {
                var item = WorkoutTemplateItem.Create(template.Id, squatsId, displayOrder: 2);
                dbContext.WorkoutTemplateItems.Add(item);
                await dbContext.SaveChangesAsync();

                await AddTemplateItemAttributes(dbContext, item.Id, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 4),
                    (DefName: "Reps", Value: 20)
                });
            }

            if (benchPressId != Guid.Empty)
            {
                var item = WorkoutTemplateItem.Create(template.Id, benchPressId, displayOrder: 3);
                dbContext.WorkoutTemplateItems.Add(item);
                await dbContext.SaveChangesAsync();

                await AddTemplateItemAttributes(dbContext, item.Id, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 4),
                    (DefName: "Reps", Value: 12),
                    (DefName: "Weight", Value: 10m)
                });
            }

            if (crunchesId != Guid.Empty)
            {
                var item = WorkoutTemplateItem.Create(template.Id, crunchesId, displayOrder: 4);
                dbContext.WorkoutTemplateItems.Add(item);
                await dbContext.SaveChangesAsync();

                await AddTemplateItemAttributes(dbContext, item.Id, new (string DefName, object Value)[] {
                    (DefName: "Sets", Value: 3),
                    (DefName: "Reps", Value: 20)
                });
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded default workout template: {Name}", template.Name);
        }
    }

    private static async Task AddTemplateItemAttributes(
        FamFitDbContext dbContext,
        Guid templateItemId,
        (string DefName, object Value)[] attributes)
    {
        var definitions = await dbContext.ActivityAttributeDefinitions
            .ToDictionaryAsync(d => d.Name);

        foreach (var (DefName, Value) in attributes)
        {
            if (!definitions.TryGetValue(DefName, out var definition))
                continue;

            ActivityAttributeValue attributeValue = Value switch
            {
                int intValue => ActivityAttributeValue.Create(definition.Id, intValue: intValue),
                decimal decimalValue => ActivityAttributeValue.Create(definition.Id, decimalValue: decimalValue),
                null => ActivityAttributeValue.Create(definition.Id),
                string strValue => ActivityAttributeValue.Create(definition.Id, stringValue: strValue),
                _ => ActivityAttributeValue.Create(definition.Id, stringValue: Value.ToString())
            };

            dbContext.AttributeValues.Add(attributeValue);
            await dbContext.SaveChangesAsync();

            dbContext.WorkoutTemplateItemAttributeValues.Add(
                WorkoutTemplateItemAttributeValue.Create(templateItemId, attributeValue.Id));
            await dbContext.SaveChangesAsync();
        }
    }
}
