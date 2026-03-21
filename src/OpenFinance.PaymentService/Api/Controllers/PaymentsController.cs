using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.PaymentService.Api.Controllers;

[ApiController]
[Authorize]
[Route("open-finance/v1/payments")]
[Produces("application/json")]
public class PaymentsController(
    InitiatePaymentUseCase initiatePayment,
    GetPaymentStatusUseCase getPaymentStatus,
    CancelPaymentUseCase cancelPayment) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiatePaymentRequest request,
        [FromHeader(Name = "x-idempotency-key")] string? idempotencyKey,
        CancellationToken ct)
    {
        var result = await initiatePayment.ExecuteAsync(request, idempotencyKey, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return CreatedAtAction(nameof(GetStatus), new { paymentId = result.Value.PaymentId }, result.Value);
    }

    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetStatus(
        Guid paymentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getPaymentStatus.ExecuteAsync(consentId, paymentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("{paymentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Cancel(
        Guid paymentId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        [FromBody] CancelPaymentRequest request,
        CancellationToken ct)
    {
        var result = await cancelPayment.ExecuteAsync(consentId, paymentId, request.Reason, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return NoContent();
    }
}

public record CancelPaymentRequest(
    [property: System.ComponentModel.DataAnnotations.Required]
    [property: System.ComponentModel.DataAnnotations.StringLength(500, MinimumLength = 1)]
    string Reason);
