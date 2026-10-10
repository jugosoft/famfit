using FamFit.Domain.Enums;

namespace FamFit.Domain.Entities;

/// <summary>
/// Базовая сущность с универсальным идентификатором.
/// </summary>
public abstract record EntityBase<TId>(TId Id)
    where TId : notnull;

public record Entity(T) : EntityBase<Guid>(T);
