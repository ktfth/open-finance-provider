using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OpenFinance.Shared.Infrastructure;

/// <summary>
/// Action filter that validates model state and returns RFC 7807 problem details on invalid requests.
/// Applied globally via AddOpenFinanceInfrastructure to enforce consistent request validation.
/// </summary>
public sealed class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new UnprocessableEntityObjectResult(problemDetails);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
