namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Standard pagination parameters for list endpoints.
/// Open Finance Brasil spec requires pagination support.
/// </summary>
public sealed record PaginationParams
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 25;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
    public int Take => NormalizedPageSize;

    public int NormalizedPage => Page < 1 ? 1 : Page;
    public int NormalizedPageSize => PageSize < 1 ? DefaultPageSize : PageSize > MaxPageSize ? MaxPageSize : PageSize;
}

/// <summary>
/// Wrapper for paginated responses.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Data,
    PaginationMeta Meta
);

public sealed record PaginationMeta(
    int TotalRecords,
    int TotalPages,
    int CurrentPage,
    int PageSize
)
{
    public static PaginationMeta From(int totalRecords, PaginationParams pagination) => new(
        totalRecords,
        (int)Math.Ceiling(totalRecords / (double)pagination.NormalizedPageSize),
        pagination.NormalizedPage,
        pagination.NormalizedPageSize
    );
}
