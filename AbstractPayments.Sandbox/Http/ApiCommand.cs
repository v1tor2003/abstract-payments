using System;
using System.Net.Http;
using System.Net.Http.Json;

namespace AbstractPayments.Sandbox.Http;

/// <summary>
/// Base command for HTTP requests without a request body.
/// </summary>
/// <typeparam name="TResponse">The strongly-typed response type.</typeparam>
public abstract class ApiCommand<TResponse>
{
    /// <summary>
    /// Gets the HTTP method.
    /// </summary>
    public abstract HttpMethod Method { get; }

    /// <summary>
    /// Gets the target endpoint URI.
    /// </summary>
    public abstract string Endpoint { get; }

    /// <summary>
    /// Creates the HttpContent representation of the request.
    /// </summary>
    public virtual HttpContent? CreateContent() => null;
}

/// <summary>
/// Base command for HTTP requests containing a request body.
/// </summary>
/// <typeparam name="TPayload">The strongly-typed request payload type.</typeparam>
/// <typeparam name="TResponse">The strongly-typed response type.</typeparam>
public abstract class ApiCommand<TPayload, TResponse> : ApiCommand<TResponse>
{
    /// <summary>
    /// Gets the request payload.
    /// </summary>
    public TPayload Payload { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiCommand{TPayload, TResponse}"/> class.
    /// </summary>
    protected ApiCommand(TPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        Payload = payload;
    }

    /// <inheritdoc />
    public override HttpContent? CreateContent()
    {
        return JsonContent.Create(Payload);
    }
}
