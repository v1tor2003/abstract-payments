namespace AbstractPayments.Core.Extensions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Root options pattern model for configuring the AbstractPayments framework.
/// </summary>
public class PaymentFrameworkOptions : IValidatableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether webhook ingestion and background processing are enabled.
    /// </summary>
    public bool EnableWebhooks { get; set; }

    /// <summary>
    /// Gets or sets the webhook security/signature validation key.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Validates the consistency of configured options.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection of validation errors, if any.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EnableWebhooks && string.IsNullOrWhiteSpace(WebhookSecret))
        {
            yield return new ValidationResult(
                "WebhookSecret must be provided when EnableWebhooks is enabled.",
                new[] { nameof(WebhookSecret) });
        }
    }
}
