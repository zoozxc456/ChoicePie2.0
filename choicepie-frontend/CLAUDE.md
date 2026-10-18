# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project

ChoicePie frontend — a Nuxt 4 app for a real-time multiplayer quiz game (host creates a room,
players join via code, live question/answer/ranking flow) plus a quiz library (create/browse/
solo-attempt quizzes). Backend is a separate .NET project at `../choicepie-backend`.

## Tech stack

- **Nuxt 4** (`app/` source directory convention) + Vue 3 (`<script setup lang="ts">` everywhere)
- **@nuxt/ui** for components (`UButton`, `UIcon`, etc.), Tailwind CSS v4
- **Pinia** (setup-store syntax only — `defineStore(name, () => {...})`, not options API)
- **@microsoft/signalr** for the real-time game hub (no socket.io)
- **@sentry/nuxt** error monitoring (DSN-gated, see Environment)
- **@nuxtjs/i18n** — default locale `zh-TW`, `strategy: 'no_prefix'`
- **pnpm** package manager (`pnpm-lock.yaml`)
- **Vitest** (`@nuxt/test-utils`) — unit tests under `tests/nuxt/unit/**/*.spec.ts`, see Commands

## Commands

`pnpm lint`, `pnpm typecheck` and `pnpm test` are the same checks CI runs (`.github/workflows/frontend-ci.yml`). For quick iteration use these directly:

```bash
npx eslint <files>          # lint specific files (fast, preferred over linting the whole repo)
npx eslint --fix <files>    # auto-fix (e.g. operator-linebreak)
npx vue-tsc --noEmit        # typecheck (pnpm typecheck)
pnpm lint                   # eslint . (whole repo, as CI does)
npx vitest run              # run unit tests (tests/nuxt/unit/**/*.spec.ts)
```

**Do not run `nuxi build` / `nuxi dev` to verify changes** — use typecheck + lint only.

End-to-end tests live in `../e2e` (Playwright against a multi-node Docker Compose stack; the Nuxt
build there runs only inside Docker — see `../e2e/README.md`). They select elements by `data-testid`,
so keep those attributes when refactoring game-room components.

## Directory structure (`app/`)

- `pages/` — file-based routing (see Routing below)
- `components/` — feature-scoped subfolders, not flat: `gameRoom/` (+ `gameRoom/host/`,
  `gameRoom/result/`), `library/`, `home/`, `host/`, `attempt/`, `admin/`, `settings/`, `creator/`,
  `common/`
- `stores/` — 15 Pinia stores: `auth`, `account`, `game`, `gameSession`, `quiz`, `quizAttempt`,
  `creator`, `favorites`, `following`, plus admin-side `adminAuth`, `adminDashboard`, `adminMember`,
  `adminMembershipTier`, `adminQuiz`, `adminQuizReport`
- `composables/` — `useApi.ts` (REST), `useGameRoom.ts` (SignalR), `useGoogleIdentity.ts` (Google
  Identity Services ID-token flow), `usePlayerSession.ts` (localStorage reconnect credential per room),
  `useQuestionDrafts.ts` (local question drafts for manual / AI quiz creation), `useDebouncedSearch.ts`
  (admin list search boxes), `useCountdown.ts` (per-question timer on the solo attempt page),
  `useModalTarget.ts` (admin "open a modal for item X" state), `useRequireLogin.ts`
  (`ensureLoggedIn()` — redirect guests to `/login?redirect=…` before login-only actions),
  `useAuthRedirect.ts` (shared by `login.vue` / `sign-in/index.vue`: sanitized `redirect`,
  `redirectIfAuthenticated()` via `fetchMe`, `goToRedirect()`)
- `services/{domain}/client.ts` — typed API-client layer between stores and `useApi()`
  (e.g. `services/quiz/client.ts`; domains: `account`, `creator`, `gameSession`, `quiz`,
  `quizAttempt`, `admin/*`; `services/auth.ts` is the flat auth client). Non-standard Nuxt
  convention but consistently used.
- `types/` — `api.ts` (backend DTOs, `ApiEnvelope<T>`), `gameRoom.ts`, `other.ts` (SignalR
  payload types), `quiz.ts`, `user.ts`, `auth.ts`, `account.ts`, `adminAuth.ts`, `adminUser.ts`
