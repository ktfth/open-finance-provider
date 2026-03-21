using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.ResourcesService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.ResourcesService.Api.Controllers;

[ApiController]
[Authorize]
[Route("open-finance/v1/resources")]
[Produces("application/json")]
public class ResourcesController(GetResourcesUseCase getResources) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ResourceListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResources(
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getResources.ExecuteAsync(consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
