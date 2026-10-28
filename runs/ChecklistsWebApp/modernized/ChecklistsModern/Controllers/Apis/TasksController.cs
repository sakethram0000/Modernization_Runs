using ChecklistsModern.Models;
using ChecklistsModern.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistsModern.Controllers.Apis;

[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;
    private readonly UserManager<ApplicationUser> _userManager;

    public TasksController(ITaskService tasks, UserManager<ApplicationUser> userManager)
    {
        _tasks = tasks;
        _userManager = userManager;
    }

    [HttpDelete("api/tasks/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _tasks.GetTaskWithChecklistAsync(id);

        if (task == null)
            return NotFound();

        if (task.Checklist!.AuthorId != _userManager.GetUserId(User))
            return Unauthorized();

        task.Delete();
        await _tasks.CompleteAsync();

        return NoContent();
    }

    [HttpPut("api/tasks/{id}/check")]
    public async Task<IActionResult> Check(int? id)
    {
        if (id == null)
            return BadRequest(new ApiMessage("Requires an id for the to-do item"));

        var task = await _tasks.GetTaskWithChecklistAsync(id.Value);

        if (task == null)
            return NotFound();

        if (task.Checklist!.AuthorId != _userManager.GetUserId(User))
            return Unauthorized();

        if (task.IsDeleted)
            return BadRequest(new ApiMessage("Cannot check a deleted item"));

        task.Check();
        await _tasks.CompleteAsync();

        return Ok();
    }

    [HttpDelete("api/tasks/{id}/check")]
    public async Task<IActionResult> Uncheck(int? id)
    {
        if (id == null)
            return BadRequest(new ApiMessage("Requires an id for the to-do item"));

        var task = await _tasks.GetTaskWithChecklistAsync(id.Value);

        if (task == null)
            return NotFound();

        if (task.Checklist!.AuthorId != _userManager.GetUserId(User))
            return Unauthorized();

        if (task.IsDeleted)
            return BadRequest(new ApiMessage("Cannot uncheck a deleted item"));

        task.Uncheck();
        await _tasks.CompleteAsync();

        return NoContent();
    }
}
