using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.ActivityAttributes;

/// <summary>
/// Значение атрибута для привязки к сущности.
/// </summary>
public record AttributeValueInput(
    string DefinitionName,
    int? IntValue,
    decimal? DecimalValue,
    bool? BoolValue,
    string? StringValue);

/// <summary>
/// Команда обновления атрибутов ActivityType.
/// Принимает словарь атрибутов по именам определений.
/// </summary>
public record UpdateActivityTypeAttributesCommand(
    Guid ActivityTypeId,
    IReadOnlyList<AttributeValueInput> AttributeValues) : IRequest;

/// <summary>
/// Handler команды UpdateActivityTypeAttributesCommand.
/// </summary>
public class UpdateActivityTypeAttributesHandler : IRequestHandler<UpdateActivityTypeAttributesCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateActivityTypeAttributesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task Handle(
        UpdateActivityTypeAttributesCommand request,
        CancellationToken cancellationToken)
    {
        // Проверяем существование ActivityType
        var activityType = await _dbContext.ActivityTypes
            .FirstOrDefaultAsync(at => at.Id == request.ActivityTypeId, cancellationToken);

        if (activityType is null)
            throw new InvalidOperationException($"ActivityType with Id {request.ActivityTypeId} not found.");

        // Удаляем старые связи
        var existingLinks = _dbContext.ActivityTypeAttributeValues
            .Where(atav => atav.ActivityTypeId == request.ActivityTypeId);
        _dbContext.ActivityTypeAttributeValues.RemoveRange(existingLinks);

        // Обрабатываем каждое значение
        foreach (var attrInput in request.AttributeValues)
        {
            // Находим определение по имени
            var definition = await _dbContext.ActivityAttributeDefinitions
                .FirstOrDefaultAsync(d => d.Name == attrInput.DefinitionName, cancellationToken);

            if (definition is null)
                continue; // Пропускаем неизвестные определения

            // Находим или создаём значение атрибута
            var existingValue = await _dbContext.AttributeValues
                .FirstOrDefaultAsync(av => av.DefinitionId == definition.Id &&
                    MatchValue(av, attrInput, definition.DataType), cancellationToken);

            ActivityAttributeValue attributeValue;

            if (existingValue is null)
            {
                attributeValue = ActivityAttributeValue.Create(
                    definition.Id,
                    attrInput.IntValue,
                    attrInput.DecimalValue,
                    attrInput.BoolValue ?? false,
                    attrInput.StringValue);

                _dbContext.AttributeValues.Add(attributeValue);
            }
            else
            {
                existingValue.Update(
                    attrInput.IntValue,
                    attrInput.DecimalValue,
                    attrInput.BoolValue,
                    attrInput.StringValue);
                attributeValue = existingValue;
            }

            // Создаём связь
            _dbContext.ActivityTypeAttributeValues.Add(
                ActivityTypeAttributeValue.Create(request.ActivityTypeId, attributeValue.Id));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool MatchValue(ActivityAttributeValue value, AttributeValueInput input, Domain.Enums.AttributeDataType dataType)
    {
        return dataType switch
        {
            Domain.Enums.AttributeDataType.Integer => value.IntValue == input.IntValue,
            Domain.Enums.AttributeDataType.Decimal => value.DecimalValue == input.DecimalValue,
            Domain.Enums.AttributeDataType.Boolean => value.BoolValue == (input.BoolValue ?? false),
            Domain.Enums.AttributeDataType.String => value.StringValue == input.StringValue,
            _ => false
        };
    }
}
