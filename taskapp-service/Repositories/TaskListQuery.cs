namespace backend.Repositories;

/// <summary>
/// Normalized filter and paging inputs for task list queries (built by the service layer).
/// </summary>
public sealed record TaskListQuery(
    string? Search,
    string? Status,
    string? Priority,
    string SortBy,
    string SortOrder,
    int Page,
    int PageSize);
