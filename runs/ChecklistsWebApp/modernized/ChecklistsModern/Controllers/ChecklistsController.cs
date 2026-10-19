using System.Security.Claims;
using ChecklistsModern.Services;
using ChecklistsModern.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistsModern.Controllers;

[Authorize]
public class ChecklistsController : Controller
{
    private readonly IChecklistService _checklists;
    private readonly ILogger<ChecklistsController> _logger;

    public ChecklistsController(IChecklistService checklists, ILogger<ChecklistsController> logger)
    {
        _checklists = checklists;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var checklists = await _checklists.ListAsync(CurrentUserId());
        return View(checklists);
    }

    public IActionResult New()
    {
        return View("ChecklistForm", new ChecklistFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "")] ChecklistFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("ChecklistForm", viewModel);
        }

        if (viewModel.Id == 0)
        {
            await _checklists.AddAsync(viewModel.Name, CurrentUserId());
        }
        else
        {
            var renamed = await _checklists.RenameAsync(viewModel.Id, viewModel.Name);
            if (!renamed)
            {
                return NotFound();
            }
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var checklist = await _checklists.GetAsync(id.Value);
        if (checklist is null)
        {
            return NotFound();
        }

        if (checklist.AuthorId != CurrentUserId())
        {
            _logger.LogInformation("Rejected edit of checklist {ChecklistId} for a different author", id);
            return Unauthorized();
        }

        return View("ChecklistForm", new ChecklistFormViewModel(checklist));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
