using Microsoft.AspNetCore.Mvc;
using OpenFinance.CreditCardService.Application.UseCases;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.CreditCardService.Api.Controllers;

/// <summary>
/// Credit Cards API — aligned with Open Finance Brasil Credit Cards specification.
/// Provides access to card accounts, limits, bills, and bill transactions.
/// All endpoints require a valid consent token via the x-consent-id header.
/// </summary>
[ApiController]
[Route("open-finance/v1/credit-cards-accounts")]
[Produces("application/json")]
public class CardAccountsController(
    GetCardAccountsUseCase getCardAccounts,
    GetCardAccountDetailsUseCase getCardAccountDetails,
    GetCardLimitsUseCase getCardLimits,
    GetCardBillsUseCase getCardBills,
    GetCardBillTransactionsUseCase getCardBillTransactions) : ControllerBase
{
    /// <summary>
    /// Returns a list of active credit card accounts for the authenticated user.
    /// Requires permission: CREDIT_CARDS_ACCOUNTS_READ
    /// </summary>
    [HttpGet]
    [ProducesResponseType<CardAccountListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCardAccounts(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCardAccounts.ExecuteAsync(userId, consentId, ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns detailed information for a specific credit card account.
    /// Requires permission: CREDIT_CARDS_ACCOUNTS_READ
    /// </summary>
    [HttpGet("{cardAccountId:guid}")]
    [ProducesResponseType<CardAccountDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardAccountDetails(
        Guid cardAccountId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCardAccountDetails.ExecuteAsync(cardAccountId, consentId, ct);
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns the credit limits for a specific card account.
    /// Includes total, individual, national, and international limit lines.
    /// Requires permission: CREDIT_CARDS_ACCOUNTS_LIMITS_READ
    /// </summary>
    [HttpGet("{cardAccountId:guid}/limits")]
    [ProducesResponseType<CardLimitsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardLimits(
        Guid cardAccountId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCardLimits.ExecuteAsync(cardAccountId, consentId, ct);
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns all bills (faturas) for a specific card account, ordered by due date descending.
    /// Requires permission: CREDIT_CARDS_ACCOUNTS_BILLS_READ
    /// </summary>
    [HttpGet("{cardAccountId:guid}/bills")]
    [ProducesResponseType<CardBillListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardBills(
        Guid cardAccountId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCardBills.ExecuteAsync(cardAccountId, consentId, ct);
        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns all transactions within a specific bill.
    /// Requires permission: CREDIT_CARDS_ACCOUNTS_BILLS_TRANSACTIONS_READ
    /// </summary>
    [HttpGet("{cardAccountId:guid}/bills/{billId:guid}/transactions")]
    [ProducesResponseType<CardBillTransactionListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardBillTransactions(
        Guid cardAccountId,
        Guid billId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getCardBillTransactions.ExecuteAsync(cardAccountId, billId, consentId, ct);
        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }
}
