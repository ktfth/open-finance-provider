using Microsoft.AspNetCore.Mvc;
using OpenFinance.PaymentService.Application.UseCases;
using OpenFinance.Shared.Contracts;

namespace OpenFinance.PaymentService.Api.Controllers;

[ApiController]
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
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request, CancellationToken ct)
    {
        var result = await initiatePayment.ExecuteAsync(request, ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetStatus), new { paymentId = result.Value.PaymentId }, result.Value);
    }

    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid paymentId, CancellationToken ct)
    {
        var result = await getPaymentStatus.ExecuteAsync(paymentId, ct);
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("{paymentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid paymentId, [FromBody] CancelPaymentRequest request, CancellationToken ct)
    {
        var result = await cancelPayment.ExecuteAsync(paymentId, request.Reason, ct);
        if (result.IsFailure)
            return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return NoContent();
    }
}

public record CancelPaymentRequest(string Reason);
