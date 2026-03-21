using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.PensionService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.PensionService.Api.Controllers;

/// <summary>
/// Pension Plans API — aligned with Open Finance Brasil Pension specification (Phase 4).
/// Covers PGBL, VGBL, and other supplementary pension products.
/// Requires permission: PENSION_READ
/// </summary>
[ApiController]
[Authorize]
[Route("open-finance/v1/pensions")]
[Produces("application/json")]
public class PensionsController(
    GetPensionsUseCase getPensions,
    GetPensionDetailsUseCase getPensionDetails,
    GetPensionBalanceUseCase getPensionBalance,
    GetPensionContributionsUseCase getPensionContributions,
    GetPensionWithdrawalsUseCase getPensionWithdrawals) : ControllerBase
{
    /// <summary>
    /// Returns all pension plans held by the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PensionListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPensions(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getPensions.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns full details for a specific pension plan, including tax regime,
    /// beneficiary information, fee structure, and scheduled retirement date.
    /// </summary>
    [HttpGet("{pensionId:guid}")]
    [ProducesResponseType<PensionDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPensionDetails(
        Guid pensionId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getPensionDetails.ExecuteAsync(pensionId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns the current financial balance of a pension plan at the latest reference date.
    /// Includes gross/net balance, accumulated contributions, yield, and fees applied.
    /// </summary>
    [HttpGet("{pensionId:guid}/balance")]
    [ProducesResponseType<PensionBalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPensionBalance(
        Guid pensionId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getPensionBalance.ExecuteAsync(pensionId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns all contribution records for a pension plan, ordered by most recent first.
    /// Includes regular, extra, portability-in, and employer contributions.
    /// </summary>
    [HttpGet("{pensionId:guid}/contributions")]
    [ProducesResponseType<PensionContributionListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPensionContributions(
        Guid pensionId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getPensionContributions.ExecuteAsync(pensionId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns all withdrawal and redemption events for a pension plan.
    /// Includes gross amount, withholding tax, and net amount for each event.
    /// </summary>
    [HttpGet("{pensionId:guid}/withdrawals")]
    [ProducesResponseType<PensionWithdrawalListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPensionWithdrawals(
        Guid pensionId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getPensionWithdrawals.ExecuteAsync(pensionId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
