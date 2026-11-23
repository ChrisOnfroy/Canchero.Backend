namespace Application.Common;

/// <summary>
/// Representa una página de resultados.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    /// <summary>
    /// Obtiene la cantidad total de páginas.
    /// </summary>
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
