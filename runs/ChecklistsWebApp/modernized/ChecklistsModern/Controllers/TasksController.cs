using ChecklistsModern.Models;
using ChecklistsModern.Services;
using ChecklistsModern.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistsModern.Controllers;

// No [Authorize] and no owner check in Save: kept from the legacy controller (approved at Gate 3).
public class TasksController : Controller
{
    private readonly IChecklistService _checklists;
    private readonly ITaskService _tasks;
    private readonly UserManager<ApplicationUser> _userManager;

    public TasksController(IChecklistService checklists, ITaskService tasks, UserManager<ApplicationUser> userManager)
    {
        _checklists = checklists;
        _tasks = tasks;
        _userManager = userManager;
    }

    [Route("checklists/{checklistId}/tasks")]
    public async Task<IActionResult> Index(int? checklistId)
    {
        if (checklistId == null)
            return NotFound();

        var checklist = await _checklists.GetChecklistAsync(checklistId.Value);

        if (checklist == null)
            return NotFound();

        if (checklist.AuthorId != _userManager.GetUserId(User))
            return Challenge();

        var tasks = await _tasks.GetTasksFromChecklistAsync(checklistId.Value);

        return View(new TaskListingViewModel(checklist, tasks));
    }

    [Route("checklists/{checklistId}/tasks/new")]
    public async Task<IActionResult> New(int? checklistId)
    {
        if (checklistId == null)
            return NotFound();

        var checklist = await _checklists.GetChecklistAsync(checklistId.Value);

        if (checklist == null)
            return NotFound();

        if (checklist.AuthorId != _userManager.GetUserId(User))
            return Challenge();

        var task = new ChecklistTask
        {
            ChecklistId = checklistId.Value
        };

        return View("TaskForm", new TaskFormViewModel(task));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ChecklistTask task)
    {
        if (!ModelState.IsValid)
        {
            return View("TaskForm", new TaskFormViewModel(task));
        }

        if (task.Id == 0)
        {
            _tasks.Add(task);
        }
        else
        {
            var taskFromDb = await _tasks.GetTaskFromChecklistAsync(task.ChecklistId, task.Id);

            if (taskFromDb == null)
                return NotFound();

            taskFromDb.Title = task.Title;
        }

        await _tasks.CompleteAsync();

        return RedirectToAction("Index", new { ChecklistId = task.ChecklistId });
    }

    [Route("checklists/{checklistId}/tasks/edit/{id}")]
    public async Task<IActionResult> Edit(int checklistId, int id)
    {
        var task = await _tasks.GetTaskFromChecklistAsync(checklistId, id);

        if (task == null)
            return NotFound();

        if (task.Checklist!.AuthorId != _userManager.GetUserId(User))
            return Challenge();

        return View("TaskForm", new TaskFormViewModel(task));
    }
}
