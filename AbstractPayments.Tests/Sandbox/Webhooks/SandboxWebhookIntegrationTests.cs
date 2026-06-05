namespace AbstractPayments.Tests.Sandbox.Webhooks;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// E2E Integration tests verifying webhook endpoints for all payment gateway strategies.
/// </summary>
[Collection("Sandbox Tests")]
public class SandboxWebhookIntegrationTests : IClassFixture<SandboxTestApplicationFactory>
{
    private readonly SandboxTestApplicationFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxWebhookIntegrationTests"/> class.
    /// </summary>
    public SandboxWebhookIntegrationTests(SandboxTestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Webhooks_For_All_Providers_Should_Update_Status_In_Coupled_And_Abstracted_Approaches()
    {
        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        
        // Providers list to test
        var providers = new[] { "mercadopago", "pagseguro", "efibank" };

        foreach (var provider in providers)
        {
            // 1. Coupled Webhook Test
            var coupledTxId = Guid.NewGuid().ToString();
            var coupledTx = new Transaction
            {
                Id = coupledTxId,
                Amount = 100m,
                Provider = provider,
                PaymentString = "coupled-payload",
                Status = ETransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            using (var conn = connectionFactory.CreateConnection())
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt)
                    VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt);",
                    coupledTx);
            }

            HttpRequestMessage coupledMsg;
            if (provider == "mercadopago")
            {
                coupledMsg = new HttpRequestMessage(HttpMethod.Post, $"/v1/api/coupled/payments/webhook/{provider}")
                {
                    Content = JsonContent.Create(new { action = "payment.updated", data = new { id = coupledTxId }, status = "approved" })
                };
                coupledMsg.Headers.Add("X-Signature", "mercadopago_coupled_secret");
            }
            else if (provider == "pagseguro")
            {
                coupledMsg = new HttpRequestMessage(HttpMethod.Post, $"/v1/api/coupled/payments/webhook/{provider}")
                {
                    Content = JsonContent.Create(new { reference_id = coupledTxId, status = "PAID" })
                };
                coupledMsg.Headers.Add("X-Signature", "pagseguro_coupled_secret");
            }
            else // efibank
            {
                coupledMsg = new HttpRequestMessage(HttpMethod.Post, $"/v1/api/coupled/payments/webhook/{provider}")
                {
                    Content = JsonContent.Create(new { pix = new[] { new { txid = coupledTxId, endToEndId = "efi_e2e_123" } } })
                };
                coupledMsg.Headers.Add("X-Signature", "efibank_coupled_secret");
            }

            var coupledRes = await _client.SendAsync(coupledMsg);
            Assert.Equal(HttpStatusCode.NoContent, coupledRes.StatusCode);

            using (var conn = connectionFactory.CreateConnection())
            {
                var tx = await conn.QueryFirstOrDefaultAsync<Transaction>("SELECT * FROM Transactions WHERE Id = @Id;", new { Id = coupledTxId });
                Assert.NotNull(tx);
                Assert.Equal(ETransactionStatus.Paid, tx.Status);
                if (provider == "efibank")
                {
                    Assert.Equal("efi_e2e_123", tx.EndToEndId);
                }
            }

