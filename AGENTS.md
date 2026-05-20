# OrderAPI — Agent Instructions

Event ticketing system: an ASP.NET Core 8 Web API for managing orders, carts, seat holds, and payments.

## Solution Structure

| Project | Role |
|---|---|
| `OrderAPI.Domain` | Pure core — use case handlers, storage interfaces, domain models, exceptions, service interfaces. **No project references.** |
| `OrderAPI.DAL` | EF Core + SQL Server — entities, `DbContext`, storage implementations, specifications, migrations |
| `OrderAPI` | ASP.NET Core host — controllers, `Program.cs`, middleware, external HTTP clients |
| `OrderAPI.Tests` | xUnit + Moq unit tests (one file per use case handler) |
| `OrderAPI.IntegrationTests` | xUnit + `WebApplicationFactory` integration tests against real HTTP endpoints |

Dependency direction: `OrderAPI` → `OrderAPI.DAL` → `OrderAPI.Domain`.

## Key Patterns

### Use Cases (MediatR CQRS)
Each business operation lives in `OrderAPI.Domain/UseCases/<UseCase>/` as two files:
- `BookCartRequest.cs` — implements `IRequest<TResponse>`
- `BookCartRequest.Handler.cs` — implements `IRequestHandler<TRequest, TResponse>`

Controllers have only `IMediator` and dispatch via `_mediator.Send(request)`. Follow this exact two-file split when adding a new use case.

### Storage Pattern (one interface per operation)
Each data operation has a dedicated interface + implementation:
- `OrderAPI.Domain/Storage/<Operation>/I<Operation>Storage.cs` — interface
- `OrderAPI.DAL/Storage/<Operation>/<Operation>Storage.cs` — implementation

All storage implementations are registered as `Scoped` in `OrderAPI.DAL/ServiceCollectionExtensions.cs`. Add new ones there.

### Specifications
`ISpecification<T>` in `OrderAPI.DAL/Specifications/` returns `Expression<Func<T, bool>>`, applied with `.Where(spec.ToExpression())`. Use them to compose query filters.

### Exception → HTTP Mapping
`OrderAPI/Middleware/ExceptionHandlerMiddleware.cs` maps domain exceptions to status codes. Add new exception mappings there, not in controllers.

### External Service Clients
- `ICatalogApiClient` / `ICatalogApiClient` in `OrderAPI.Domain/Services/` — interfaces only; HTTP implementations in `OrderAPI/Services/CatalogClient/`
- `IPaymentApiClient` / `PaymentApiClient` in `OrderAPI/Services/PaymentClient/`
- Base URLs come from `appsettings.json`: `CatalogApi:BaseUrl`, `PaymentApi:BaseUrl`

## Build & Test Commands

```powershell
dotnet build OrderAPI.sln
dotnet run --project OrderAPI/OrderAPI.csproj

# Unit tests only
dotnet test OrderAPI.Tests/OrderAPI.Tests.csproj

# Integration tests only (require no external services — all replaced in-process)
dotnet test OrderAPI.IntegrationTests/OrderAPI.IntegrationTests.csproj

# EF Core migrations
dotnet ef migrations add <Name> --project OrderAPI.DAL --startup-project OrderAPI
dotnet ef database update --project OrderAPI.DAL --startup-project OrderAPI
```

## Configuration

`appsettings.json` keys:
- `ConnectionStrings:Redis` — `localhost:6379`
- `ConnectionStrings:DefaultConnection` — SQL Server (in `appsettings.Development.json`)
- `Jwt:Key` / `Jwt:Issuer` / `Jwt:Audience`
- `CatalogApi:BaseUrl` / `PaymentApi:BaseUrl`

## Testing Conventions

**Unit tests** (`OrderAPI.Tests`):
- One test class per use case handler
- All dependencies mocked with `Moq`; handler instantiated directly, no DI container
- EF Core InMemory used for DAL-level storage tests

**Integration tests** (`OrderAPI.IntegrationTests`):
- `OrderApiFactory` replaces SQL Server → InMemory, Redis → `AddDistributedMemoryCache()`, `IPaymentApiClient` → `Mock`
- `JwtHelper.CreateToken(userId)` creates bearer tokens for authenticated requests
- Tests call real HTTP endpoints through `HttpClient`

## Shared Library

`Homework.Ticketing.System.Shared` NuGet provides `ResultModel<T>` (paginated wrapper), `ECurrency`, and other shared models used across domain and response types.

## Conventions

- Split request and handler into separate files: `<UseCase>Request.cs` + `<UseCase>Request.Handler.cs`
- One class per file, one file per storage operation
- Authorization: `[Authorize]` on `CartsController`; `[AllowAnonymous]` on specific `OrdersController` actions
- Cache invalidation: implement `IEventCacheInvalidator` and wire via `RedisEventCacheInvalidator` when order state changes affect the catalog cache
