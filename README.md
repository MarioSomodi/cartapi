# Cart API

Minimal shopping-cart Web API

The implementation is intentionally narrow but production-minded:

- `.NET 10`
- ASP.NET Core Web API
- PostgreSQL via EF Core
- JWT bearer authentication with cart ownership scoped by `tenantId + sub`
- Swagger, API versioning, `ProblemDetails`, health checks, correlation IDs, ECS-style structured logging, and OpenTelemetry tracing

## Solution Structure

```text
src/
  Cart.Api
  Cart.Application
  Cart.Domain
  Cart.Persistence
tests/
  Cart.Domain.Tests
  Cart.IntegrationTests
```

## Implemented Scope

HTTP endpoints:

| Method | Route | Notes |
| --- | --- | --- |
| `POST` | `/api/v1/cart` | creates an empty cart or returns the existing active cart |
| `GET` | `/api/v1/cart` | gets the current authenticated cart |
| `POST` | `/api/v1/cart/items` | adds an item, lazily creating the cart if needed |
| `PUT` | `/api/v1/cart/items/{itemId}` | changes item quantity |
| `DELETE` | `/api/v1/cart/items/{itemId}` | removes an item |
| `DELETE` | `/api/v1/cart` | clears the cart |
| `GET` | `/health` | aggregate health |
| `GET` | `/health/live` | process liveness |
| `GET` | `/health/ready` | dependency readiness |

Notable business rules:

- one active cart per `tenantId + sub`
- same-SKU adds merge into one line
- if the same SKU is added with a different `unitPrice` or `currency`, the API rejects the request with `409 Conflict`
- optimistic concurrency conflicts are surfaced as `409 Conflict`

## Prerequisites

- .NET SDK `10.0.201` or compatible via `global.json`
- Docker Desktop or another Docker engine with Compose support

## Local Run

Start API + PostgreSQL with Docker Compose:

```powershell
docker compose up -d
```

API will be available at:

- `http://localhost:8080`

Swagger UI is available at:

- `http://localhost:8080/swagger`

Visual Studio startup for API + DB:

1. Open `Cart.slnx` in Visual Studio.
2. Set `docker-compose` as the Startup Project.
3. Run with the `Docker Compose` profile.

If you prefer running API from the SDK and only DB in Docker:

```powershell
docker compose up -d postgres
dotnet run --project src/Cart.Api
```

## Container Workflow

This repository uses a single container workflow:

- one Dockerfile: `Dockerfile` (repo root)
- one compose file: `docker-compose.yml`
- Visual Studio starts containers through `docker-compose.dcproj`

Use this for team/local/CI consistency.

## Authentication

The API validates bearer tokens. For local development, generate a token with `dotnet user-jwts`.

Create a token for SDK run mode (`http://localhost:5248` / `https://localhost:7039`):

```powershell
dotnet user-jwts create --project src/Cart.Api --name subject-1 --claim tenantId=tenant-1 --output token
```

If API is running via Docker Compose on `http://localhost:8080`, create a token with explicit audience:

```powershell
dotnet user-jwts create --project src/Cart.Api --name subject-1 --claim tenantId=tenant-1 --audience http://localhost:8080 --output token
```

When running via Docker Compose, the API container reads local user-secrets through the compose volume mount `${APPDATA}/Microsoft/UserSecrets -> /root/.microsoft/usersecrets` so `dotnet user-jwts` signing keys can be validated inside the container.

Use the returned token in Swagger via the `Authorize` button or via Postman

## Local Database Configuration

Default development connection string:

```text
Host=localhost;Port=5432;Database=cartdb;Username=postgres;Password=postgres
```

Useful Compose commands:

```powershell
docker compose logs -f api
docker compose logs -f postgres
docker compose down
```

## Tests

Run the full test suite:

```powershell
dotnet test Cart.slnx -c Debug
```

Current automated coverage includes:

- domain tests for cart aggregate rules
- integration tests for health, controller flows, auth/ownership, validation, correlation ID behavior, and conflict handling

## Observability

The API includes:

- response/request correlation via `X-Correlation-ID`
- ECS-style JSON console logging through Serilog
- request log enrichment with `CorrelationId`, `TenantId`, `SubjectId`, and `CartId` when available
- OpenTelemetry tracing for ASP.NET Core and `HttpClient`
- development console trace exporter enabled in `appsettings.Development.json`

If a request does not provide `X-Correlation-ID`, the API generates one and echoes it back on the response.

## Container Image

Validate the container build locally:

```powershell
docker build -t cartapi:local .
```

The runtime container listens on port `8080`.

## CI

GitHub Actions workflow: [.github/workflows/ci.yml](.github/workflows/ci.yml)

It validates:

- restore
- build
- tests
- Docker Compose configuration
- Docker image build

## Some notes and assumption I made from the task

- the coded slice is a slim modular monolith, while the architecture document describes a broader service-oriented platform
- authentication is intentionally limited to JWT validation; login/session flows are out of scope
- integration tests currently focus on API behavior and use a test authentication scheme plus in-memory cart persistence for authenticated scenarios
