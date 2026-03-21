using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.LoanService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.LoanService.Api.Controllers;

[ApiController]
[Authorize]
[Route("open-finance/v1/unarranged-accounts-overdraft")]
[Produces("application/json")]
public class OverdraftsController(
    GetOverdraftsUseCase getOverdrafts,
    GetOverdraftDetailsUseCase getOverdraftDetails) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<OverdraftListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOverdrafts(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getOverdrafts.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    [HttpGet("{contractId:guid}")]
    [ProducesResponseType<OverdraftDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOverdraftDetails(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getOverdraftDetails.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }
}
