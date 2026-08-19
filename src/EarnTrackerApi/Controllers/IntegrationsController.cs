using System.Text.Json;
using EarnTrackerApi.Dtos.IntegrationDto;
using EarnTrackerApi.Extensions;
using EarnTrackerApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EarnTrackerApi.Controllers;

[ApiController]
[Authorize]
[Route("api/integrations")]
public sealed class IntegrationsController(
    IPayPalService payPal,
    IPayStackService payStack,
    IPaymentRecordingService paymentRecorder) : ControllerBase
{
    /// <summary>Records a fictional payment for product demonstration only.</summary>
    [HttpPost("demo/payments")]
    public async Task<ActionResult> CreateDemoPayment(
        [FromBody] CreateDemoPaymentDto request,
        CancellationToken cancellationToken)
    {
        var transaction = await paymentRecorder.RecordDemoPaymentAsync(
            User.GetUserId(),
            request,
            cancellationToken);

        return Ok(new
        {
            transaction.Id,
            transaction.ExternalId,
            transaction.Amount,
            transaction.Fee,
            transaction.Currency,
            transaction.Status,
            transaction.Description,
            transaction.OccurredAt,
            IsDemo = true
        });
    }

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
        await paymentRecorder.RecordPayPalCapturesAsync(
            User.GetUserId(),
            response.RootElement,
            cancellationToken);
        return Ok(response.RootElement.Clone());
    }

    /// <summary>Captures an order after the sandbox buyer approves it.</summary>
    [HttpPost("paypal/orders/{orderId}/capture")]
    public async Task<ActionResult<JsonElement>> CapturePayPalOrder(
        string orderId,
        CancellationToken cancellationToken)
    {
        using var response = await payPal.CaptureOrderAsync(orderId, cancellationToken);
        await paymentRecorder.RecordPayPalCapturesAsync(
            User.GetUserId(),
            response.RootElement,
            cancellationToken);
        return Ok(response.RootElement.Clone());
    }

    /// <summary>Gets a completed PayPal Sandbox capture.</summary>
    [HttpGet("paypal/captures/{captureId}")]
    public async Task<ActionResult<JsonElement>> GetPayPalCapture(
        string captureId,
        CancellationToken cancellationToken)
    {
        using var response = await payPal.GetCaptureAsync(captureId, cancellationToken);
        await paymentRecorder.RecordPayPalCapturesAsync(
            User.GetUserId(),
            response.RootElement,
            cancellationToken);
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
