namespace backend.DTOs;

public class PagedTasksResponse
{
    public List<TaskResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
