# Golden Examples: Resilient Integration Testing

Use these before-and-after structures to guide your containerized and wire-mocked integration test generation.

## 1. Checkout Workflow Testing

### Anti-Pattern (Non-Deterministic, Shared DB, Real Network Calls)


```javascript
class BrokenCartIntegrationTests {
    async test_add_item_to_cart() {
        // Violates Isolation: Hardcoded ID assumes this is the first row ever made
        const userId = 1;

        // Flaky: Hits a real external identity provider over the web
        const token = await ExternalAuthSDK.login("real_user", "password");
    
        const client = new HttpClient(token);
        const response = await client.post("/api/cart/add", { "item_id": 99 });

        // Flaky: If another test ran before this, total items won't be 1
        assert.equal(response.json().total_items, 1);
    }
}
```

### Refactored Solution (Idempotent, Ephemeral, and Fully Isolated)
```javascript
class EphemeralCheckoutIntegrationTests {
    private static databaseContainer;
    private static networkMocker;
    private static testServer;
    private databaseFixture;

    async setUpSuite() {
        // 1. Initialize an Ephemeral Database via Testcontainers (Dynamic Ports)
        this.databaseContainer = await new PostgresContainer("postgres:16-alpine")
           .withDatabaseName("test_marketplace")
           .withUsername("sandbox_user")
           .withPassword("sandbox_pass")
           .withReadinessStrategy(Wait.forListeningPort())
           .start();

        // 2. Initialize the Network HTTP Mocker for External APIs
        this.networkMocker = await new WireMockServer().startOnRandomPort();

        // 3. Inject Dynamic Properties into the Application Context Root
        const runtimeDatabaseUrl = this.databaseContainer.getJdbcUrl();
        const runtimePaymentGatewayUrl = this.networkMocker.getBaseUrl();

        // Bypass OAuth via local JWT validation stub configuration
        this.testServer = await ApplicationCompositionRoot.startTestingServer({
            "database.url": runtimeDatabaseUrl,
            "payment.gateway.url": runtimePaymentGatewayUrl,
            "auth.mode": "TESTING_STUB_VAL_ENABLED"
        });

        this.databaseFixture = new TestDatabaseContext(runtimeDatabaseUrl);
    }

    async tearDownTestCase() {
        // Guarantee clean, idempotent state boundaries between individual test runs
        await this.networkMocker.resetAllMappings();
        await this.databaseFixture.truncateTables(["orders", "users", "products"]);
    }

    async tearDownSuite() {
        // Terminate resource contexts gracefully upon suite completion
        await this.testServer.stop();
        await this.networkMocker.stop();
        await this.databaseContainer.stop();
    }

    async test_should_successfully_complete_checkout_when_payment_gateway_approves() {
        // 1. ARRANGE
        // Use randomized unique identifiers to completely eliminate state collisions
        const testUserId = UUID.generate();
        const testOrderId = UUID.generate();
        const testProductId = UUID.generate();
        
        // Localized Idempotent Seed
        await this.databaseFixture.execute("INSERT INTO users (id, name) VALUES (?, 'Vitor')", [testUserId]);
        await this.databaseFixture.execute("INSERT INTO products (id, price) VALUES (?, 150.00)", [testProductId]);
        await this.databaseFixture.execute(
            "INSERT INTO orders (id, user_id, amount, status) VALUES (?,?, 150.00, 'PENDING')", 
            [testOrderId, testUserId]
        );

        // Program the Network Mocker with a canned wire response
        await this.networkMocker.stubFor(
            Request.post("/v1/charges")
               .withHeader("Content-Type", "application/json")
               .withJsonBody({ "amount": 150.00, "reference": testOrderId.toString() })
               .willReturn(
                    Response.status(200)
                       .withJsonBody({ "charge_id": "ch_mock_9921", "status": "succeeded" })
                )
        );

        // Generate local mock JWT token bypass payload
        const mockAuthToken = TestTokenGenerator.buildMockJwtString({ 
            "sub": testUserId.toString(),
            "roles": 
        });

        // 2. ACT
        const response = await this.testServer.httpClient()
           .withHeader("Authorization", "Bearer " + mockAuthToken)
           .post("/api/checkout", { "order_id": testOrderId.toString() });

        // 3. ASSERT
        assert.equal(response.statusCode, 200);
        assert.equal(response.json().transaction_status, "COMPLETED");

        // Assert Database Mutation directly inside the real Testcontainers Instance
        const orderRow = await this.databaseFixture.querySingle("SELECT * FROM orders WHERE id =?", [testOrderId]);
        assert.equal(orderRow.status, "PAID");

        // Assert Network Outbound Footprint (Verifying wire telemetry)
        const interceptedRequests = await this.networkMocker.findAllRequests(Request.post("/v1/charges"));
        assert.equal(interceptedRequests.length, 1);
    }
}
```