using Microsoft.AspNetCore.Mvc;
using OpenFinance.InvestmentService.Application.UseCases;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InvestmentService.Api.Controllers;

/// <summary>
/// Treasury Bonds (Tesouro Direto) API — aligned with Open Finance Brasil Investments specification.
/// Covers Tesouro Prefixado, Tesouro IPCA+, Tesouro IPCA+ com juros semestrais,
/// Tesouro Selic, and Tesouro Prefixado com juros semestrais.
/// Requires permission: INVESTMENTS_TREASURE_TITLES_READ
/// </summary>
[ApiController]
[Route("open-finance/v1/investments/treasure-titles")]
[Produces("application/json")]
public class TreasuryBondsController(
    GetTreasuryBondsUseCase getTreasuryBonds,
    GetTreasuryBondDetailsUseCase getTreasuryBondDetails,
    GetTreasuryBondBalanceUseCase getTreasuryBondBalance) : ControllerBase
{
    /// <summary>
    /// Returns all Tesouro Direto (Treasury Bond) positions for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<TreasuryBondListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTreasuryBonds(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getTreasuryBonds.ExecuteAsync(userId, consentId, ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns detailed data for a specific Tesouro Direto position.
    /// Includes nominal quantity, unit prices, income tax (regressive table), and IOF.
    /// </summary>
    [HttpGet("{investmentId:guid}")]
    [ProducesResponseType<TreasuryBondDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTreasuryBondDetails(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getTreasuryBondDetails.ExecuteAsync(investmentId, consentId, ct);
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns the current balance for a Tesouro Direto position.
    /// Includes gross amount, income tax (IR regressivo), IOF, net amount, and accrued yield.
    /// </summary>
    [HttpGet("{investmentId:guid}/balances")]
    [ProducesResponseType<TreasuryBondBalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTreasuryBondBalance(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getTreasuryBondBalance.ExecuteAsync(investmentId, consentId, ct);
        return result.Value is null ? NotFound() : Ok(result.Value);
    }
}
