using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedTasksResponse>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortOrder = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await taskService.GetTasksAsync(
            search,
            status,
            priority,
            sortBy,
            sortOrder,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound(new { message = "Task not found." });
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] TaskUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(
        int id,
        [FromBody] TaskUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await taskService.UpdateAsync(id, request, cancellationToken);
        if (task is null)
        {
            return NotFound(new { message = "Task not found." });
        }

        return Ok(task);
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult<TaskResponse>> MarkCompleted(int id, CancellationToken cancellationToken = default)
    {
        var task = await taskService.MarkCompletedAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound(new { message = "Task not found." });
        }

        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await taskService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = "Task not found." });
        }

        return NoContent();
    }
}
