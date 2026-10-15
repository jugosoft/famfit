using FamFit.Application.Commands.ActivityAttributes;
using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.WorkoutTemplates;

/// <summary>
/// Команда добавления элемента в шаблон тренировки с атрибутами.
/// </summary>
public record AddWorkoutTemplateItemCommand(
    Guid TemplateId,
    Guid ActivityTypeId,
    IReadOnlyList<AttributeValueInput> AttributeValues,
    int DisplayOrder) : IRequest<WorkoutTemplateItem>;

/// <summary>
/// Handler команды AddWorkoutTemplateItemCommand.
/// </summary>
public class AddWorkoutTemplateItemHandler : IRequestHandler<AddWorkoutTemplateItemCommand, WorkoutTemplateItem>
{
    private readonly IApplicationDbContext _dbContext;

    public AddWorkoutTemplateItemHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<WorkoutTemplateItem> Handle(
        AddWorkoutTemplateItemCommand request,
        CancellationToken cancellationToken)
    {
        // Проверяем существование шаблона
        var template = await _dbContext.WorkoutTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template is null)
            throw new InvalidOperationException($"WorkoutTemplate with Id {request.TemplateId} not found.");

        // Создаём элемент
        var item = WorkoutTemplateItem.Create(
            request.TemplateId,
            request.ActivityTypeId,
            request.DisplayOrder);
        _dbContext.WorkoutTemplateItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Обрабатываем атрибуты
        foreach (var attrInput in request.AttributeValues)
        {
            var definition = await _dbContext.ActivityAttributeDefinitions
                .FirstOrDefaultAsync(d => d.Name == attrInput.DefinitionName, cancellationToken);

            if (definition is null)
                continue;

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

            _dbContext.WorkoutTemplateItemAttributeValues.Add(
                WorkoutTemplateItemAttributeValue.Create(item.Id, attributeValue.Id));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
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
