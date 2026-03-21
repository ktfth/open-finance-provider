using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinance.ConsentService.Application.UseCases;
using OpenFinance.Shared.Contracts;
using OpenFinance.Shared.Infrastructure;

namespace OpenFinance.ConsentService.Api.Controllers;

[ApiController]
[Authorize]
[Route("open-finance/v1/consents")]
[Produces("application/json")]
public class ConsentsController(
    CreateConsentUseCase createConsent,
    GetConsentUseCase getConsent,
    RevokeConsentUseCase revokeConsent,
    ValidateConsentUseCase validateConsent) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ConsentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateConsentRequest request, CancellationToken ct)
    {
        var result = await createConsent.ExecuteAsync(request, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return CreatedAtAction(nameof(GetById), new { id = result.Value.ConsentId }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ConsentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await getConsent.ExecuteAsync(id, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return result.Value is null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(Guid id, [FromBody] RevokeRequest request, CancellationToken ct)
    {
        var result = await revokeConsent.ExecuteAsync(id, request.Reason, ct);
        if (result.IsFailure)
            return result.Error!.ToErrorResponse();

        return NoContent();
    }

    [HttpGet("{id:guid}/validate")]
    [AllowAnonymous] // Inter-service endpoint: called by HttpConsentValidator without user JWT
    [ProducesResponseType<ConsentValidationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate(
        Guid id,
        [FromQuery] string[] permissions,
        CancellationToken ct)
    {
        var result = await validateConsent.ExecuteAsync(id, permissions, ct);
        return Ok(new ConsentValidationResponse(result.Value));
    }
}

public record RevokeRequest(
    [property: System.ComponentModel.DataAnnotations.Required]
    [property: System.ComponentModel.DataAnnotations.StringLength(500, MinimumLength = 1)]
    string Reason);
public record ConsentValidationResponse(bool IsValid);
