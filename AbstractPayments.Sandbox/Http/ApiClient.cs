using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AbstractPayments.Sandbox.Http;

/// <summary>
/// Generalized client that executes HTTP commands against API gateways.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The underlying HttpClient.</param>
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sends the API command and returns the deserialized response object.
    /// </summary>
    public async Task<TResponse?> SendAsync<TResponse>(
        ApiCommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(command.Method, command.Endpoint)
        {
            Content = command.CreateContent()
        };

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }
}
