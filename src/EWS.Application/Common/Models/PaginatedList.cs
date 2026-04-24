namespace EWS.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalRows { get; }
    public int TotalPage => (int)Math.Ceiling(TotalRows / (double)PageSize);

    public PaginatedList(IReadOnlyList<T> items, int totalRows, int page, int pageSize)
    {
        Items = items;
        TotalRows = totalRows;
        Page = page;
        PageSize = pageSize;
    }
}
