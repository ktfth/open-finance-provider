using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.InvestmentService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.InvestmentService.Api.Controllers;

/// <summary>
/// Variable Income Investments API — aligned with Open Finance Brasil Investments specification.
/// Covers Stocks (Ações), Equity Funds (FIA), BDRs, ETFs, and Real Estate Investment Funds (FII).
/// Requires permission: INVESTMENTS_VARIABLE_INCOMES_READ
/// </summary>
[ApiController]
[Authorize]
[Route("open-finance/v1/investments/variable-incomes")]
[Produces("application/json")]
public class VariableIncomeController(
    GetVariableIncomeUseCase getVariableIncome,
    GetVariableIncomeDetailsUseCase getVariableIncomeDetails,
    GetVariableIncomeBalanceUseCase getVariableIncomeBalance,
    GetVariableIncomeTransactionsUseCase getVariableIncomeTransactions) : ControllerBase
{
    /// <summary>
    /// Returns all variable income investment positions for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<VariableIncomeListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVariableIncome(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getVariableIncome.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns full details of a specific variable income position.
    /// Includes ticker, ISIN, average cost, current price, gross/net amounts and income tax (IR).
    /// </summary>
    [HttpGet("{investmentId:guid}")]
    [ProducesResponseType<VariableIncomeDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariableIncomeDetails(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getVariableIncomeDetails.ExecuteAsync(investmentId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns the current balance for a variable income position.
    /// Includes quantity, current unit price, gross/net amounts, and yield percentage.
    /// </summary>
    [HttpGet("{investmentId:guid}/balances")]
    [ProducesResponseType<VariableIncomeBalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariableIncomeBalance(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getVariableIncomeBalance.ExecuteAsync(investmentId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns movements (buy, sell, dividends, JCP) for a variable income position
    /// filtered by date range. Includes brokerage fees and tax values.
    /// </summary>
    [HttpGet("{investmentId:guid}/transactions")]
    [ProducesResponseType<VariableIncomeTransactionListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariableIncomeTransactions(
        Guid investmentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        var result = await getVariableIncomeTransactions.ExecuteAsync(investmentId, consentId, from, to, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
