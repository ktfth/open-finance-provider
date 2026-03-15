using Microsoft.AspNetCore.Mvc;
using OpenFinance.InvestmentService.Application.UseCases;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.InvestmentService.Api.Controllers;

/// <summary>
/// Fixed Income Investments API — aligned with Open Finance Brasil Investments specification.
/// Covers CDB, LCI, LCA, CRI, CRA, Debêntures, Letra Financeira and Fixed Income Funds.
/// Requires permission: INVESTMENTS_FIXED_INCOMES_READ
/// </summary>
[ApiController]
[Route("open-finance/v1/investments/fixed-incomes")]
[Produces("application/json")]
public class FixedIncomeController(
    GetFixedIncomeUseCase getFixedIncome,
    GetFixedIncomeDetailsUseCase getFixedIncomeDetails,
    GetFixedIncomeBalanceUseCase getFixedIncomeBalance,
    GetFixedIncomeTransactionsUseCase getFixedIncomeTransactions) : ControllerBase
{
    /// <summary>
    /// Returns all fixed income investment positions for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<FixedIncomeListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFixedIncome(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getFixedIncome.ExecuteAsync(userId, consentId, ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns full details of a specific fixed income investment position.
    /// Includes ISIN, issuer, rate structure (pre-fixed, post-fixed, hybrid), and tax details.
    /// </summary>
    [HttpGet("{investmentId:guid}")]
    [ProducesResponseType<FixedIncomeDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFixedIncomeDetails(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getFixedIncomeDetails.ExecuteAsync(investmentId, consentId, ct);
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns the current marked-to-market balance for a fixed income investment.
    /// Includes gross amount, income tax, IOF, and net amount at reference date.
    /// </summary>
    [HttpGet("{investmentId:guid}/balances")]
    [ProducesResponseType<FixedIncomeBalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFixedIncomeBalance(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getFixedIncomeBalance.ExecuteAsync(investmentId, consentId, ct);
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns all movements (purchases, redemptions, interest payments) for a fixed income investment
    /// filtered by date range.
    /// </summary>
    [HttpGet("{investmentId:guid}/transactions")]
    [ProducesResponseType<FixedIncomeTransactionListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFixedIncomeTransactions(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        var result = await getFixedIncomeTransactions.ExecuteAsync(investmentId, consentId, from, to, ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}
