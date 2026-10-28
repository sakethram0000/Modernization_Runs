using ChecklistsModern.Models;
using ChecklistsModern.Services;
using ChecklistsModern.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistsModern.Controllers;

[Authorize]
public class ChecklistsController : Controller
{
    private readonly IChecklistService _checklists;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChecklistsController(IChecklistService checklists, UserManager<ApplicationUser> userManager)
    {
        _checklists = checklists;
        _userManager = userManager;
    }

    // GET: Checklists
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var checklists = await _checklists.GetChecklistsCreatedByUserAsync(userId);
        return View(checklists);
    }

    public IActionResult New()
    {
        var viewModel = new ChecklistFormViewModel();
        return View("ChecklistForm", viewModel);
    }

    // No author check on an existing checklist: kept from the legacy action (approved at Gate 3).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ChecklistFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("ChecklistForm", viewModel);
        }

        if (viewModel.Id == 0)
        {
            var checklist = new Checklist
            {
                Id = viewModel.Id,
                Name = viewModel.Name!,
                AuthorId = _userManager.GetUserId(User)!
            };

            _checklists.Add(checklist);
        }
        else
        {
            var checklist = await _checklists.GetChecklistAsync(viewModel.Id);

            if (checklist == null)
                return NotFound();

            checklist.Name = viewModel.Name!;
        }

        await _checklists.CompleteAsync();

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var checklist = await _checklists.GetChecklistAsync(id.Value);

        if (checklist == null)
            return NotFound();

        if (checklist.AuthorId != _userManager.GetUserId(User))
            return Challenge();

        var viewModel = new ChecklistFormViewModel(checklist);

        return View("ChecklistForm", viewModel);
    }
}
