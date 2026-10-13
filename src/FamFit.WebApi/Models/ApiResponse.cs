namespace FamFit.WebApi.Models;

/// <summary>
/// Универсальный ответ API.
/// </summary>
public record ApiResponse<T>(T? Data, string? Error = null)
{
    public bool Success => Error is null;
}

/// <summary>
/// Ответ с пагинацией.
/// </summary>
public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
