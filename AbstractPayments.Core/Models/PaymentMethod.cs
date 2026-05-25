namespace AbstractPayments.Core.Models;

/// <summary>
/// Supported payment method capabilities inside the framework.
/// Meaning all listed methods here have their contracts defined
/// Per Payments Module and Webhook Handler plugins
/// </summary>
public enum PaymentMethod
{
    Pix,
    CreditCard,
    BankSlip
}
