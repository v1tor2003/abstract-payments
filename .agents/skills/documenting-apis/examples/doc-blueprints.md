# Golden Examples: Internal & Public Documentation Blueprints

## 1. JavaScript/TypeScript
### 1.1 Documented Use Case (Internal)

```javascript
/**
 * USE CASE: ProcessDroneTelemetryCommand
 * Layer: Application (Layer 2)
 *
 * DESIGN INTENT:
 * Coordinates incoming data packets from operating flight units. Validates legal airspace 
 * constraints by making a synchronous inbound port call, updates telemetry metrics, and 
 * fires a decoupled domain event upon detecting speed/boundary violations.
 *
 * SECURITY: Requires internal system-level token or trusted ingestion gateway claims.
 * TRANSACTION: Executed within a read-committed snapshot boundary to prevent race conditions on fast updates.
 */
class ProcessDroneTelemetryUseCase {
    
    /**
     * Executes the telemetry ingestion pipeline.
     * 
     * @param {TelemetryInputDTO} input - Flattened Input DTO containing sanitized geographic coordinates and speed indicators.
     * @returns {TelemetryOutputDTO} - Deterministic Output DTO containing audit confirmation IDs and status alerts.
     * 
     * @throws {TelemetryValidationException} - Encountered when structural check fails on packet payload.
     * @throws {AirspaceViolationException} - Encountered when localized business logic flags non-flight zones.
     */
    execute(input) {
        // Implementation logic details...
    }
}

```

### 1.2 Swagger / OpenAPI Controller (Public)

```javascript
/**
 * @openapi
 * /api/v1/drones/telemetry:
 *   post:
 *     summary: Ingest operating flight unit telemetry
 *     description: Accepts and processes real-time telemetry coordinates. Triggers airspace validations.
 *     tags:
 *       - Drone Management
 *     security:
 *       - BearerAuth:
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required: [droneId, latitude, longitude, speedKnots]
 *             properties:
 *               droneId:
 *                 type: string
 *                 format: uuid
 *                 example: "123e4567-e89b-12d3-a456-426614174000"
 *               latitude:
 *                 type: number
 *                 minimum: -90
 *                 maximum: 90
 *                 example: 37.7749
 *               longitude:
 *                 type: number
 *                 minimum: -180
 *                 maximum: 180
 *                 example: -122.4194
 *               speedKnots:
 *                 type: number
 *                 minimum: 0
 *                 example: 45.20
 *     responses:
 *       200:
 *         description: Telemetry logged successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 auditLogId:
 *                   type: string
 *                   format: uuid
 *                   example: "987f6543-e21b-34c5-d678-109923812000"
 *                 flightStatus:
 *                   type: string
 *                   enum:
 *                   example: "CLEAR"
 *       401:
 *         description: Missing or expired bearer token
 *       422:
 *         description: Spatial violation encountered (e.g. No-Fly Zone)
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/ProblemDetails'
 */

```

---

## 2. C#/.NET Projects
### 2.1. Documented Use Case (Internal C# Transcription)

In the C# ecosystem, internal documentation of architectural components is handled using **XML Documentation Comments (`///`)** and structured `<remarks>` blocks. This is a clean translation of the domain-level design intent:

```csharp
namespace Ingestion.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// <strong>USE CASE: ProcessDroneTelemetryCommand</strong><br/>
/// <strong>Layer:</strong> Application (Layer 2)
/// </summary>
/// <remarks>
/// <para>
/// <strong>DESIGN INTENT:</strong><br/>
/// Coordinates incoming data packets from operating flight units. Validates legal airspace 
/// constraints by making a synchronous inbound port call, updates telemetry metrics, and 
/// fires a decoupled domain event upon detecting speed/boundary violations.
/// </para>
/// <para>
/// <strong>SECURITY:</strong> Requires internal system-level token or trusted ingestion gateway claims.
/// </para>
/// <para>
/// <strong>TRANSACTION:</strong> Executed within a read-committed snapshot boundary to prevent race conditions on fast updates.
/// </para>
/// </remarks>
public sealed class ProcessDroneTelemetryUseCase
{
    /// <summary>
    /// Executes the telemetry ingestion pipeline.
    /// </summary>
    /// <param name="input">Flattened Input DTO containing sanitized geographic coordinates and speed indicators.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, wrapping the deterministic Output DTO containing audit confirmation IDs and status alerts.</returns>
    /// <exception cref="TelemetryValidationException">Encountered when structural check fails on packet payload.</exception>
    /// <exception cref="AirspaceViolationException">Encountered when localized business logic flags non-fly zones.</exception>
    public async Task<TelemetryOutputDto> ExecuteAsync(TelemetryInputDto input, CancellationToken cancellationToken = default)
    {
        // Ingestion logic implementation...
        await Task.CompletedTask;
        
        return new TelemetryOutputDto(
            AuditLogId: Guid.NewGuid(),
            FlightStatus: "CLEAR"
        );
    }
}

```

