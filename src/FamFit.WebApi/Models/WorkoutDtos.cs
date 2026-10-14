namespace FamFit.WebApi.Models;

/// <summary>
/// Запрос на создание тренировки.
/// </summary>
public record CreateWorkoutRequest(string? Name = null);

/// <summary>
/// Запрос на создание шаблона тренировки.
/// </summary>
public record CreateWorkoutTemplateRequest(
    string Name,
    string? Description = null);

/// <summary>
/// Запрос на добавление элемента в шаблон.
/// </summary>
public record AddWorkoutTemplateItemRequest(
    Guid ActivityTypeId,
    int? Sets = null,
    int? Reps = null,
    int? Weight = null);

/// <summary>
/// Запрос на обновление атрибутов типа активности.
/// </summary>
public record UpdateActivityTypeAttributesRequest(
    int? Sets = null,
    int? Reps = null,
    int? Weight = null);
