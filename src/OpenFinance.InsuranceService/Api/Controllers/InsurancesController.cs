using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.InsuranceService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.InsuranceService.Api.Controllers;

/// <summary>
/// Insurance API — aligned with Open Finance Brasil Insurance specification (Phase 4).
/// Provides access to insurance policies, premium payments, claims, and coverages.
/// All endpoints require a valid consent token via the x-consent-id header.
/// </summary>
[ApiController]
[Authorize]
[Route("open-finance/v1/insurances")]
[Produces("application/json")]
public class InsurancesController(
    GetInsurancesUseCase getInsurances,
    GetInsuranceDetailsUseCase getInsuranceDetails,
    GetInsurancePremiumUseCase getInsurancePremium,
    GetInsuranceClaimsUseCase getInsuranceClaims,
    GetInsuranceCoveragesUseCase getInsuranceCoverages) : ControllerBase
{
    /// <summary>
    /// Returns the list of insurance policies for the authenticated user.
    /// Requires permission: INSURANCE_READ
    /// </summary>
    [HttpGet]
    [ProducesResponseType<InsuranceListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInsurances(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getInsurances.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns detailed information for a specific insurance policy.
    /// Requires permission: INSURANCE_READ
    /// </summary>
    [HttpGet("{insuranceId:guid}")]
    [ProducesResponseType<InsuranceDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInsuranceDetails(
        Guid insuranceId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getInsuranceDetails.ExecuteAsync(insuranceId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns premium payment details for a specific insurance policy,
    /// including total, paid, and outstanding amounts with individual installments.
    /// Requires permission: INSURANCE_PREMIUM_READ
    /// </summary>
    [HttpGet("{insuranceId:guid}/premium")]
    [ProducesResponseType<InsurancePremiumResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInsurancePremium(
        Guid insuranceId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getInsurancePremium.ExecuteAsync(insuranceId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns all claims filed against a specific insurance policy.
    /// Requires permission: INSURANCE_CLAIMS_READ
    /// </summary>
    [HttpGet("{insuranceId:guid}/claims")]
    [ProducesResponseType<InsuranceClaimListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInsuranceClaims(
        Guid insuranceId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getInsuranceClaims.ExecuteAsync(insuranceId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns all coverage lines for a specific insurance policy.
    /// Requires permission: INSURANCE_COVERAGES_READ
    /// </summary>
    [HttpGet("{insuranceId:guid}/coverages")]
    [ProducesResponseType<InsuranceCoverageListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInsuranceCoverages(
        Guid insuranceId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getInsuranceCoverages.ExecuteAsync(insuranceId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