- `utils/` — `safeRedirect.ts` (`sanitizeRedirect`, same-site paths only), `absoluteUrl.ts`,
  `apiErrorMessage.ts` (`getApiErrorMessage` — use it instead of hand-rolled `instanceof ApiError`
  checks), `createPagedList.ts` (paged-list state helper used by list stores), `pagination.ts`
  (`totalPagesOf`, `buildPageItems`), `adminQuizStatus.ts` (admin quiz status style + i18n label key),
  `quizCover.ts` (cover color/emoji options, `coverColorClass`, `createDefaultCover`), `imageFile.ts`
  (`validateImageFile` for avatar / cover uploads), `passwordStrength.ts`, `shareUrl.ts`, `tags.ts`,
  plus `formatDate.ts`, `statusTone.ts`, `podium.ts`, `aiSource.ts`. Keep pure logic here (unit-tested
  under `tests/nuxt/unit/utils/`) rather than inline in components
- `layouts/` — `default`, `content`, `admin`, `bigScreen`
- `middleware/auth.ts` (member), `middleware/admin-auth.ts`, `plugins/auth.client.ts`
- `assets/css/design-tokens.css`, `assets/css/main.css`
- `app.config.ts` — Nuxt UI theme wiring to `cp-*` semantic colors

`server/` (Nitro, repo root of the frontend, not under `app/`) is the BFF — see API section.

`i18n/locales/{zh-TW,en}.json` lives at the **repo root**, not under `app/` (Nuxt i18n module
convention). Keys are namespaced roughly one-per-page/feature (`room`, `host`, `library`, etc).

## Routing

