using backend.DTOs;
using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class TaskService(ITaskRepository taskRepository) : ITaskService
{
    private const int MaxPageSize = 100;

    public async Task<PagedTasksResponse> GetTasksAsync(
        string? search,
        string? status,
        string? priority,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }
        else if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        var listQuery = new TaskListQuery(
            Search: search,
            Status: status,
            Priority: priority,
            SortBy: sortBy ?? "createdAt",
            SortOrder: sortOrder ?? "desc",
            Page: page,
            PageSize: pageSize);

        var (items, totalCount) = await taskRepository.QueryPagedAsync(listQuery, cancellationToken);

        return new PagedTasksResponse
        {
            Items = items.Select(ToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        return task is null ? null : ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(TaskUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            IsCompleted = request.IsCompleted,
            CreatedAt = now,
            UpdatedAt = now
        };

        taskRepository.Add(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, TaskUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.IsCompleted = request.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(task);
    }

    public async Task<TaskResponse?> MarkCompletedAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.IsCompleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        await taskRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        taskRepository.Remove(task);
        await taskRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static TaskResponse ToResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        IsCompleted = task.IsCompleted,
        Priority = task.Priority,
        DueDate = task.DueDate,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
