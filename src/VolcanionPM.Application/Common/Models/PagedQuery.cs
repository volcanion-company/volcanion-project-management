namespace VolcanionPM.Application.Common.Models;

/// <summary>
/// Base class for paginated queries
/// </summary>
public abstract class PagedQuery
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>
    /// Number of items to skip
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Number of items to take
    /// </summary>
    public int Take => PageSize;
}