Key flows: `join/index.vue` → `join/[code].vue` (player joins by code), `host/room/[code].vue`
(host's live room, `middleware: ['auth']`), `bigscreen/[code].vue` (projector view, spectator only),
`library/*` (`index` has tabs all / mine / favorites / following; `[id]` quiz detail is public;
`new/{index,manual,ai}` and `mine/[id]/edit` need auth), `creators/[id].vue` (creator profile +
their quizzes, follow toggle), `attempt/[id].vue` (solo quiz attempt — separate scoring system from
the multiplayer SignalR ranking, do not conflate), `history/*` (hosted/played sessions),
`settings.vue` (profile, avatar, password, Google link/unlink, delete account; `auth` middleware),
`forgot-password` / `reset-password` / `verify-email`, and `admin/*` (`login`, dashboard, `members`,
`quizzes`, `quiz-reports`, `membership-tiers`; layout `admin`, `middleware: ['admin-auth']`).
Two member auth entry points exist: `login.vue` and `sign-in/index.vue` — check both before assuming
one is the canonical entry point when touching auth pages. Post-login `redirect` query values must go
through `sanitizeRedirect`.

## API / backend integration

**REST** — goes through the **Nuxt server BFF**, not directly to the backend. `useApi()`
(`app/composables/useApi.ts`) calls same-origin `/api/v1/**` with `useRequestFetch()` (so SSR
forwards the incoming cookies), unwraps `ApiEnvelope<T>` so callers get `T` directly, maps 429 to a
friendly message, and on 401 dedupes a single in-flight refresh (`useAuthStore().fetchMe()` for
members, `useAdminAuthStore().fetchMe()` for `/api/v1/admin/*`) and retries once, passing the fresh
token via the `x-fresh-access-token` header. Store code should go through
`app/services/{domain}/client.ts` wrappers, not call `useApi()` directly.

BFF (`server/`): `server/api/v1/[...path].ts` is a catch-all proxy that reads the httpOnly session
cookie and forwards it to the .NET backend as `Authorization: Bearer` (admin paths use the admin
cookies). Explicit routes handle auth (`auth/{login,register,google,refresh,logout}`,
`admin/auth/{login,refresh,logout}`, which set/rotate/clear the session cookies), `account/{avatar,
password,delete}` and `uploads/quiz-covers`. Helpers: `server/utils/backendFetch.ts` (calls
`runtimeConfig.backendApiUrl`, forwards the real client IP as `X-Forwarded-For`, logs redacted
request/response only in dev or when `debugBackendFetch` is on), `session.ts` (`cp_access_token` /
`cp_refresh_token` and `cp_admin_*` cookies), `refreshDedupe.ts`. Add new REST endpoints that need
a body/cookie handling beyond plain proxying as explicit routes; plain ones are covered by the
catch-all.

**WebSocket (game)** — `useGameRoom()` (`app/composables/useGameRoom.ts`) wraps a SignalR
`HubConnection` to `${apiBaseUrl}/api/gamehub` — SignalR still connects **directly** to the
backend (WebSockets only with `skipNegotiation` so multi-node backends need no sticky sessions, `withCredentials`, authenticated by the backend's own auth cookie,
which the BFF forwards from auth responses via `forwardSetCookie`); `NUXT_PUBLIC_API_BASE_URL` is
only used for this. Server→client events (`RoomCreated`, `PlayerJoined`, `PlayerDisconnected`,
`PlayerReconnected`, `PauseStateChanged`, `HostConnectionChanged`, `QuestionStart`, `AnswerResult`,
`GameEnd`, `RoomStateSync`, etc.) are routed into `useGameStore()` mutators; client→server calls
include `CreateRoom`, `StartGame`, `SkipQuestion`, `PauseGame`, `RejoinRoom` (host),
`WatchRoom` (bigscreen), `JoinRoom`, `RejoinAsPlayer`, `SubmitAnswer`. After an automatic
reconnect, `useGameRoom` restores membership by role (player → `RejoinAsPlayer` with the saved
reconnect token, host → `RejoinRoom`, bigscreen → `WatchRoom`).

Before assuming a backend feature is wired to UI, check `docs/unconnected-apis.md` (backend
endpoints with no frontend caller — currently none) and `docs/todo.md` (feature history and open
items).

**Environment** (`.env.example`): `NUXT_PUBLIC_API_BASE_URL` (SignalR only), `NUXT_BACKEND_API_URL`
(BFF → backend), `NUXT_PUBLIC_GOOGLE_CLIENT_ID`, `NUXT_PUBLIC_SENTRY_DSN` (empty = Sentry fully
disabled; `sentry.client.config.ts` / `sentry.server.config.ts`), `NUXT_TRUST_PROXY_HEADERS` (only
when Nuxt sits behind a trusted load balancer), `NUXT_DEBUG_BACKEND_FETCH`. Source-map upload is
disabled in `nuxt.config.ts`.

## State management

All Pinia stores use **setup-store syntax** (`ref`/`computed`/actions returned as a flat
object) — follow this pattern for new stores, not the options API.

- `game.ts` — real-time multiplayer state; phase machine `idle → waiting → question → result →
  ended`; **the backend does not auto-advance phases** — the host client drives
  `skipQuestion()` via timers/watchers (see `host/room/[code].vue`). Also tracks `isPaused`
  (`PauseStateChanged` / `RoomStateSync`; the timer is frozen from `remainingMs`), `isHostOnline`
  (`HostConnectionChanged`, drives `HostOfflineBanner`), and per-player `isConnected`
  (`PlayerDisconnected` / `PlayerReconnected`). `setRoomState()` applies a full `RoomStateSync`
  snapshot after (re)join. Not persisted. Player reconnect credentials live in
  `usePlayerSession()` (localStorage), not in the store.
- `auth.ts` — persisted (`user` only, via cookie storage so SSR sees it); `quiz.ts` — persisted
  (`aiUsedDate`/`aiUsedCount` only); `adminAuth.ts` — persisted (`adminUser` only). All via
  `pinia-plugin-persistedstate` with selective `pick`, not the whole store.
- `account.ts` (`/settings` actions), `favorites.ts` / `following.ts` (library tabs, built on
  `createPagedList`), `creator.ts` (creator profile page: profile + quizzes + follow/unfollow
  toggle), `gameSession.ts` (hosted/played history), `quizAttempt.ts` (solo attempts), `admin*.ts`
  (admin pages). None of these are persisted.
- Ranking data: the backend sends an explicit `rank` field per player
  (`GameRoom.cs: BuildRankings()`). **Always render/derive rank from `entry.rank`, never from
  `v-for` array index** — array order and rank can diverge under tied scores. This has caused
  real bugs (podium/badge showing the wrong place).

## Component conventions

- `<script setup lang="ts">`, PascalCase filenames, feature-scoped folders
- Props via `interface Props { ... }` + `defineProps<Props>()`
- `<style scoped lang="scss">` present even when empty
- `t()`/`useI18n()` for all user-facing text — no hardcoded strings
- Custom `cp-*` Tailwind-integrated classes (`text-cp-primary`, `border-cp-border`, etc.) plus
  plain global utility classes from `design-tokens.css` (`.player-chip`, `.room-code`,
  `.option-card`, `.rank-1/2/3`) — check that file before inventing new ad hoc styling
- Large pages with multiple distinct view states (e.g. per-phase UI) should be split into
  child components per state, with the page kept as a thin state-switch + lifecycle shell
  (see `host/room/[code].vue` and `components/gameRoom/host/*` for the established pattern)

## Working conventions

- Respond to the user in Traditional Chinese.
- When committing, stage only the files relevant to the requested change — don't sweep in
  unrelated pending edits without asking.
- Write unit tests for new code going forward (Vitest, see Commands above).
- Favor splitting into components, composables, or stores whenever it improves reuse and
  single-responsibility — do this proactively, not just when asked (see Component conventions
  above for the established pattern).
- Commit messages must be entirely in English, and must not include a co-author line/trailer.
