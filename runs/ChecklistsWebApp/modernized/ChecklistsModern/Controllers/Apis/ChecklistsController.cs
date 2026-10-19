using System.Security.Claims;
using ChecklistsModern.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistsModern.Controllers.Apis;

[Authorize]
[Route("api/checklists")]
public class ChecklistsController : ControllerBase
{
    private readonly IChecklistService _checklists;

    public ChecklistsController(IChecklistService checklists)
    {
        _checklists = checklists;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _checklists.DeleteAsync(id, authorId);

        if (result == DeleteResult.NotFound)
        {
            return NotFound();
        }

        if (result == DeleteResult.NotAuthor)
        {
            return Unauthorized();
        }

        return NoContent();
    }
}
