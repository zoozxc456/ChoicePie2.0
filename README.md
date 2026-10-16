# ChoicePie 🥧

ChoicePie is a **real-time multiplayer quiz game** (think Kahoot) plus a **quiz library community** where users create, browse, and solo-attempt quizzes.

- **Host**: signs in, builds quizzes (manually or with AI), opens a room, and controls the game pace.
- **Player**: no account needed — enter the room code and a nickname to play from a phone.
- **Bigscreen**: a spectator view for projectors; watches but doesn't play.
- **Admin**: back office for members, quizzes, reports, and membership tiers.

## Concept

> *What if learning could be as fun as playing a game?*

Knowledge often stays on the slide or in the book and is never truly absorbed. ChoicePie turns passive content into an active, shared experience: paste in an article, your notes, or a link, and within minutes it becomes a quiz game a room full of people can play together. Everyone can be a teacher, and everyone can be a student.

Design principles that follow from this:

- **Low friction to create.** Creating quizzes is free, with no subscription or approval queue. AI can draft multiple-choice questions (with explanations) from pasted text or a URL, and the host can preview and edit every question before playing.
- **Zero friction to join.** Players don't sign up — they scan a QR code or enter a 6-digit room code, pick a nickname, and play from their phone. Only the host needs an account.
- **The host owns the pace.** Nothing auto-advances; the host decides when to reveal answers, pause, or move on, which fits talks, classrooms, and team sessions.
- **Speed and accuracy both matter.** Faster correct answers earn more points, keeping a live leaderboard competitive.
- **Learning continues after the game.** Solo attempts need no room and save every answer for question-by-question review; game history keeps rankings and wrong-answer reviews.
- **Knowledge is shared.** A public quiz library with creators, follows, favorites, and comments lets good quizzes travel beyond a single session.

Typical scenarios: a speaker running a quiz at the end of a tech talk, a teacher checking understanding in class, friends turning an obscure piece of history into a game, or a student making dry exam material more active.

Current MVP limits: up to 30 players per room; a room stays playable for 24 hours, becomes read-only after the game ends, and is cleared after 48 hours.

## Repository layout

This is a monorepo. The frontend and backend are developed independently and have separate CI pipelines.

```
ChoicePie2.0/
├── choicepie-frontend/   # Nuxt 4 frontend + Nitro BFF
├── choicepie-backend/    # .NET 10 Web API + SignalR GameHub
│   ├── ChoicePie.Backend/
│   └── docs/backend-spec.md
└── .github/workflows/    # frontend-ci.yml, backend-ci.yml
```

---

## Features

| Area | Features |
|---|---|
| Live game | Create / join room, start, skip question, pause; reconnect after disconnect (players via reconnect token, host via `RejoinRoom`, bigscreen via `WatchRoom`); game auto-pauses and shows a banner when the host goes offline |
| Quiz library | Create / edit / publish / archive, AI generation (can extract content from a URL), tags, sharing, related quizzes, favorites, comments, reports |
| Solo attempt | Play a quiz alone; scored separately from the multiplayer ranking |
| Community | Creator profiles, follow creators, following feed |
| History | Hosted and played sessions with rankings |
| Account | Email sign-up / sign-in, Google sign-in and linking, password reset, email verification, avatar, account deletion |
| Admin | Dashboard, members (suspension), quiz takedown, report handling, membership tiers (drive the daily AI quota) |

Scoring is computed on the backend and never trusts the client: 0–3 s = 1000, 3–6 s = 800, 6–10 s = 600, 10 s+ = 400, wrong answer = 0.

---

## Architecture

```
 Browser ──REST (same-origin /api/v1/**)──▶ Nuxt server (Nitro BFF) ──Bearer──▶ .NET Web API ──▶ PostgreSQL
    │                                                                              │
    └──────── SignalR WebSocket (/api/gamehub, direct to backend, cookie auth) ────┘──▶ Redis (GameRoom state / cache)
```

