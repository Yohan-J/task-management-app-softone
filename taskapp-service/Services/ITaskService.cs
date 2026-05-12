using backend.DTOs;

namespace backend.Services;

public interface ITaskService
{
    Task<PagedTasksResponse> GetTasksAsync(
        string? search,
        string? status,
        string? priority,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(TaskUpsertRequest request, CancellationToken cancellationToken = default);

    Task<TaskResponse?> UpdateAsync(int id, TaskUpsertRequest request, CancellationToken cancellationToken = default);

    Task<TaskResponse?> MarkCompletedAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
