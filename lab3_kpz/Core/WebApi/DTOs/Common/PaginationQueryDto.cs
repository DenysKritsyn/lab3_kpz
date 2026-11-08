namespace lab3_kpz.Core.WebApi.DTOs.Common;

public class PaginationQueryDto
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
    }

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

