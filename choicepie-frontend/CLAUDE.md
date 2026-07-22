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
- **@nuxtjs/i18n** — default locale `zh-TW`, `strategy: 'no_prefix'`
- **pnpm** package manager (`pnpm-lock.yaml`)
- No test framework is configured (no vitest/jest/playwright, no test files)

## Commands

There is no `lint`/`typecheck`/`test` script in `package.json`. Use these directly:

```bash
npx eslint <files>          # lint specific files (fast, preferred over linting the whole repo)
npx eslint --fix <files>    # auto-fix (e.g. operator-linebreak)
npx vue-tsc --noEmit        # typecheck
```

**Do not run `nuxi build` / `nuxi dev` to verify changes** — use typecheck + lint only.

## Directory structure (`app/`)

- `pages/` — file-based routing (see Routing below)
- `components/` — feature-scoped subfolders, not flat: `gameRoom/` (+ `gameRoom/host/`,
  `gameRoom/result/`), `library/`, `home/`, `host/`, `attempt/`, `common/`
- `stores/` — 5 Pinia stores: `auth.ts`, `game.ts`, `gameSession.ts`, `quiz.ts`, `quizAttempt.ts`
- `composables/` — only `useApi.ts` (REST) and `useGameRoom.ts` (SignalR)
- `services/{domain}/client.ts` — typed API-client layer between stores and `useApi()`
  (e.g. `services/quiz/client.ts`). Non-standard Nuxt convention but consistently used.
- `types/` — `api.ts` (backend DTOs, `ApiEnvelope<T>`), `gameRoom.ts`, `other.ts` (SignalR
  payload types), `quiz.ts`, `user.ts`, `auth.ts`
- `middleware/auth.ts`, `plugins/auth.client.ts`
- `assets/css/design-tokens.css`, `assets/css/main.css`
- `app.config.ts` — Nuxt UI theme wiring to `cp-*` semantic colors

`i18n/locales/{zh-TW,en}.json` lives at the **repo root**, not under `app/` (Nuxt i18n module
convention). Keys are namespaced roughly one-per-page/feature (`room`, `host`, `library`, etc).

## Routing

Key flows: `join/index.vue` → `join/[code].vue` (player joins by code), `host/room/[code].vue`
(host's live room, `middleware: ['auth']`), `bigscreen/[code].vue` (projector view),
`library/*` (browse/create/edit quizzes, `mine/` = user's own), `attempt/[id].vue` (solo
quiz attempt — separate scoring system from the multiplayer SignalR ranking, do not conflate).
Two auth entry points exist: `login.vue` and `sign-in/index.vue` — check both before assuming
one is the canonical entry point when touching auth pages.

## API / backend integration

**REST** — `useApi()` (`app/composables/useApi.ts`): raw `$fetch` wrapper, `credentials:
'include'` (cookie-based auth), unwraps `ApiEnvelope<T>` so callers get `T` directly, handles
401 by deduping a single in-flight refresh + retry via `useAuthStore().fetchMe()`. Store code
should go through `app/services/{domain}/client.ts` wrappers, not call `useApi()` directly.

There is a dead/unused `app/services/serverInstance.ts` that shadows `useApi` via `useFetch` —
nothing imports it. Don't use it as a reference; it should eventually be deleted.

Requests go **directly cross-origin** to `runtimeConfig.public.apiBaseUrl`
(`NUXT_PUBLIC_API_BASE_URL` env var). The comment in `.env.example` about a `nitro.devProxy` is
stale — no such proxy exists in `nuxt.config.ts`.

**WebSocket (game)** — `useGameRoom()` (`app/composables/useGameRoom.ts`) wraps a SignalR
`HubConnection` to `${apiBaseUrl}/api/gamehub`. Server→client events (`RoomCreated`,
`PlayerJoined`, `QuestionStart`, `AnswerResult`, `GameEnd`, `RoomStateSync`, etc.) are routed
into `useGameStore()` mutators; client→server calls include `CreateRoom`, `StartGame`,
`SkipQuestion`, `SubmitAnswer`. Known gap: `PlayerLeft` sends SignalR connectionId not player
id, so `removePlayer` is currently a no-op stub (documented inline in the composable).

Before assuming a backend feature is wired to UI, check `docs/unconnected-apis.md` (endpoints
implemented but not yet used by any page — quiz management actions, the entire solo
quiz-attempt flow) and `docs/todo.md` (mock-data removal tracking).

## State management

All Pinia stores use **setup-store syntax** (`ref`/`computed`/actions returned as a flat
object) — follow this pattern for new stores, not the options API.

- `game.ts` — real-time multiplayer state; phase machine `idle → waiting → question → result →
  ended`; **the backend does not auto-advance phases** — the host client drives
  `skipQuestion()` via timers/watchers (see `host/room/[code].vue`). Not persisted.
- `auth.ts`, `quiz.ts` — persisted via `pinia-plugin-persistedstate` (selective `pick`, not the
  whole store)
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
- Write unit tests for new code going forward (note: no test framework is set up yet — this
  needs to be picked/added; see Commands above).
- Favor splitting into components, composables, or stores whenever it improves reuse and
  single-responsibility — do this proactively, not just when asked (see Component conventions
  above for the established pattern).
- Commit messages must be entirely in English, and must not include a co-author line/trailer.
