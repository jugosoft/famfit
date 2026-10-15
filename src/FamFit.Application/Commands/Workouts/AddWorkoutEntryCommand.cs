using FamFit.Application.Commands.ActivityAttributes;
using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.Workouts;

/// <summary>
/// Команда добавления записи в тренировку с атрибутами.
/// </summary>
public record AddWorkoutEntryCommand(
    Guid WorkoutId,
    Guid ActivityTypeId,
    IReadOnlyList<AttributeValueInput> AttributeValues) : IRequest<WorkoutEntry>;

/// <summary>
/// Handler команды AddWorkoutEntryCommand.
/// </summary>
public class AddWorkoutEntryHandler : IRequestHandler<AddWorkoutEntryCommand, WorkoutEntry>
{
    private readonly IApplicationDbContext _dbContext;

    public AddWorkoutEntryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<WorkoutEntry> Handle(
        AddWorkoutEntryCommand request,
        CancellationToken cancellationToken)
    {
        // Проверяем существование Workout
        var workout = await _dbContext.Workouts
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout is null)
            throw new InvalidOperationException($"Workout with Id {request.WorkoutId} not found.");

        // Создаём запись
        var entry = WorkoutEntry.Create(request.WorkoutId, request.ActivityTypeId);
        _dbContext.WorkoutEntries.Add(entry);
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

            _dbContext.WorkoutEntryAttributeValues.Add(
                WorkoutEntryAttributeValue.Create(entry.Id, attributeValue.Id));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry;
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
