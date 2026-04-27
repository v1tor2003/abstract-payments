# AbstractPayments.Sandbox

## 1. Introduction

The **AbstractPayments.Sandbox** project is a playground for testing and demonstrating the capabilities of the **AbstractPayments** framework. It serves as a reference implementation for developers looking to integrate the framework into their own applications.

This sandbox demonstrates how to:
*   Extend the framework with custom gateway implementations.
*   Process webhooks using a customized logic.
*   Orchestrate payments across multiple providers.

---

## 2. Framework Integration

To start using the framework, you need to register it in your `Program.cs`. This involves configuring global options, webhook strategies, and registering your gateways.

```csharp
// Program.cs
builder.Services.AddAbstractPayments(options =>
{
    options.EnableWebhooks = true;
    
    // Configure webhook signature validation
    options.Webhooks.UseSignatureValidator<MyCustomSignatureValidator>();

    // Register a custom gateway implementation
    options.AddGateway<IPixPaymentGateway, MyCustomPixGateway>("custom-provider", cfg => {
        // Custom configuration logic
    });
});
```

---

## 3. Implementing Custom Logic

### 3.1 Custom Gateway Implementation
When adding support for a new payment provider, you implement the corresponding module interface.

```csharp
public class MyCustomPixGateway : IPixPaymentGateway
{
    public string Name => "custom-provider";

    public bool Supports(PaymentMethod method) => method == PaymentMethod.Pix;

    public async Task<PixPaymentResult> GenerateAsync(PixPaymentRequest request)
    {
        // 1. Map request to provider-specific SDK/API
        // 2. Call the provider
        // 3. Map provider response back to PixPaymentResult
        
        return new PixPaymentResult {
            Success = true,
            ExternalId = "prov_12345",
            QrCode = "000201..."
        };
    }

    // Implement other methods: GetStatusAsync, RefundAsync...
}
```

### 3.2 Custom Webhook Processor
The `BaseWebhookProcessor` handles the boilerplate (validation and parsing). You only need to implement the `HandleAsync` method to define what happens when a valid event arrives.

```csharp
public class MyBusinessWebhookProcessor : BaseWebhookProcessor
{
    private readonly ILogger<MyBusinessWebhookProcessor> _logger;

    public MyBusinessWebhookProcessor(
        IWebhookSignatureValidator validator, 
        IWebhookEventParserStrategy parserStrategy,
        ILogger<MyBusinessWebhookProcessor> logger) 
        : base(validator, parserStrategy)
    {
        _logger = logger;
    }

    protected override async Task HandleAsync(WebhookEvent evt)
    {
        _logger.LogInformation("Processing event {EventId} for provider {Provider}", 
            evt.Id, evt.ProviderName);

        // Custom Logic:
        // - Update order status in your database
        // - Send email notifications
        // - Trigger ledger updates
    }
}
```

---

## 4. Webhook Endpoint

Exposing a webhook endpoint is straightforward. You inject the `IWebhookProcessor` and delegate the incoming request to it.

```csharp
app.MapPost("/api/webhooks/{provider}", async (
    string provider,
    HttpRequest httpRequest,
    IWebhookProcessor processor) =>
{
    try 
    {
        // Wrap the raw HTTP request into the framework's WebhookRequest
        var request = new WebhookRequest {
            ProviderName = provider,
            Headers = httpRequest.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Body = await new StreamReader(httpRequest.Body).ReadToEndAsync()
        };

        await processor.ProcessAsync(request);
        
        return Results.Ok();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});
```

---

## 5. Running the Sandbox

1.  Clone the repository.
2.  Navigate to the sandbox directory:
    ```bash
    cd AbstractPayments.Sandbox
    ```
3.  Run the application:
    ```bash
    dotnet run
    ```
4.  The API will be available at `http://localhost:5000` (or the configured port). You can use the Swagger UI (if enabled) or tools like Postman to test the endpoints.
