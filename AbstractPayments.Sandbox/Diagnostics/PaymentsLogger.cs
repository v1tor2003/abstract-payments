namespace AbstractPayments.Sandbox.Diagnostics;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Source-generated, compile-time performance logging methods for payment pathways.
/// </summary>
public static partial class PaymentsLogger
{
    /// <summary>
    /// Logs when a Pix payment starts processing.
    /// </summary>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Initializing Pix payment generation. Amount: {Amount}, Provider: {Provider}")]
    public static partial void LogPaymentInitiated(this ILogger logger, decimal amount, string provider);

    /// <summary>
    /// Logs when a Pix payment is successfully created and persisted.
    /// </summary>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Pix payment successfully generated. Transaction ID: {TransactionId}, PaymentString: {PaymentString}")]
    public static partial void LogPaymentGenerated(this ILogger logger, string transactionId, string paymentString);

    /// <summary>
    /// Logs when a Pix payment fails.
    /// </summary>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Payment generation failed for Provider: {Provider}. Reason: {Error}")]
    public static partial void LogPaymentFailed(this ILogger logger, string provider, string error, Exception? exception = null);
}