            // 2. Abstracted Webhook Test
            var abstractedTxId = Guid.NewGuid().ToString();
            var abstractedTx = new Transaction
            {
                Id = abstractedTxId,
                Amount = 200m,
                Provider = provider,
                PaymentString = "abstracted-payload",
                Status = ETransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            using (var conn = connectionFactory.CreateConnection())
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt)
                    VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt);",
                    abstractedTx);
            }

            HttpRequestMessage abstractedMsg;
            if (provider == "mercadopago")
            {
                abstractedMsg = new HttpRequestMessage(HttpMethod.Post, $"/v1/api/payments/webhook/{provider}")
                {
                    Content = JsonContent.Create(new { action = "payment.updated", data = new { id = abstractedTxId }, status = "approved" })
                };
                abstractedMsg.Headers.Add("X-Signature", "mercadopago_secret");
            }
            else if (provider == "pagseguro")
            {
                abstractedMsg = new HttpRequestMessage(HttpMethod.Post, $"/v1/api/payments/webhook/{provider}")
                {
                    Content = JsonContent.Create(new { reference_id = abstractedTxId, status = "PAID" })
                };
                abstractedMsg.Headers.Add("X-Signature", "pagseguro_secret");
            }
            else // efibank
            {
                abstractedMsg = new HttpRequestMessage(HttpMethod.Post, $"/v1/api/payments/webhook/{provider}")
                {
                    Content = JsonContent.Create(new { pix = new[] { new { txid = abstractedTxId, endToEndId = "efi_abstracted_e2e" } } })
                };
                abstractedMsg.Headers.Add("X-Signature", "efibank_secret");
            }

            var abstractedRes = await _client.SendAsync(abstractedMsg);
            Assert.Equal(HttpStatusCode.NoContent, abstractedRes.StatusCode);

            Transaction pollingTx = null;
            for (int i = 0; i < 20; i++)
            {
                using (var conn = connectionFactory.CreateConnection())
                {
                    pollingTx = await conn.QueryFirstOrDefaultAsync<Transaction>("SELECT * FROM Transactions WHERE Id = @Id;", new { Id = abstractedTxId });
                }
                if (pollingTx != null && pollingTx.Status == ETransactionStatus.Paid)
                {
                    break;
                }
                await Task.Delay(50);
            }

            Assert.NotNull(pollingTx);
            Assert.Equal(ETransactionStatus.Paid, pollingTx.Status);
            if (provider == "efibank")
            {
                Assert.Equal("efi_abstracted_e2e", pollingTx.EndToEndId);
            }
        }
    }

    [Fact]
    public async Task Webhooks_Under_Concurrent_Load_Should_All_Process_Successfully()
    {
        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        int concurrentRequests = 50;
        var tasks = new Task<HttpResponseMessage>[concurrentRequests];
        var transactionIds = new string[concurrentRequests];

        // 1. Arrange: Insert transactions sequentially to avoid SQLite write locks during initialization
        using (var conn = connectionFactory.CreateConnection())
        {
            for (int i = 0; i < concurrentRequests; i++)
            {
                var txId = Guid.NewGuid().ToString();
                transactionIds[i] = txId;
                var tx = new Transaction
                {
                    Id = txId,
                    Amount = 10.0m + i,
                    Provider = "mercadopago",
                    PaymentString = $"load-payload-{i}",
                    Status = ETransactionStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await conn.ExecuteAsync(@"
                    INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt)
                    VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt);",
                    tx);
            }
        }

        // 2. Act: Trigger concurrent HTTP webhook POST requests to test ingestion and queue behavior
        for (int i = 0; i < concurrentRequests; i++)
        {
            var txId = transactionIds[i];
            var msg = new HttpRequestMessage(HttpMethod.Post, "/v1/api/payments/webhook/mercadopago")
            {
                Content = JsonContent.Create(new { action = "payment.updated", data = new { id = txId }, status = "approved" })
            };
            msg.Headers.Add("X-Signature", "mercadopago_secret");
            tasks[i] = _client.SendAsync(msg);
        }

        var responses = await Task.WhenAll(tasks);

        // 3. Assert: All HTTP ingestion requests must return 204 NoContent immediately
        foreach (var res in responses)
        {
            Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        }

        // 4. Polling Assert: Wait for the background worker queue to serialize updates to Paid in SQLite
        int pendingCount = concurrentRequests;
        for (int attempt = 0; attempt < 60; attempt++)
        {
            using (var conn = connectionFactory.CreateConnection())
            {
                var pendingTxIds = await conn.QueryAsync<string>(
                    "SELECT Id FROM Transactions WHERE Id IN @Ids AND Status = @Status;",
                    new { Ids = transactionIds, Status = ETransactionStatus.Pending });
                pendingCount = pendingTxIds.Count();
            }

            if (pendingCount == 0)
            {
                break;
            }
            await Task.Delay(100);
        }

        Assert.Equal(0, pendingCount);
    }
}
