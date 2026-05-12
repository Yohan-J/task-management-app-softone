using backend.Models;

namespace backend.Repositories;

public interface ITaskRepository
{
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> QueryPagedAsync(
        TaskListQuery query,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    void Add(TaskItem entity);

    void Remove(TaskItem entity);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