- **All REST goes through the BFF.** The frontend's `useApi()` calls same-origin `/api/v1/**`, which `choicepie-frontend/server/api/v1/` forwards to the backend (a `[...path].ts` catch-all, plus dedicated routes for auth / account / uploads / admin that handle cookies and token refresh).
- **SignalR connects directly to the backend.** `useGameRoom()` connects to `${NUXT_PUBLIC_API_BASE_URL}/api/gamehub` and authenticates with the `HttpOnly` auth cookie set by the backend.
- **Live room state lives in Redis** (`GameRoom`). When a game ends it is persisted to PostgreSQL as a `GameSession` history record.
- **The backend does not auto-advance question phases.** The host client's timers drive progress by calling `SkipQuestion`.
- `GameRoomLock` is an in-process lock, so **only a single backend instance is supported** for now. Running multiple instances requires a distributed lock.

### Frontend (`choicepie-frontend`)

| Concern | Tech |
|---|---|
| Framework | Nuxt 4 (`app/` directory), Vue 3 `<script setup lang="ts">` |
| UI | @nuxt/ui, Tailwind CSS v4, `cp-*` design tokens (`app/assets/css/design-tokens.css`) |
| State | Pinia (setup-store syntax) + pinia-plugin-persistedstate |
| Real-time | @microsoft/signalr |
| i18n | @nuxtjs/i18n, default `zh-TW`, plus `en` |
| Other | zod, chart.js, gsap, @vueuse, @sentry/nuxt |
| Tests | Vitest + @nuxt/test-utils (`tests/nuxt/unit/**/*.spec.ts`) |

Key directories: `pages/` (file-based routing), `components/<feature>/`, `stores/`, `composables/` (`useApi`, `useGameRoom`, `useGoogleIdentity`, `usePlayerSession`), `services/<domain>/client.ts` (typed API clients), `types/`, `server/` (BFF).

### Backend (`choicepie-backend/ChoicePie.Backend`)

.NET 10 / C# 14, **Clean Architecture + DDD + CQRS (MediatR) + domain events**, EF Core + PostgreSQL, HybridCache (in-memory L1 + Redis L2).

Dependency direction: `WebApi → Infrastructure → Application → Domain → Shared.Kernel`

| Project | Responsibility |
|---|---|
| `Domain` | Aggregates: `Member`, `AuthAccount`, `AdminUser`, `Quiz`, `QuizAttempt`, `GameRoom`, `GameSession`, `MembershipTier`, `Comment`, `QuizReport`, and more |
| `Application` | Feature slices with Commands / Queries / Contracts / Dtos (MediatR handlers) |
| `Infrastructure` | EF Core persistence and migrations, Query Services, JWT, Anthropic AI quiz generation, web page fetching (with SSRF protection), Cloudflare R2 uploads |
| `WebApi` | Controllers (`api/v{version}/...`), `Hubs/GameHub`, authentication and authorization setup |
| `Shared.Kernel` | `AggregateRoot`, `ValueObject`, `Enumeration`, `Specification`, DI marker interfaces |
| `Shared.Application` | MediatR contracts, paging, caching abstractions |
| `Shared.Hosting` | `ApiResponse<T>` envelope, exception handling, rate limiting, health checks, Sentry, Scalar/OpenAPI |
| `Shared.Infrastructure.*` | Persistence, Caching (Redis), Email (MailKit), Security (Argon2id, refresh tokens) |

Every project has a matching `*.Tests` project (NUnit 4 + NSubstitute + ExpectedObjects). `WebApi.Tests` and `Shared.Infrastructure.Persistence.Tests` use Testcontainers to run integration tests against real PostgreSQL / Redis.

