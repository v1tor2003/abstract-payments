namespace AbstractPayments.Tests.Sandbox.Payments;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Coupled;
using AbstractPayments.Sandbox.Http.Commands;
using AbstractPayments.Sandbox.Requests;
using AbstractPayments.Sandbox.Responses;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// E2E Integration tests verifying coupled vs abstracted Minimal API payment endpoints.
/// </summary>
[Collection("Sandbox Tests")]
public class SandboxPaymentIntegrationTests : IClassFixture<SandboxTestApplicationFactory>
{
    private readonly SandboxTestApplicationFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxPaymentIntegrationTests"/> class.
    /// </summary>
    public SandboxPaymentIntegrationTests(SandboxTestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Abstracted_Pix_Route_Should_Return_BadRequest_When_Provider_Is_Not_Registered()
    {
        // Arrange
        var request = new AbstractedPixRequest(100.00m, "unregistered_stripe");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/api/payments/pix", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Provider Not Registered", problem.Title);
        Assert.Contains("unregistered_stripe", problem.Detail);
    }

    [Fact]
    public async Task GET_Endpoints_Should_Return_List_And_Specific_Transactions()
    {
        // Arrange
        var txId = Guid.NewGuid().ToString();
        var seededTx = new Transaction
        {
            Id = txId,
            Amount = 99.99m,
            Provider = "inline_seeded_provider",
            PaymentString = "inline_seeded_qrcode",
            Status = ETransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        using (var conn = connectionFactory.CreateConnection())
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt)
                VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt);",
                seededTx);
        }

        // Act & Assert 1: GET all transactions
        var listResponse = await _client.GetAsync("/v1/api/payments/pix");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var list = await listResponse.Content.ReadFromJsonAsync<List<Transaction>>();
        Assert.NotNull(list);
        Assert.Contains(list, t => t.Id == txId && t.Provider == "inline_seeded_provider");

        // Act & Assert 2: GET specific transaction by ID
        var singleResponse = await _client.GetAsync($"/v1/api/payments/pix/{txId}");
        Assert.Equal(HttpStatusCode.OK, singleResponse.StatusCode);

        var singleTx = await singleResponse.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(singleTx);
        Assert.Equal(txId, singleTx.Id);
        Assert.Equal(99.99m, singleTx.Amount);
    }

    [Fact]
    public async Task Coupled_And_Abstracted_MercadoPago_Pix_Creation_Should_Succeed()
    {
        // Arrange Mock API Response
        _factory.MockHttpHandler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Contains("/v1/payments", req.RequestUri.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""id"": 123456789,
                    ""status"": ""pending"",
                    ""point_of_interaction"": {
                        ""transaction_data"": {
                            ""qr_code"": ""mercadopago_emv_copy_paste"",
                            ""qr_code_base64"": ""mercadopago_base64_image""
                        }
                    }
                }")
            };
        };

        // Coupled Call
        var coupledReq = new CoupledPixRequest(350.00m, "mercadopago");
        var coupledRes = await _client.PostAsJsonAsync("/v1/api/coupled/payments/pix", coupledReq);
        Assert.Equal(HttpStatusCode.OK, coupledRes.StatusCode);
        var mpCoupledData = await coupledRes.Content.ReadFromJsonAsync<MercadoPagoPixResponse>();
        Assert.NotNull(mpCoupledData);
        Assert.Equal(123456789, mpCoupledData.Id);

        // Abstracted Call
        var abstractedReq = new AbstractedPixRequest(350.00m, "mercadopago");
        var abstractedRes = await _client.PostAsJsonAsync("/v1/api/payments/pix", abstractedReq);
        Assert.Equal(HttpStatusCode.OK, abstractedRes.StatusCode);
        var mpAbstractedData = await abstractedRes.Content.ReadFromJsonAsync<AbstractedPixResponse>();
        Assert.NotNull(mpAbstractedData);
        Assert.Equal("mercadopago_emv_copy_paste", mpAbstractedData.PaymentString);
    }

    [Fact]
    public async Task Coupled_And_Abstracted_PagSeguro_Pix_Creation_Should_Succeed()
    {
        // Arrange Mock API Response
        _factory.MockHttpHandler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Contains("/orders", req.RequestUri.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""id"": ""ps_order_1111"",
                    ""reference_id"": ""ps_ref_999"",
                    ""qr_codes"": [
                        {
                            ""id"": ""ps_qr_999"",
                            ""text"": ""pagseguro_emv_copy_paste"",
                            ""link"": ""pagseguro_image_url""
                        }
                    ]
                }")
            };
        };

        // Coupled Call
        var coupledReq = new CoupledPixRequest(220.00m, "pagseguro");
        var coupledRes = await _client.PostAsJsonAsync("/v1/api/coupled/payments/pix", coupledReq);
        Assert.Equal(HttpStatusCode.OK, coupledRes.StatusCode);
        var psCoupledData = await coupledRes.Content.ReadFromJsonAsync<PagSeguroPixResponse>();
        Assert.NotNull(psCoupledData);
        Assert.Equal("ps_ref_999", psCoupledData.ReferenceId);

        // Abstracted Call
        var abstractedReq = new AbstractedPixRequest(220.00m, "pagseguro");
        var abstractedRes = await _client.PostAsJsonAsync("/v1/api/payments/pix", abstractedReq);
        Assert.Equal(HttpStatusCode.OK, abstractedRes.StatusCode);
        var psAbstractedData = await abstractedRes.Content.ReadFromJsonAsync<AbstractedPixResponse>();
        Assert.NotNull(psAbstractedData);
        Assert.Equal("pagseguro_emv_copy_paste", psAbstractedData.PaymentString);
    }

    [Fact]
    public async Task Coupled_And_Abstracted_EfiBank_Pix_Creation_Should_Succeed()
    {
        // Arrange Mock API Response
        _factory.MockHttpHandler.Handler = req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Contains("/v2/cob", req.RequestUri.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""txid"": ""efibank_txid_7777"",
                    ""pixCopiaECola"": ""efibank_emv_copy_paste"",
                    ""status"": ""ATIVA""
                }")
            };
        };

        // Coupled Call
        var coupledReq = new CoupledPixRequest(880.00m, "efibank");
        var coupledRes = await _client.PostAsJsonAsync("/v1/api/coupled/payments/pix", coupledReq);
        Assert.Equal(HttpStatusCode.OK, coupledRes.StatusCode);
        var efiCoupledData = await coupledRes.Content.ReadFromJsonAsync<EfiBankPixResponse>();
        Assert.NotNull(efiCoupledData);
        Assert.Equal("efibank_txid_7777", efiCoupledData.Txid);

        // Abstracted Call
        var abstractedReq = new AbstractedPixRequest(880.00m, "efibank");
        var abstractedRes = await _client.PostAsJsonAsync("/v1/api/payments/pix", abstractedReq);
        Assert.Equal(HttpStatusCode.OK, abstractedRes.StatusCode);
        var efiAbstractedData = await abstractedRes.Content.ReadFromJsonAsync<AbstractedPixResponse>();
        Assert.NotNull(efiAbstractedData);
        Assert.Equal("efibank_emv_copy_paste", efiAbstractedData.PaymentString);
    }
}
