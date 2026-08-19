using System.Globalization;
using System.Text.Json;
using EarnTrackerApi.Exceptions;
using EarnTrackerApi.Dtos.IntegrationDto;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Models;

namespace EarnTrackerApi.Services;

public sealed class PaymentRecordingService(IUnitOfWork unitOfWork)
    : IPaymentRecordingService
{
    private const string Provider = "PayPal";

    public async Task<EarningTransaction> RecordDemoPaymentAsync(
        Guid userId,
        CreateDemoPaymentDto request,
        CancellationToken cancellationToken = default)
    {
        const string demoProvider = "Demo";
        var currency = request.Currency.Trim().ToUpperInvariant();
        var source = await unitOfWork.Library.GetIncomeSourceAsync(
            userId,
            demoProvider,
            currency,
            cancellationToken);

        if (source is null)
        {
            source = new IncomeSource
            {
                UserId = userId,
                Name = "Demo payments",
                Provider = demoProvider,
                Currency = currency
            };
            await unitOfWork.Library.AddIncomeSourceAsync(source, cancellationToken);
        }

        var transaction = new EarningTransaction
        {
            IncomeSourceId = source.Id,
            ExternalId = $"DEMO-{Guid.NewGuid():N}",
            Amount = request.Amount,
            Fee = 0,
            Currency = currency,
            Status = "Completed",
            Description = $"Demo · {request.Description.Trim()}",
            OccurredAt = DateTimeOffset.UtcNow
        };
        await unitOfWork.Library.AddTransactionAsync(transaction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task RecordPayPalCapturesAsync(
        Guid userId,
        JsonElement response,
        CancellationToken cancellationToken = default)
    {
        var captures = ReadCaptures(response).ToList();
        if (captures.Count == 0)
        {
            return;
        }

        var sources = new Dictionary<string, IncomeSource>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var capture in captures)
        {
            if (!capture.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!sources.TryGetValue(capture.Currency, out var source))
            {
                source = await unitOfWork.Library.GetIncomeSourceAsync(
                    userId,
                    Provider,
                    capture.Currency,
                    cancellationToken);

                if (source is null)
                {
                    source = new IncomeSource
                    {
                        UserId = userId,
                        Name = Provider,
                        Provider = Provider,
                        Currency = capture.Currency
                    };
                    await unitOfWork.Library.AddIncomeSourceAsync(source, cancellationToken);
                }

                sources[capture.Currency] = source;
            }

            var transaction = await unitOfWork.Library.GetTransactionAsync(
                source.Id,
                capture.Id,
                cancellationToken);

            if (transaction is null)
            {
                transaction = new EarningTransaction
                {
                    IncomeSourceId = source.Id,
                    ExternalId = capture.Id,
                    Amount = capture.Amount,
                    Fee = capture.Fee,
                    Currency = capture.Currency,
                    Status = "Completed",
                    Description = capture.Description,
                    OccurredAt = capture.OccurredAt
                };
                await unitOfWork.Library.AddTransactionAsync(
                    transaction,
                    cancellationToken);
            }
            else
            {
                transaction.Amount = capture.Amount;
                transaction.Fee = capture.Fee;
                transaction.Status = "Completed";
                transaction.Description = capture.Description;
                transaction.OccurredAt = capture.OccurredAt;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<PayPalCapture> ReadCaptures(JsonElement response)
    {
        if (response.TryGetProperty("purchase_units", out var purchaseUnits))
        {
            foreach (var purchaseUnit in purchaseUnits.EnumerateArray())
            {
                var description = ReadString(purchaseUnit, "description") ??
                    "PayPal payment";
                if (!purchaseUnit.TryGetProperty("payments", out var payments) ||
                    !payments.TryGetProperty("captures", out var captures))
                {
                    continue;
                }

                foreach (var capture in captures.EnumerateArray())
                {
                    yield return ParseCapture(capture, description);
                }
            }

            yield break;
        }

        if (response.TryGetProperty("amount", out _))
        {
            yield return ParseCapture(response, "PayPal payment");
        }
    }

    private static PayPalCapture ParseCapture(
        JsonElement capture,
        string description)
    {
        var id = ReadRequiredString(capture, "id");
        var status = ReadRequiredString(capture, "status");
        var amountElement = capture.GetProperty("amount");
        var currency = ReadRequiredString(amountElement, "currency_code")
            .ToUpperInvariant();
        var amount = ReadDecimal(amountElement, "value");
        var fee = 0m;

        if (capture.TryGetProperty("seller_receivable_breakdown", out var breakdown) &&
            breakdown.TryGetProperty("paypal_fee", out var feeElement))
        {
            fee = ReadDecimal(feeElement, "value");
        }

        var occurredAt = DateTimeOffset.UtcNow;
        var timestamp = ReadString(capture, "create_time");
        if (DateTimeOffset.TryParse(
            timestamp,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var parsed))
        {
            occurredAt = parsed;
        }

        return new PayPalCapture(
            id,
            amount,
            fee,
            currency,
            status,
            description,
            occurredAt);
    }

    private static decimal ReadDecimal(JsonElement parent, string propertyName)
    {
        var value = ReadRequiredString(parent, propertyName);
        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var result)
                ? result
                : throw new ExternalServiceException(
                    Provider,
                    $"'{propertyName}' was not a valid amount.");
    }

    private static string ReadRequiredString(JsonElement parent, string propertyName) =>
        ReadString(parent, propertyName) ?? throw new ExternalServiceException(
            Provider,
            $"The response did not contain '{propertyName}'.");

    private static string? ReadString(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value)
            ? value.GetString()
            : null;

    private sealed record PayPalCapture(
        string Id,
        decimal Amount,
        decimal Fee,
        string Currency,
        string Status,
        string Description,
        DateTimeOffset OccurredAt);
}
