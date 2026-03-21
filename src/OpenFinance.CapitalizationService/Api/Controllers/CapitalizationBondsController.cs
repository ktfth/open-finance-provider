using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.CapitalizationService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.CapitalizationService.Api.Controllers;

/// <summary>
/// Capitalization Bonds API — aligned with Open Finance Brasil Capitalization specification (Phase 4).
/// Covers Traditional, Incentive, Popular, and Compulsory Purchase bond modalities.
/// Requires permission: CAPITALIZATION_TITLES_READ
/// </summary>
[ApiController]
[Authorize]
[Route("open-finance/v1/capitalization-bonds")]
[Produces("application/json")]
public class CapitalizationBondsController(
    GetCapitalizationBondsUseCase getCapitalizationBonds,
    GetCapitalizationBondDetailsUseCase getCapitalizationBondDetails,
    GetCapitalizationBondPaymentsUseCase getCapitalizationBondPayments) : ControllerBase
{
    /// <summary>
    /// Returns all capitalization bonds held by the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<CapitalizationBondListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCapitalizationBonds(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCapitalizationBonds.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns full details of a specific capitalization bond.
    /// Includes payment schedule, redemption percentages, prize draw amounts, and mathematical reserve.
    /// </summary>
    [HttpGet("{bondId:guid}")]
    [ProducesResponseType<CapitalizationBondDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapitalizationBondDetails(
        Guid bondId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCapitalizationBondDetails.ExecuteAsync(bondId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns the payment instalment list for a specific capitalization bond.
    /// Includes due dates, amounts, currency, and payment status.
    /// </summary>
    [HttpGet("{bondId:guid}/payments")]
    [ProducesResponseType<CapitalizationBondPaymentListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCapitalizationBondPayments(
        Guid bondId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCapitalizationBondPayments.ExecuteAsync(bondId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