---

### 2.2. OpenAPI / Minimal API Endpoint (Public C# Transcription)

Instead of relying on disconnected YAML decorators inside JSDoc comments, modern ASP.NET Core applications define and bind API metadata directly to routes using fluent builders and strongly typed data contracts.

Here is how you map the public OpenAPI controller specifications natively:

```csharp
namespace Ingestion.Api.Endpoints;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Ingestion.Application.UseCases;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Ingestion endpoint conforming to clean architecture's modular registration pattern. [1]
/// </summary>
public sealed class ProcessDroneTelemetryEndpoint : IEndpoint [1]
{
    public void MapEndpoint(IEndpointRouteBuilder app) [1]
    {
        app.MapPost("/api/v1/drones/telemetry", HandleTelemetryInflowAsync)
           .WithName("IngestDroneTelemetry") [2]
           .WithSummary("Ingest operating flight unit telemetry")
           .WithDescription("Accepts and processes real-time telemetry coordinates. Triggers airspace validations.")
           .WithTags("Drone Management")
           .RequireAuthorization(); // Declares OAuth2/Bearer requirements natively
    }

    private static async Task<Results<Ok<TelemetryOutputDto>, UnauthorizedHttpResult, UnprocessableEntity<ProblemDetails>>> HandleTelemetryInflowAsync(
        TelemetryInputDto input,
        ProcessDroneTelemetryUseCase useCase,
        CancellationToken cancellationToken) [3, 4]
    {
        try
        {
            var result = await useCase.ExecuteAsync(input, cancellationToken);
            return TypedResults.Ok(result); [4]
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
        catch (AirspaceViolationException ex)
        {
            // Maps exceptions to RFC 7807 Problem Details
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Spatial violation encountered",
                Detail = ex.Message,
                Type = "https://errors.droneapi.com/spatial-violation",
                Instance = "/api/v1/drones/telemetry"
            };
            return TypedResults.UnprocessableEntity(problem);
        }
    }
}

/// <summary>
/// Input contract representing sanitized geocoordinates and indicators.
/// </summary>
public record TelemetryInputDto(
   
    Guid DroneId,

   
   
    double Latitude,

   
   
    double Longitude,

   
   
    double SpeedKnots
);

/// <summary>
/// Output contract with audit and execution details.
/// </summary>
public record TelemetryOutputDto(
    Guid AuditLogId,
    string FlightStatus
);

```

---

### 2.3. Key Mapping Concepts & Best Practices

**Strongly Typed Response Schemas (`TypedResults`):** Returning a generic `IResult` from endpoints hides the returned types from the native OpenAPI schema generator. Utilizing the union type `Results<Ok<T>,...>` explicitly informs the generation pipeline of all status codes (`200 OK`, `401 Unauthorized`, `422 Unprocessable Entity`) and dynamically constructs matching JSON response definitions.

**Built-in Schema Annotations:** Rather than writing custom JSON-schema rules manually, system validators like `and` are automatically parsed by the native OpenAPI compilation engine to apply `minimum`, `maximum`, and required rules directly inside the generated `openapi.json`.

**Endpoint Metadata Builders:** Fluent configurations (`.WithSummary()`, `.WithDescription()`, and `.WithTags()`) co-locate endpoints with their respective metadata, reducing documentation drift and eliminating loose YAML comments.

**Native Authentication Handling:** Appending `.RequireAuthorization()` registers security schemes in the endpoint route properties, ensuring documentation rendering engines like **Scalar** automatically generate interactive authorization input forms for Bearer authentication.