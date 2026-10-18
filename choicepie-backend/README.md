# ChoicePie Backend

The API for [ChoicePie](../README.md) — a real-time multiplayer quiz game and quiz library. It serves REST endpoints (called through the [frontend's](../choicepie-frontend/README.md) Nuxt BFF) and a SignalR hub for live games.

## Tech stack

- **.NET 10** / C# 14, ASP.NET Core Web API + SignalR
- **Clean Architecture + DDD + CQRS** with MediatR and domain events
- **EF Core** + PostgreSQL (Npgsql, snake_case naming)
- **HybridCache** (in-memory L1 + Redis L2); Redis also stores live game rooms
- JWT auth (HttpOnly cookies), Argon2id password hashing, Google sign-in
- MailKit (email), Anthropic API (AI quiz generation), Cloudflare R2 (uploads)
- Scalar / OpenAPI, Sentry (optional)
- NUnit 4, NSubstitute, ExpectedObjects, Testcontainers

## Getting started

Requires the .NET 10 SDK, PostgreSQL, and Redis. Docker is needed for integration tests.

All commands below run from `ChoicePie.Backend/`.

### 1. Configure secrets

Secrets are not stored in `appsettings.json`. Use user secrets for the WebApi project:

```bash
dotnet user-secrets --project ChoicePie.Backend.WebApi set "DatabaseConnections:0:Type" "NPGSQL"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "DatabaseConnections:0:ConnectionString" "Host=localhost;Port=5432;Database=choicepie;Username=postgres;Password=postgres"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "RedisConnection:ConnectionString" "localhost:6379"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "Jwt:SigningKey" "<random string, at least 32 characters>"
```

`DatabaseConnections` is an array; only the first entry is used, and only `NPGSQL` is supported.

To sign in to the admin back office, also seed the first admin:

```bash
dotnet user-secrets --project ChoicePie.Backend.WebApi set "AdminBootstrap:Email" "admin@example.com"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "AdminBootstrap:Name" "Admin"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "AdminBootstrap:Password" "<password>"
```

### 2. Run

```bash
dotnet run --project ChoicePie.Backend.WebApi   # http://localhost:5185
```

On startup, the API applies EF Core migrations and runs the data seeders automatically.

| URL | Description |
|---|---|
| `http://localhost:5185/api-docs` | Scalar API docs (Development only) |
| `http://localhost:5185/api/v1/...` | REST API |
| `http://localhost:5185/api/gamehub` | SignalR game hub |
| `/health/live`, `/health/ready` | Liveness (process) and readiness (PostgreSQL + Redis) |

You can also build and run the container with `docker compose up --build`. The compose file only contains the API; PostgreSQL and Redis must be provided separately.

### Optional configuration

Defaults live in `appsettings.json`; put secret values in user secrets.

| Section | Purpose |
|---|---|
| `Jwt` | `Issuer`, `Audience`, `AccessTokenExpirationSeconds` (`SigningKey` is required, see above) |
| `Cors:AllowedOrigins` | Allowed frontend origins |
| `Smtp` | Email sender; `FrontendBaseUrl` is used to build links in emails |
| `Google:ClientId` | Google sign-in |
| `AiQuizGeneration` | `Provider` (`Anthropic`), `ApiKey`, `Model` |
| `CloudflareR2` | Quiz cover and avatar uploads |
| `Auth:CookieDomain` | Set for cross-subdomain deployments (switches cookies to `SameSite=None; Secure`) |
| `ForwardedHeaders` | `KnownProxies` / `KnownNetworks` trusted for `X-Forwarded-For` |
| `RateLimiting` | `Auth` / `Email` / `Ai` / `Write` / `Global`, each `PermitLimit` + `WindowSeconds` |
| `Sentry` | `Dsn` (empty = disabled), `TracesSampleRate` |

## Project structure

```
ChoicePie.Backend/
├── ChoicePie.Backend.Domain/            # Aggregates, value objects, domain events, specifications
├── ChoicePie.Backend.Application/       # Feature slices: Commands / Queries / Contracts / Dtos
├── ChoicePie.Backend.Infrastructure/    # EF Core, migrations, seeding, query services, identity, external services
├── ChoicePie.Backend.WebApi/            # Controllers, Requests, GameHub, auth setup, Program.cs
├── ChoicePie.Backend.Shared.Kernel/     # AggregateRoot, ValueObject, Enumeration, Specification, DI markers
├── ChoicePie.Backend.Shared.Application/            # MediatR contracts, paging, caching abstractions
├── ChoicePie.Backend.Shared.Hosting/                # ApiResponse envelope, exception handling, rate limiting, health checks, Sentry
├── ChoicePie.Backend.Shared.Infrastructure.*/       # Persistence, Caching, Email, Security
└── *.Tests/                             # One test project per product project
```

Dependency direction: `WebApi → Infrastructure → Application → Domain → Shared.Kernel`. Inner layers never reference outer ones.

### Domain

Aggregates live in `ChoicePie.Backend.Domain/Aggregates/`:

| Area | Aggregates |
|---|---|
| Identity | `Member`, `AuthAccount`, `AdminUser`, `AdminAuthAccount`, `RefreshToken`, `PasswordResetToken`, `EmailVerificationToken` |
| Membership | `MembershipTier`, `AiUsageLog` |
| Quizzes | `Quiz`, `QuizAttempt`, `QuizFavorite`, `CreatorFollow`, `Comment`, `QuizReport` |
| Live game | `GameRoom` (stored in Redis), `GameSession` (history in PostgreSQL) |

## API overview

All REST responses use the `ApiResponse<T>` envelope (`Code` / `Status` / `Data` / `Message`). Routes are versioned as `api/v1/...`.

| Controller | Responsibility |
|---|---|
| `AuthController` | Register, login, Google login, refresh, logout, password reset, email verification |
| `AccountController` | Profile, avatar, password, Google link/unlink, account deletion |
| `MeController` | Favorites, following, following quizzes |
| `CreatorsController` | Creator profiles, their quizzes, follow/unfollow |
| `QuizzesController` | Quiz CRUD, publish/archive, AI generation, tags, share, related, favorite, comments, report |
| `QuizAttemptsController` | Solo quiz attempts |
| `GameSessionsController` | Hosted and played game history |
| `UploadsController` | Quiz cover uploads |
| `Admin*Controller` | Admin auth, dashboard, members, membership tiers, quizzes, quiz reports (`AdminOnly`) |

### Game hub (`/api/gamehub`)

| Caller | Methods |
|---|---|
| Host (signed in, `MemberOnly`) | `CreateRoom`, `StartGame`, `SkipQuestion`, `PauseGame`, `RejoinRoom` |
| Player (anonymous) | `JoinRoom`, `RejoinAsPlayer`, `SubmitAnswer` |
| Bigscreen (anonymous) | `WatchRoom` |

Server → client events include `RoomCreated`, `PlayerJoined`, `PlayerDisconnected`, `PlayerReconnected`, `GameStarted`, `QuestionStart`, `AnswerProgress`, `AnswerResult`, `QuestionEnd`, `GameEnd`, `PauseStateChanged`, `HostConnectionChanged`, and `RoomStateSync`.

> The hub serializes room updates with an in-process lock, so **only a single backend instance is supported**. Scaling out requires a distributed lock.

## Database migrations

Migrations live in `ChoicePie.Backend.Infrastructure/Persistence/Migrations/` and are applied automatically on startup. To add one (requires the `dotnet-ef` tool):

```bash
dotnet ef migrations add <Name> \
  --project ChoicePie.Backend.Infrastructure \
  --startup-project ChoicePie.Backend.WebApi \
  --output-dir Persistence/Migrations
```

## Tests

```bash
dotnet build ChoicePie.Backend.sln
dotnet test ChoicePie.Backend.sln
```

- Unit tests: NUnit + NSubstitute + ExpectedObjects.
- Integration tests: `WebApi.Tests` (`WebApplicationFactory` + Testcontainers PostgreSQL) and `Shared.Infrastructure.Persistence.Tests` start real containers, so Docker must be running.
- New code is developed with TDD — start from a failing test.

## Further reading

- [ChoicePie.Backend/CLAUDE.md](ChoicePie.Backend/CLAUDE.md) — detailed architecture, auth flow, and DDD / CQRS conventions
- [docs/backend-spec.md](docs/backend-spec.md) — early design spec (for reference; the code is the source of truth)
- [CONTRIBUTING.md](../CONTRIBUTING.md) — workflow and coding guidelines
