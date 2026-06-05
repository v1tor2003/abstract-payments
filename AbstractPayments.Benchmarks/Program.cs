namespace AbstractPayments.Benchmarks;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Extensions;
using AbstractPayments.Core.Models.Webhooks;
using AbstractPayments.Sandbox.Gateways.Webhooks;
using AbstractPayments.Sandbox.Services;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// In-memory transaction repository mock for benchmark isolation.
/// Avoids I/O overhead so measurements reflect queue throughput only.
/// </summary>
internal sealed class MockTransactionRepository : ITransactionRepository
{
    public ConcurrentDictionary<string, Transaction> Transactions { get; } = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task InsertAsync(Transaction transaction)
    {
        Transactions[transaction.Id] = transaction;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Transaction>> GetAllAsync()
        => Task.FromResult<IEnumerable<Transaction>>(Transactions.Values);

    public Task<Transaction?> GetByIdAsync(string id)
    {
        Transactions.TryGetValue(id, out var tx);
        return Task.FromResult(tx);
    }

    public Task UpdateAsync(Transaction transaction)
    {
        Transactions[transaction.Id] = transaction;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Entry point for the AbstractPayments webhook ingestion load benchmarks.
/// Measures both ingestion (enqueueing) and processing (dequeue + handle) throughput
/// at 1 k, 5 k, 10 k, and 100 k events over three iterations each, reporting averages.
/// </summary>
internal sealed class Program
{
    /// <summary>Load sizes (event counts) to benchmark.</summary>
    private static readonly int[] Loads = [1_000, 5_000, 10_000, 100_000];

    /// <summary>Number of warm-up + measurement iterations per load level.</summary>
    private const int Iterations = 3;

    public static async Task Main(string[] _)
    {
        PrintHeader();

        Console.WriteLine(
            "| Load Size | Iteration | Ingestion (ms) | Ingestion RPS | Processing (ms) | Processing RPS | Total (ms) |");
        Console.WriteLine(
            "|-----------|-----------|----------------|---------------|-----------------|----------------|------------|");

        foreach (var load in Loads)
        {
            await RunLoadLevelAsync(load);
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static void PrintHeader()
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine("  AbstractPayments — Webhook Queue Load Benchmark");
        Console.WriteLine("=================================================================");
        Console.WriteLine($"  OS            : {Environment.OSVersion}");
        Console.WriteLine($"  Processors    : {Environment.ProcessorCount}");
        Console.WriteLine($"  .NET Runtime  : {Environment.Version}");
        Console.WriteLine($"  Date (UTC)    : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("=================================================================");
        Console.WriteLine();
    }

    private static async Task RunLoadLevelAsync(int load)
    {
        double sumIngestionMs = 0;
        double sumProcessingMs = 0;
        double sumTotalMs = 0;

        for (int iter = 1; iter <= Iterations; iter++)
        {
            var (ingestionMs, processingMs) = await RunSingleIterationAsync(load);
            double totalMs = ingestionMs + processingMs;

            sumIngestionMs += ingestionMs;
            sumProcessingMs += processingMs;
            sumTotalMs += totalMs;

            double ingestionRps = load / (ingestionMs / 1_000.0);
            double processingRps = load / (processingMs / 1_000.0);

            Console.WriteLine(
                $"| {load,9} | {iter,9} | {ingestionMs,14:F2} | {ingestionRps,13:F0} | {processingMs,15:F2} | {processingRps,14:F0} | {totalMs,10:F2} |");
        }

        double avgIngestionMs = sumIngestionMs / Iterations;
        double avgProcessingMs = sumProcessingMs / Iterations;
        double avgTotalMs = sumTotalMs / Iterations;

        double avgIngestionRps = load / (avgIngestionMs / 1_000.0);
        double avgProcessingRps = load / (avgProcessingMs / 1_000.0);

        Console.WriteLine(
            $"| **AVG {load}** | **-** | **{avgIngestionMs:F2}** | **{avgIngestionRps:F0}** | **{avgProcessingMs:F2}** | **{avgProcessingRps:F0}** | **{avgTotalMs:F2}** |");
        Console.WriteLine(
            "|-----------|-----------|----------------|---------------|-----------------|----------------|------------|");
    }

    private static async Task<(double IngestionMs, double ProcessingMs)> RunSingleIterationAsync(int load)
    {
        // ── DI container (fresh per iteration for clean state) ──────────────
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));

        services.AddAbstractPayments()
            .AddEventsModule(events =>
            {
                events.IngestionEndpoint = "/v1/api/payments/webhook";
                events.ListenFrom("mercadopago", opts => opts
                    .UseSignatureValidator<MercadoPagoSignatureValidator>()
                    .UseConverter<MercadoPagoEventConverter>()
                    .UseHandler<MercadoPagoEventHandler>());
            });

        var queue = new InMemoryWebhookQueue();
        services.AddSingleton<InMemoryWebhookQueue>(queue);
        services.AddSingleton<IWebhookQueue>(queue);

        var repo = new MockTransactionRepository();
        services.AddSingleton<ITransactionRepository>(repo);

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IWebhookProcessor>();

        // ── Pre-seed repository with pending transactions ────────────────────
        var transactionIds = new string[load];
        for (int i = 0; i < load; i++)
        {
            var txId = Guid.NewGuid().ToString();
            transactionIds[i] = txId;
            await repo.InsertAsync(new Transaction
            {
                Id = txId,
                Amount = 100m,
                Provider = "mercadopago",
                PaymentString = "qr-code-payload",
                Status = ETransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }

        // ── Prepare WebhookContext payloads ──────────────────────────────────
        var contexts = new WebhookContext[load];
        for (int i = 0; i < load; i++)
        {
            var body = $"{{\"action\":\"payment.updated\",\"data\":{{\"id\":\"{transactionIds[i]}\"}},\"status\":\"approved\"}}";
            var headers = new Dictionary<string, string> { { "X-Signature", "mercadopago_secret" } };
            contexts[i] = new WebhookContext("mercadopago", body, headers);
        }

        // ── Phase 1: Concurrent ingestion (enqueue) ──────────────────────────
        var ingestionSw = Stopwatch.StartNew();
        var ingestionTasks = new Task[load];
        for (int i = 0; i < load; i++)
        {
            var ctx = contexts[i];
            ingestionTasks[i] = processor.ProcessAsync(ctx);
        }
        await Task.WhenAll(ingestionTasks);
        ingestionSw.Stop();

        // ── Phase 2: Sequential processing (dequeue + handle) ───────────────
        using var cts = new CancellationTokenSource();
        var processingSw = Stopwatch.StartNew();

        var processingTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var @event = await queue.DequeueAsync(cts.Token);
                    using var scope = provider.CreateScope();
                    var handler = scope.ServiceProvider
                        .GetKeyedService<IWebhookEventHandler>($"handler:{@event.Provider}");
                    if (handler is not null)
                        await handler.HandleAsync(@event);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        });

        // Wait until every transaction has been marked as Paid
        while (true)
        {
            int paidCount = repo.Transactions.Values.Count(t => t.Status == ETransactionStatus.Paid);
            if (paidCount >= load) break;
            await Task.Delay(10);
        }

        processingSw.Stop();
        cts.Cancel();
        try { await processingTask; } catch { /* OperationCanceledException expected */ }

        return (ingestionSw.Elapsed.TotalMilliseconds, processingSw.Elapsed.TotalMilliseconds);
    }
}
