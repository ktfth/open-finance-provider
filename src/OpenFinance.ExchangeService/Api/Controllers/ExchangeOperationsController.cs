using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.ExchangeService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.ExchangeService.Api.Controllers;

/// <summary>
/// Foreign Exchange Operations API — aligned with Open Finance Brasil Exchange specification (Phase 4).
/// Provides access to exchange operations, details, and lifecycle events.
/// Requires permission: EXCHANGES_READ
/// </summary>
[ApiController]
[Authorize]
[Route("open-finance/v1/exchanges")]
[Produces("application/json")]
public class ExchangeOperationsController(
    GetExchangeOperationsUseCase getExchangeOperations,
    GetExchangeOperationDetailsUseCase getExchangeOperationDetails,
    GetExchangeEventsUseCase getExchangeEvents) : ControllerBase
{
    /// <summary>
    /// Returns all foreign exchange operations for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<ExchangeOperationListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExchangeOperations(
        [FromQuery] string userId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getExchangeOperations.ExecuteAsync(userId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns full details of a specific foreign exchange operation.
    /// Includes exchange rate, VET, IOF, IR, counterparty and delivery information.
    /// </summary>
    [HttpGet("{operationId:guid}")]
    [ProducesResponseType<ExchangeOperationDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExchangeOperationDetails(
        Guid operationId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getExchangeOperationDetails.ExecuteAsync(operationId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    /// <summary>
    /// Returns all lifecycle events for a foreign exchange operation.
    /// Events include closings, settlements, partial settlements, cancellations and amendments.
    /// </summary>
    [HttpGet("{operationId:guid}/events")]
    [ProducesResponseType<ExchangeEventListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExchangeEvents(
        Guid operationId,
        [FromHeader(Name = "x-consent-id")] Guid consentId,
        CancellationToken ct)
    {
        var result = await getExchangeEvents.ExecuteAsync(operationId, consentId, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();
        return Ok(result.Value);
    }
}
