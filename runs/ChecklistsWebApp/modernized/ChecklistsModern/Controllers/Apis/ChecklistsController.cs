using ChecklistsModern.Models;
using ChecklistsModern.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistsModern.Controllers.Apis;

[Authorize]
public class ChecklistsController : ControllerBase
{
    private readonly IChecklistService _checklists;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChecklistsController(IChecklistService checklists, UserManager<ApplicationUser> userManager)
    {
        _checklists = checklists;
        _userManager = userManager;
    }

    [HttpDelete("api/checklists/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var checklist = await _checklists.GetChecklistAsync(id);

        if (checklist == null || checklist.IsDeleted)
            return NotFound();

        if (checklist.AuthorId != _userManager.GetUserId(User))
            return Unauthorized();

        checklist.Delete();
        await _checklists.CompleteAsync();

        return NoContent();
    }
}
