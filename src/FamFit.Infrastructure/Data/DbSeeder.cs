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

        // Seed: Activity Units (единицы измерения активностей)
        if (!await dbContext.ActivityUnits.AnyAsync())
        {
            var defaultUnits = new (string Name, string Symbol, string DataType, int DisplayOrder)[]
            {
                ("Подходы", "подх.", "Integer", 1),
                ("Раз", "раз", "Integer", 2),
                ("Повторения", "повт.", "Integer", 3),
                ("ВесКг", "кг", "Decimal", 4),
                ("Дистанция(км)", "км", "Decimal", 5),
                ("Дистанция", "м", "Decimal", 6),
                ("Время(мин)", "мин", "Decimal", 7),
                ("Время", "сек", "Decimal", 8),
                ("Произвольное", "", "String", 9),
                ("Произвольное", "", "String", 10)
            };

            foreach (var (Name, Symbol, DataType, DisplayOrder) in defaultUnits)
            {
                var unit = ActivityUnit.Create(Name, Symbol, DataType, DisplayOrder);
                dbContext.ActivityUnits.Add(unit);
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} activity units", defaultUnits.Length);
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
            // Получаем ID единиц измерения
            var units = await dbContext.ActivityUnits
                .ToDictionaryAsync(u => u.Name);

            var unitId = units.TryGetValue("Подходы", out var approachesUnit) ? approachesUnit.Id : Guid.Empty;

            if (unitId == Guid.Empty)
            {
                logger.LogWarning("Could not find 'Подходы' activity unit, skipping activity types seed");
            }
            else
            {
                var defaultTypeNames = new[]
                {
                    "Отжимания",
                    "Приседания",
                    "Планка",
                    "Бег",
                    "Велосипед",
                    "Жим гантелей лёжа",
                    "Скручивания",
                    "Бёрпи"
                };

                foreach (var name in defaultTypeNames)
                {
                    var activityType = ActivityType.Create(name, unitId);
                    dbContext.ActivityTypes.Add(activityType);
                    await dbContext.SaveChangesAsync();

                    // Создаём значения атрибутов и привязываем к ActivityType
                    var definitions = await dbContext.ActivityAttributeDefinitions
                        .ToDictionaryAsync(d => d.Name);

                    var attributes = name switch
                    {
                        "Отжимания" or "Приседания" or "Жим гантелей лёжа" or "Скручивания" or "Бёрпи" => new[]
                        {
                            ("Sets", (object)3),
                            ("Reps", (object)15),
                            ("Weight", (object)20)
                        },
                        "Планка" => new[]
                        {
                            ("Time", (object)30)
                        },
                        "Бег" or "Велосипед" => new[]
                        {
                            ("Time", (object)30),
                            ("Distance", (object)5)
                        },
                        _ => Array.Empty<(string, object)>()
                    };

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

                        dbContext.ActivityTypeAttributeValues.Add(
                            ActivityTypeAttributeValue.Create(activityType.Id, attributeValue.Id));
                        await dbContext.SaveChangesAsync();
                    }
                }

                logger.LogInformation("Seeded {Count} default activity types", defaultTypeNames.Length);
            }
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
