# Public API Presentation & Swagger Standards

Public endpoints must isolate the internal database or execution detail from the consumer boundary. The contract schema represents a product.

## 1. Route Swagger Requirements

* **Summary & Description**: Provide a brief functional action summary. The description must list scope tokens, pagination, and transactional side effects.
* **Property Examples**: Every primitive attribute in a schema definition must possess a realistic mock example value (e.g., use `150.50` instead of `0` for price).
* **RFC 7807 Problem Shape**: Document exceptions using the standardized problem details model:

```json
{
  "type": "https://api.domain.com/errors/insufficient-funds",
  "title": "Unprocessable Entity Mutation",
  "status": 422,
  "detail": "The request was rejected because the source account balance ($12.00) is lower than the transaction debit charge ($150.00).",
  "instance": "/api/v1/transactions/tx_8819201",
  "code": "ACCOUNT_BALANCE_LOW"
}

```

## 2. HTTP Status Code Assignment Protocol

| Status Code | Context Applied | Swagger Requirement |
| --- | --- | --- |
| **200 OK** | Successful execution returning read data payload or async tracking handles. | Document returned model array/object. |
| **201 Created** | Successful persistence mutation resource generation. | Include `Location` header pointing to the new resource. |
| **204 No Content** | Successful deletion/mutation with no response body to return. | Document the successful operation without a response schema. |
| **400 Bad Request** | Request serialization failures, type mismatches, or invalid input validation rules. | Expose field-by-field validation matrix. |
| **401 Unauthorized** | Missing, malformed, or expired authentication tokens. | Detail required token schemes (e.g., Bearer JWT). |
| **403 Forbidden** | Client is authenticated but lacks the necessary RBAC permissions or scopes. | Document necessary access clearances. |
| **404 Not Found** | Resource not found. | Document the successful operation without a response schema. |
| **409 Conflict** | Resource conflict with current state. | Document the successful operation without a response schema. |
| **422 Unprocessable** | Syntactically correct request that fails complex business rules or invariants. | Expose targeted business error codes. |
| **429 Too Many Requests** | Rate limiting applied. | Document the successful operation without a response schema. |
| **500 Internal Error** | Unexpected server-side execution failures. | Document the successful operation without a response schema. |

## Prompt Engineering Anchor

When instructed to write, expand, or refactor public Swagger/OpenAPI specifications, inject these constraints:

Objective: Generate public API documentation (OpenAPI 3.x specifications or framework controller metadata decorators) for the target route implementation.

1. SPEC PARSING: Extract mandatory HTTP status codes, default response headers, base error object schemas, and semantic style guidelines.
2. SCHEMATIC COMPLETENESS: Ensure every path parameter, query filter, header requirements token, and nested property object contains explicit type bounds, required validation markers, and hyper-realistic sample mock values.
3. Never leak inner-system stack traces, internal database models, or structural logic terminology into the public api schema outputs.
