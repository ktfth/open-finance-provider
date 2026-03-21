using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.AccountService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.AccountService.Api.Controllers;

[ApiController]
[Authorize]
[Route("open-finance/v1/accounts")]
[Produces("application/json")]
public class AccountsController(
    GetAccountsUseCase getAccounts,
    GetAccountDetailsUseCase getAccountDetails,
    GetBalanceUseCase getBalance,
    GetTransactionsUseCase getTransactions) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AccountListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getAccounts.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return Ok(result.Value);
    }

    [HttpGet("{accountId:guid}")]
    [ProducesResponseType<AccountDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountDetails(
        Guid accountId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getAccountDetails.ExecuteAsync(accountId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    [HttpGet("{accountId:guid}/balance")]
    [ProducesResponseType<BalanceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(
        Guid accountId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getBalance.ExecuteAsync(accountId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    [HttpGet("{accountId:guid}/transactions")]
    [ProducesResponseType<TransactionListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTransactions(
        Guid accountId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        var result = await getTransactions.ExecuteAsync(accountId, consentId, from, to, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return Ok(result.Value);
    }

}
