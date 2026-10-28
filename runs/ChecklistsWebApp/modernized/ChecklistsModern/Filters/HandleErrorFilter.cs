using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChecklistsModern.Filters;

// Port of the global HandleErrorAttribute: an unhandled action exception renders Views/Shared/Error.
public class HandleErrorFilter : IExceptionFilter
{
    private readonly ILogger<HandleErrorFilter> _logger;

    public HandleErrorFilter(ILogger<HandleErrorFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in {Action}", context.ActionDescriptor.DisplayName);
        context.Result = new ViewResult
        {
            ViewName = "Error",
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }
}
