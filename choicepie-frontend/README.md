# ChoicePie Frontend

The web app for [ChoicePie](../README.md) — a real-time multiplayer quiz game and quiz library. Built with Nuxt 4, it also includes a Nitro server that acts as a BFF (backend-for-frontend) in front of the [.NET backend](../choicepie-backend/README.md).

## Tech stack

- **Nuxt 4** + Vue 3 (`<script setup lang="ts">`)
- **@nuxt/ui** + Tailwind CSS v4
- **Pinia** (setup stores) + pinia-plugin-persistedstate
- **@microsoft/signalr** for the live game
- **@nuxtjs/i18n** (`zh-TW` default, `en`)
- **@sentry/nuxt** (optional)
- **Vitest** + @nuxt/test-utils

## Getting started

Requires Node.js 22 and pnpm 10, plus a running [backend](../choicepie-backend/README.md).

```bash
cp .env.example .env
pnpm install
pnpm dev          # https://localhost:3000
```

The dev server uses HTTPS. Put a local certificate at `cert/key.pem` and `cert/cert.pem` (for example with `mkcert localhost`). The `cert/` folder is gitignored.

### Environment variables

| Variable | Description |
|---|---|
| `NUXT_PUBLIC_API_BASE_URL` | Backend address used **only** by SignalR (`/api/gamehub`). For local development: `http://localhost:5185` |
| `NUXT_BACKEND_API_URL` | Backend address used by the BFF (server-only). Defaults to the value above |
| `NUXT_PUBLIC_GOOGLE_CLIENT_ID` | Google Identity Services client ID |
| `NUXT_PUBLIC_SENTRY_DSN` | Leave empty to disable Sentry |
| `NUXT_TRUST_PROXY_HEADERS` | `true` only behind a trusted load balancer |
| `NUXT_DEBUG_BACKEND_FETCH` | Log BFF requests outside dev (credentials masked) |

See [.env.example](.env.example) for all options.

## Scripts

| Command | Description |
|---|---|
| `pnpm dev` | Start the dev server |
| `pnpm build` / `pnpm preview` | Production build and local preview |
| `pnpm lint` | ESLint (run in CI) |
| `pnpm typecheck` | `vue-tsc --noEmit` (run in CI) |
| `pnpm test` | Vitest unit tests (run in CI) |
| `pnpm test:watch` | Vitest in watch mode |

## How it talks to the backend

- **REST**: the browser calls same-origin `/api/v1/**` via `useApi()`. The Nitro server in `server/api/v1/` forwards these requests to the backend, handles auth cookies, and refreshes tokens.
- **Live game**: `useGameRoom()` opens a SignalR connection directly to `${NUXT_PUBLIC_API_BASE_URL}/api/gamehub`, authenticated by the backend's `HttpOnly` cookie.

## Project structure

```
app/
├── pages/          # File-based routes (join, host, bigscreen, library, attempt, history, admin, ...)
├── components/     # Feature-scoped folders (gameRoom/, library/, host/, admin/, ...)
├── stores/         # Pinia setup stores
├── composables/    # useApi, useGameRoom, useGoogleIdentity, usePlayerSession
├── services/       # Typed API clients per domain
├── types/          # Backend DTOs and SignalR payloads
├── layouts/        # default, content, admin, bigScreen
├── middleware/     # auth, admin-auth
└── assets/css/     # Design tokens and global styles
server/             # Nitro BFF (api/v1 routes + utils)
i18n/locales/       # zh-TW.json, en.json
tests/nuxt/unit/    # Vitest specs
docs/               # Feature history, unconnected APIs, debugging notes
```

## Further reading

- [CLAUDE.md](CLAUDE.md) — detailed architecture, routing, state management, and component conventions
- [docs/todo.md](docs/todo.md) — feature history and open items
- [docs/unconnected-apis.md](docs/unconnected-apis.md) — backend APIs without a frontend caller
- [CONTRIBUTING.md](../CONTRIBUTING.md) — workflow and coding guidelines
