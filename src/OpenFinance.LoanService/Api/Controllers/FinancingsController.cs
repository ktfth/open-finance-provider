using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.LoanService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.LoanService.Api.Controllers;

[ApiController]
[Authorize]
[Route("open-finance/v1/financings")]
[Produces("application/json")]
public class FinancingsController(
    GetFinancingsUseCase getFinancings,
    GetFinancingDetailsUseCase getFinancingDetails,
    GetLoanPaymentsUseCase getFinancingPayments,
    GetLoanInstalmentsUseCase getFinancingInstalments) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<FinancingListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFinancings(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getFinancings.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    [HttpGet("{contractId:guid}")]
    [ProducesResponseType<FinancingDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinancingDetails(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getFinancingDetails.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    [HttpGet("{contractId:guid}/payments")]
    [ProducesResponseType<LoanPaymentListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFinancingPayments(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getFinancingPayments.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    [HttpGet("{contractId:guid}/instalments")]
    [ProducesResponseType<LoanInstalmentListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFinancingInstalments(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getFinancingInstalments.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
