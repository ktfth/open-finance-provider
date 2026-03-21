using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.LoanService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.LoanService.Api.Controllers;

[ApiController]
[Authorize]
[Route("open-finance/v1/loans")]
[Produces("application/json")]
public class LoansController(
    GetLoansUseCase getLoans,
    GetLoanDetailsUseCase getLoanDetails,
    GetLoanPaymentsUseCase getLoanPayments,
    GetLoanInstalmentsUseCase getLoanInstalments,
    GetLoanWarrantiesUseCase getLoanWarranties) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<LoanListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLoans(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getLoans.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    [HttpGet("{contractId:guid}")]
    [ProducesResponseType<LoanDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoanDetails(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getLoanDetails.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    [HttpGet("{contractId:guid}/payments")]
    [ProducesResponseType<LoanPaymentListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLoanPayments(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getLoanPayments.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    [HttpGet("{contractId:guid}/instalments")]
    [ProducesResponseType<LoanInstalmentListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLoanInstalments(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getLoanInstalments.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    [HttpGet("{contractId:guid}/warranties")]
    [ProducesResponseType<LoanWarrantyListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLoanWarranties(
        Guid contractId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getLoanWarranties.ExecuteAsync(contractId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
