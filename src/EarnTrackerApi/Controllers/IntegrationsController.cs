using System.Text.Json;
using EarnTrackerApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EarnTrackerApi.Controllers;

[ApiController]
[Authorize]
[Route("api/integrations")]
public sealed class IntegrationsController(
    IPayPalService payPal,
    IPayStackService payStack) : ControllerBase
{
    [HttpGet("paypal/captures/{captureId}")]
    public async Task<ActionResult<JsonElement>> GetPayPalCapture(
        string captureId,
        CancellationToken cancellationToken)
    {
        using var response = await payPal.GetCaptureAsync(captureId, cancellationToken);
        return Ok(response.RootElement.Clone());
    }

    [HttpGet("paystack/transactions/{reference}")]
    public async Task<ActionResult<JsonElement>> VerifyPayStackTransaction(
        string reference,
        CancellationToken cancellationToken)
    {
        using var response = await payStack.VerifyTransactionAsync(
            reference,
            cancellationToken);
        return Ok(response.RootElement.Clone());
    }
}
