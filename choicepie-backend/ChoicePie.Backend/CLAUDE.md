# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build ChoicePie.Backend.sln          # build everything
dotnet run --project ChoicePie.Backend.WebApi   # run the API (http://localhost:5185)
docker compose up --build                   # containerized run (WebApi Dockerfile)
```

There are no test projects yet. In Development, Scalar API docs are served at `/api-docs`.

Runtime config is NOT in appsettings.json — the WebApi project uses user secrets. Required sections: `DatabaseConnections` (an **array** of `{ "Type": "NPGSQL", "ConnectionString": "..." }`; only the first entry is used and only `NPGSQL` is supported) and `RedisConnection:ConnectionString`.

## Architecture

.NET 10 / C# 14 Web API. Clean Architecture + DDD, MediatR, EF Core + PostgreSQL, HybridCache + Redis.

Projects split into two groups:

- **`ChoicePie.Backend.Shared.*`** — reusable framework code, where nearly all logic currently lives:
  - `Shared.Kernel` — domain primitives, no framework deps: `Entity<TId>`, `AuditableEntity<TId>` (audit columns + soft delete), `AppendOnlyAuditableEntity`, `AggregateRoot<TId>` (collects domain events), `ValueObject`, `Enumeration`, `DomainException`, `IRepository<T>`, `IUnitOfWork`, DI marker interfaces.
  - `Shared.Application` — MediatR contracts, `DomainEventNotification<T>`, `ICachingService`, paging contracts (`PagedResult`, `PaginationParameters`).
  - `Shared.Infrastructure.Persistence` — EF Core: `EfGenericRepository`, `EfUnitOfWork`, Npgsql registration, auditable-entity configurations.
  - `Shared.Infrastructure.Caching` — `HybridCachingService` (L1 memory ~3 min, L2 Redis ~30 min, key prefix `ChoicePie_`).
  - `Shared.Hosting` — ASP.NET Core cross-cutting: `ApiResponse<T>` envelope (`Code`/`Status`/`Data`/`Message`), global exception handler chain (`DomainException` → `BadRequest` → default, via `IExceptionHandler` + ProblemDetails), Scalar/OpenAPI setup, custom JSON converters, Scrutor DI scanning.
- **Product slice** — `Domain`, `Application`, `Infrastructure`, `WebApi`. Mostly empty scaffolds so far (each has an `AssemblyReference` marker class used for assembly scanning). Dependency direction: `WebApi → Infrastructure → Application → Domain → Shared.Kernel`; each layer may also reference its matching `Shared.*` project. Never point an inner layer at an outer one.

### Architectural principles

New code must follow **DDD** (aggregates own their invariants; model with the Shared.Kernel primitives — `AggregateRoot`, `ValueObject`, `Enumeration`; repositories only for aggregate roots), **Clean Architecture** (respect the dependency direction above; Domain stays framework-free; Use Cases live in Application as MediatR handlers), and **EDA** (state changes communicate via domain events raised on aggregates and dispatched through `EfUnitOfWork` — don't call other modules' services directly when an event fits). Apply established design patterns deliberately when they fit, and watch for code smells (long methods, large classes, primitive obsession, feature envy, etc.) when writing or reviewing code.

Local `dotnet-*` skills enforce these principles — invoke the matching skill when designing, implementing, or reviewing code in that area:

| Concern | Skill |
|---|---|
| DDD modeling / aggregate boundaries | `dotnet-ddd-skill` |
| Layering / dependency-direction review | `dotnet-clean-architecture-skill` |
| Command/Query separation (MediatR) | `dotnet-cqrs-skill` |
| Domain events / messaging | `dotnet-eda-skill` |
| Design pattern selection & review | `dotnet-design-pattern-detector-skill` |
| Code smell detection / refactoring | `dotnet-code-smell-detector-skill` |
| Full combined architecture review | `dotnet-architecture-review-suite-skill` |

**This project is developed with TDD.** When implementing any new class, method, or feature, invoke `dotnet-tdd-skill` and drive the work through the Red-Green-Refactor cycle — start from a failing test, don't write production code first. For hardening or reviewing already-written tests, use `dotnet-nunit-unit-test-skill` (NUnit3 + NSubstitute + ExpectedObjects) for unit tests and `dotnet-integration-test-skill` (Testcontainers / WebApplicationFactory) for repository/API tests.

### Conventions that aren't obvious from a single file

- **DI is by marker interface, not manual registration.** Implement `IScopedDependency`, `ITransientDependency`, or `ISingletonDependency` (Shared.Kernel) and Scrutor registers the class `AsSelfWithInterfaces` via the `AddApplication`/`AddDomain`/`AddInfrastructure` calls in `Program.cs`. Only cross-cutting services (e.g. `ICachingService`, `IUnitOfWork`) are registered explicitly in the Shared extension methods.
- **Domain events dispatch after save.** Aggregates call `AddDomainEvent(...)`; `EfUnitOfWork.SaveChangesAsync` drains events from the change tracker, saves, then publishes each as `DomainEventNotification<TEvent>` through MediatR. Handlers implement `INotificationHandler<DomainEventNotification<TEvent>>`.
- **Repositories are subclasses, not a registered generic.** Derive from `EfGenericRepository<TEntity, TContext>`, implement `GetByIdAsync`, and override `ApplyInclude` for eager loading. Mark the subclass with a DI marker interface to register it. Each aggregate owns a per-aggregate repository interface in its Domain folder (e.g. `IMemberRepository : IRepository<Member>`) rather than Application code depending on `IRepository<T>` directly; filtered queries go through named `ISpecification<T>`/`Specification<T>` classes owned by the aggregate (e.g. `MemberByEmailSpecification`) instead of inline `Expression<Func<T,bool>>` predicates.
- **Aggregate folder layout**: `Domain/Aggregates/<Name>/` holds the aggregate root class directly (e.g. `Aggregates/Member/Member.cs`, `Aggregates/Quiz/Quiz.cs`), plus `Events/`, `Exceptions/`, `Specifications/`, and `Enums/` (for `Enumeration<T>` types owned by that aggregate) subfolders, and an `Entities/` subfolder reserved for that aggregate's *non-root* child entities only (empty/absent if the aggregate has none). Value objects shared across aggregates live in `Shared.Kernel/ValueObjects/`, not under any one aggregate.
- **Persistence setup**: pooled `DbContext` (`AddSharedDbContextPool`) with snake_case naming (EFCore.NamingConventions). `ChoicePieDbContext` auto-applies all `IEntityTypeConfiguration` from the Infrastructure assembly — add entity configurations there.
- **Extension methods use the C# 14 `extension(...)` block syntax** (see `PersistenceServiceCollectionExtensions`); follow that style in new extension classes.
- Code comments and some exception messages are written in Traditional Chinese; keep that style when editing those files.