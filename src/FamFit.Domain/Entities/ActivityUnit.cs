namespace FamFit.Domain.Entities;

/// <summary>
/// Справочник единиц измерения для типов активностей (например: "Подходы", "Повторения", "Вес(кг)", "Дистанция(км)", "Время(мин)", "Произвольное").
/// </summary>
public sealed class ActivityUnit
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = string.Empty;
    public string Symbol { get; private set; } = string.Empty;
    public string DataType { get; private set; } = "Integer"; // Integer, Decimal, String
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public ICollection<ActivityType> ActivityTypes { get; private set; } = new List<ActivityType>();

    // EF Core parameterless constructor
    private ActivityUnit() { }

    public static ActivityUnit Create(
        string name,
        string symbol,
        string dataType = "Integer",
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));
        if (symbol.Length > 20)
            throw new ArgumentException("Symbol cannot exceed 20 characters.", nameof(symbol));
        if (dataType is not ("Integer" or "Decimal" or "String"))
            throw new ArgumentException($"Invalid dataType: {dataType}. Must be Integer, Decimal, or String.", nameof(dataType));

        return new ActivityUnit
        {
            Name = name.Trim(),
            Symbol = symbol.Trim(),
            DataType = dataType.Trim(),
            DisplayOrder = displayOrder
        };
    }

    public void Update(string? newName = null, string? newSymbol = null, string? newDataType = null, bool? isActive = null, int? newDisplayOrder = null)
    {
        if (newName != null)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Name cannot be empty.", nameof(newName));
            if (newName.Length > 100)
                throw new ArgumentException("Name cannot exceed 100 characters.", nameof(newName));
            Name = newName.Trim();
        }

        if (newSymbol != null)
        {
            if (string.IsNullOrWhiteSpace(newSymbol))
                throw new ArgumentException("Symbol cannot be empty.", nameof(newSymbol));
            if (newSymbol.Length > 20)
                throw new ArgumentException("Symbol cannot exceed 20 characters.", nameof(newSymbol));
            Symbol = newSymbol.Trim();
        }

        if (newDataType != null)
        {
            if (newDataType is not ("Integer" or "Decimal" or "String"))
                throw new ArgumentException($"Invalid dataType: {newDataType}. Must be Integer, Decimal, or String.", nameof(newDataType));
            DataType = newDataType.Trim();
        }

        IsActive = isActive ?? IsActive;
        DisplayOrder = newDisplayOrder ?? DisplayOrder;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
