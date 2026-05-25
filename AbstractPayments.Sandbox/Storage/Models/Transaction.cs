namespace AbstractPayments.Sandbox.Storage.Models;

using System;

/// <summary>
/// Pix Cob (Immediate Charge) Status.
/// </summary>
public enum ETransactionStatus
{
    Pending,
    Paid,
    Cancelled,
    Refunded
}

/// <summary>
/// Data model representing a physical transaction saved in the database.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets or sets the unique Transaction identifier (UUID).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated Pix payment QR string.
    /// </summary>
    public string PaymentString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the payment.
    /// </summary>
    public ETransactionStatus Status { get; set; } = ETransactionStatus.Pending;

    /// <summary>
    /// Gets or sets when the payment transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the BACEN unique EndToEndId identifier.
    /// </summary>
    public string? EndToEndId { get; set; }
}
