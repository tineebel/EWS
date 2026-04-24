namespace EWS.API.Models;

public class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalRows { get; init; }
    public int TotalPage { get; init; }
}