Auth: a short-lived JWT access token plus a refresh token, both written as `HttpOnly` cookies. Authorization policies are `MemberOnly` (also checks the member isn't suspended or deleted) and `AdminOnly`.

---

## Local development

### Prerequisites

- Node.js 22, pnpm 10 (`packageManager: pnpm@10.13.1`)
- .NET SDK 10
- PostgreSQL and Redis
- Docker (for backend integration tests; Testcontainers starts the containers automatically)

### 1. Run the backend

Secrets are **not stored in `appsettings.json`** — use user secrets:

```bash
cd choicepie-backend/ChoicePie.Backend

dotnet user-secrets --project ChoicePie.Backend.WebApi set "DatabaseConnections:0:Type" "NPGSQL"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "DatabaseConnections:0:ConnectionString" "Host=localhost;Port=5432;Database=choicepie;Username=postgres;Password=postgres"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "RedisConnection:ConnectionString" "localhost:6379"
dotnet user-secrets --project ChoicePie.Backend.WebApi set "Jwt:SigningKey" "<random string, at least 32 characters>"

dotnet run --project ChoicePie.Backend.WebApi   # http://localhost:5185
```

- EF Core migrations and data seeders run automatically on startup.
- In Development, the Scalar API docs are at `http://localhost:5185/api-docs`.
- Health checks: `/health/live` and `/health/ready`.
- You can also run `docker compose up --build` (`compose.yaml` only contains the WebApi; provide PostgreSQL and Redis separately).

Optional settings (defaults in `appsettings.json`; put secrets in user secrets):

| Section | Purpose |
|---|---|
| `Smtp` | Verification and password-reset emails; `FrontendBaseUrl` is used for links in emails |
| `Google:ClientId` | Google sign-in |
| `AiQuizGeneration` | AI quiz generation (`Provider: Anthropic`, `ApiKey`, `Model`) |
| `CloudflareR2` | Quiz cover / avatar uploads |
| `AdminBootstrap` | Seeds the first admin account (`Email` / `Name` / `Password`) |
| `Cors:AllowedOrigins` | Allowed frontend origins |
| `ForwardedHeaders` | Trusted proxies. In deployment, add the Nuxt BFF's address, otherwise all users share a single rate-limit bucket |
| `RateLimiting` | Limits for `Auth` / `Email` / `Ai` / `Write` / `Global` |
| `Sentry` | Leave `Dsn` empty to disable |

### 2. Run the frontend

```bash
cd choicepie-frontend
cp .env.example .env    # fill in as needed
pnpm install
pnpm dev                # https://localhost:3000
```

The dev server in `nuxt.config.ts` uses HTTPS, so place `key.pem` and `cert.pem` in `choicepie-frontend/cert/` (e.g. generated with `mkcert`). This folder is gitignored.

Environment variables (see `.env.example` for details):

| Variable | Description |
|---|---|
| `NUXT_PUBLIC_API_BASE_URL` | Used only by SignalR (connects to `/api/gamehub`); set to `http://localhost:5185` for local development |
| `NUXT_BACKEND_API_URL` | Backend address used by the BFF (server-only); defaults to the value above |
| `NUXT_PUBLIC_GOOGLE_CLIENT_ID` | Google Identity Services client ID |
| `NUXT_PUBLIC_SENTRY_DSN` | Leave empty to disable Sentry entirely |
| `NUXT_TRUST_PROXY_HEADERS` | Set to `true` only when Nuxt sits behind a trusted load balancer |
| `NUXT_DEBUG_BACKEND_FETCH` | Log BFF requests outside dev (credentials are masked) |

---

## Tests and checks

CI runs on pushes to `master` / `develop` / `feat/**` and on PRs to `master` / `develop`, triggered separately by which paths changed.

```bash
# Frontend (same as CI)
cd choicepie-frontend
pnpm lint
pnpm typecheck
pnpm test

# Backend (same as CI; integration tests need Docker)
cd choicepie-backend/ChoicePie.Backend
dotnet build ChoicePie.Backend.sln
dotnet test ChoicePie.Backend.sln
```

---

## Further reading

| Document | Contents |
|---|---|
| [choicepie-frontend/CLAUDE.md](choicepie-frontend/CLAUDE.md) | Frontend architecture, routing, state management, component conventions |
| [choicepie-backend/ChoicePie.Backend/CLAUDE.md](choicepie-backend/ChoicePie.Backend/CLAUDE.md) | Backend architecture, configuration, auth flow, DDD / CQRS conventions |
| [choicepie-backend/docs/backend-spec.md](choicepie-backend/docs/backend-spec.md) | Data models and API spec |
| [choicepie-frontend/docs/todo.md](choicepie-frontend/docs/todo.md) | Feature history and open items |
| [choicepie-frontend/docs/unconnected-apis.md](choicepie-frontend/docs/unconnected-apis.md) | Backend APIs not yet wired to the frontend |

---

## Contributing

Contributions are by invitation only. See [CONTRIBUTING.md](CONTRIBUTING.md) for how to report issues, the development workflow, coding guidelines, and commit conventions.

---

## License

Copyright (c) 2026 Henry Chiang. All rights reserved.

This is proprietary software. No permission is granted to use, copy, modify, or distribute it without prior written permission. See [LICENSE](LICENSE) for details.
