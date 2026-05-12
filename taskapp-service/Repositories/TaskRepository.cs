using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class TaskRepository(ApplicationDbContext dbContext) : ITaskRepository
{
    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> QueryPagedAsync(
        TaskListQuery query,
        CancellationToken cancellationToken = default)
    {
        var efQuery = dbContext.TaskItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            efQuery = efQuery.Where(t => t.Title.Contains(query.Search) ||
                                         (t.Description != null && t.Description.Contains(query.Search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var normalized = query.Status.ToLowerInvariant();
            if (normalized == "completed")
            {
                efQuery = efQuery.Where(t => t.IsCompleted);
            }
            else if (normalized == "pending")
            {
                efQuery = efQuery.Where(t => !t.IsCompleted);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            efQuery = efQuery.Where(t => t.Priority == query.Priority);
        }

        efQuery = (query.SortBy.ToLowerInvariant(), query.SortOrder.ToLowerInvariant()) switch
        {
            ("title", "asc") => efQuery.OrderBy(t => t.Title),
            ("title", _) => efQuery.OrderByDescending(t => t.Title),
            ("duedate", "asc") => efQuery.OrderBy(t => t.DueDate),
            ("duedate", _) => efQuery.OrderByDescending(t => t.DueDate),
            ("updatedat", "asc") => efQuery.OrderBy(t => t.UpdatedAt),
            ("updatedat", _) => efQuery.OrderByDescending(t => t.UpdatedAt),
            (_, "asc") => efQuery.OrderBy(t => t.CreatedAt),
            _ => efQuery.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await efQuery.CountAsync(cancellationToken);

        var items = await efQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void Add(TaskItem entity) => dbContext.TaskItems.Add(entity);

    public void Remove(TaskItem entity) => dbContext.TaskItems.Remove(entity);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
