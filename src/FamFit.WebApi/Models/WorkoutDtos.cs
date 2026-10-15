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
/// Ввод значения атрибута для шаблона.
/// </summary>
public record TemplateAttributeInput(
    string DefinitionName,
    int? IntValue,
    decimal? DecimalValue,
    bool? BoolValue,
    string? StringValue);

/// <summary>
/// Запрос на добавление элемента в шаблон.
/// </summary>
public record AddWorkoutTemplateItemRequest(
    Guid ActivityTypeId,
    IReadOnlyList<TemplateAttributeInput> AttributeValues,
    int DisplayOrder = 0);
