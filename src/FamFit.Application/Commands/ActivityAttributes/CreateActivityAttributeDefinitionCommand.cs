using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using FamFit.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Application.Commands.ActivityAttributes;

/// <summary>
/// Команда создания определения атрибута (только Admin).
/// </summary>
public record CreateActivityAttributeDefinitionCommand(
    string Name,
    string DisplayName,
    AttributeDataType DataType,
    string? Unit,
    int DisplayOrder) : IRequest<ActivityAttributeDefinition>;

/// <summary>
/// Handler команды CreateActivityAttributeDefinitionCommand.
/// </summary>
public class CreateActivityAttributeDefinitionHandler : IRequestHandler<CreateActivityAttributeDefinitionCommand, ActivityAttributeDefinition>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateActivityAttributeDefinitionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ActivityAttributeDefinition> Handle(
        CreateActivityAttributeDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ActivityAttributeDefinitions
            .AnyAsync(d => d.Name == request.Name, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Attribute definition '{request.Name}' already exists.");

        var definition = ActivityAttributeDefinition.Create(
            request.Name,
            request.DisplayName,
            request.DataType,
            request.Unit,
            request.DisplayOrder);

        _dbContext.ActivityAttributeDefinitions.Add(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return definition;
    }
}
