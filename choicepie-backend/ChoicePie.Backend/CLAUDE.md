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
- **Repositories are command-only.** `IRepository<T>`/per-aggregate repositories (`IMemberRepository`, `IAuthAccountRepository`, ...) are injected only into MediatR *command* handlers, which need a tracked aggregate to mutate.
- **Query handlers depend on a per-feature Query Service, never on `IReadRepository` directly.** Mirrors the write-side rule (no generic repository injected straight into Application code): each feature owns an `I<X>QueryService` contract in `Application/<Feature>/Contracts/` (e.g. `IQuizQueryService`, `IMemberQueryService`), consumed by that feature's MediatR Query handlers. The MediatR Query handler stays thin: resolve the current-user/request context if needed, call the Query Service, return or propagate its result — no `IQueryable`/projection code belongs in a handler, and `Application` never references `IReadRepository`.
- **Query Service implementations live in `Infrastructure`, not `Application`.** The concrete class (e.g. `Infrastructure/QueryServices/Quizzes/QuizQueryService.cs : IQuizQueryService, IScopedDependency`) is the only thing that injects `IReadRepository` and projects straight into the response DTO with `.Select(...)`/LINQ joins (e.g. `readRepository.Query<Member>().Select(m => new MemberDto(...))`) — never call `Query<T>().ToList()`/materialize the full entity and map in C# afterward, that defeats the point of projection. This keeps `IQueryable`/EF-translatability concerns (e.g. `[NotMapped]` computed properties like `Quiz.OwnerId`/`Quiz.QuestionCount` aren't translatable — project through the underlying mapped property, `CreatorId`/`Questions.Count`, instead) out of Application entirely. Prefer a LINQ `join`/`GroupJoin`+`DefaultIfEmpty()` (LEFT JOIN) in one query over separate round-trips when the second aggregate's absence isn't itself an error (e.g. Quiz's creator falls back to "Unknown", never throws). Only fall back to separate projected queries per aggregate, combined in C#, when you need to tell "aggregate A not found" apart from "aggregate B not found" for distinct error handling (e.g. `MemberQueryService`: `MemberNotFoundException` vs `AuthAccountNotFoundException`) — a single JOIN can't make that distinction.
- **`IReadRepository` lives in `Shared.Infrastructure.Persistence/Repositories/IReadRepository.cs`, not Shared.Application.** It's not a cross-layer port (nothing in `Application` references it, only `Infrastructure` Query Services do) — it exists purely as a testability seam so Query Services can be unit-tested by mocking `Query<T>()` with an in-memory `List<T>().AsQueryable()` instead of needing a real Postgres/EF InMemory-provider integration test. Its concrete implementation is `Infrastructure/Persistence/Repositories/ReadRepository.cs : EfGenericReadRepository<ChoicePieDbContext>, IReadRepository, IScopedDependency` — one shared class for every aggregate (unlike write repositories, `EfGenericReadRepository<TContext>`'s `Query<T>()` is already generic per-call, so it doesn't need a subclass per aggregate). Query Service unit tests live in `ChoicePie.Backend.Infrastructure.Tests` (NUnit3 + NSubstitute, same as `Application.Tests`/`Domain.Tests`).
- **Aggregate folder layout**: `Domain/Aggregates/<Name>/` holds the aggregate root class directly (e.g. `Aggregates/Member/Member.cs`, `Aggregates/Quiz/Quiz.cs`), plus `Events/`, `Exceptions/`, `Specifications/`, and `Enums/` (for `Enumeration<T>` types owned by that aggregate) subfolders, and an `Entities/` subfolder reserved for that aggregate's *non-root* child entities only (empty/absent if the aggregate has none). Value objects shared across aggregates live in `Shared.Kernel/ValueObjects/`, not under any one aggregate.
- **Persistence setup**: pooled `DbContext` (`AddSharedDbContextPool`) with snake_case naming (EFCore.NamingConventions). `ChoicePieDbContext` auto-applies all `IEntityTypeConfiguration` from the Infrastructure assembly — add entity configurations there.
- **Extension methods use the C# 14 `extension(...)` block syntax** (see `PersistenceServiceCollectionExtensions`); follow that style in new extension classes.
- Code comments and some exception messages are written in Traditional Chinese; keep that style when editing those files.