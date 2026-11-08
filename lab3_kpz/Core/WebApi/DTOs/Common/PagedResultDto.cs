namespace lab3_kpz.Core.WebApi.DTOs.Common;

public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage)
{
    public static PagedResultDto<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        return new(
            items,
            page,
            pageSize,
            totalCount,
            totalPages,
            page < totalPages,
            page > 1);
    }
}

