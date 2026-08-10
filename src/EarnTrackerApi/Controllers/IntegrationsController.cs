using System.Text.Json;
using EarnTrackerApi.Dtos.IntegrationDto;
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
    /// <summary>Creates a PayPal Sandbox order and returns its buyer approval link.</summary>
    [HttpPost("paypal/orders")]
    public async Task<ActionResult<JsonElement>> CreatePayPalOrder(
        [FromBody] CreatePayPalOrderDto request,
        CancellationToken cancellationToken)
    {
        using var response = await payPal.CreateOrderAsync(request, cancellationToken);
        return Ok(response.RootElement.Clone());
    }

    /// <summary>Gets the current state of a PayPal Sandbox order.</summary>
    [HttpGet("paypal/orders/{orderId}")]
    public async Task<ActionResult<JsonElement>> GetPayPalOrder(
        string orderId,
        CancellationToken cancellationToken)
    {
        using var response = await payPal.GetOrderAsync(orderId, cancellationToken);
        return Ok(response.RootElement.Clone());
    }

    /// <summary>Captures an order after the sandbox buyer approves it.</summary>
    [HttpPost("paypal/orders/{orderId}/capture")]
    public async Task<ActionResult<JsonElement>> CapturePayPalOrder(
        string orderId,
        CancellationToken cancellationToken)
    {
        using var response = await payPal.CaptureOrderAsync(orderId, cancellationToken);
        return Ok(response.RootElement.Clone());
    }

    /// <summary>Gets a completed PayPal Sandbox capture.</summary>
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
